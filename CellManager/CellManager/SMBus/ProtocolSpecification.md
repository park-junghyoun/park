# Board Communication Protocol Design

## 1. Scope
This specification defines the logical protocol used by the CellManager application to communicate with the battery test board. It introduces a transport-neutral command dictionary and payload structure, then details the first concrete transport binding (SMBus via `CommControl`). Future interfaces—such as USB CDC/HID, TCP/IP, or BLE—can reuse the logical layer while supplying their own physical/link characteristics.

## 2. Layered Transport Strategy

| Layer | Responsibility | Notes |
|-------|----------------|-------|
| **Board Protocol Client** | Serialises high-level requests (`ExecuteProfile`, `GetSystemStatus`, etc.) into canonical payloads and interprets responses. | Transport-agnostic; pure business logic. |
| **Transport Adapter** | Provides framed request/response I/O and maps link errors to protocol status. | First adapter is SMBus via `CommControl`; future adapters can target USB, TCP, BLE, etc. |
| **Physical Link** | Electrical and low-level timing characteristics. | SMBus today; alternatives may bring higher throughput or longer reach. |

The protocol client communicates with any transport implementation through an interface such as:

```csharp
public interface IBoardTransport
{
    Task<TransportResponse> ExchangeAsync(CommandCode command,
                                          ReadOnlyMemory<byte> payload,
                                          CancellationToken ct);
    TransportFeatures Features { get; }
}
```

Each adapter is responsible for honoring its link’s framing rules, timeouts, retry semantics, and integrity checks while presenting the same `[status][seq][data…]` envelope to upper layers. This separation keeps host logic identical regardless of the underlying medium.

## 3. Transport Binding: SMBus via CommControl
- **Bus Type**: SMBus 2.0 compliant, operating in master/slave mode (PC = master, board = slave).
- **Physical Speed**: Standard mode (100 kHz) required; firmware may opt-in to 400 kHz once validated.
- **Transactions**: Only SMBus Word and Block primitives are used so the implementation can reuse `CommControl` helpers:
  - `WriteWord_Cs` / `ReadWord_Cs` for simple control/status words (≤ 2 bytes).
  - `WriteBlock_Cs` / `ReadBlock_Cs` / combined Write-Read for longer payloads (≤ 160 bytes including PEC), matching `ga_VARIABLE_COM_MAX_SIZE` in the firmware.
- **PEC**: Packet Error Code (CRC-8 ATM) is mandatory on every multi-byte transfer. The host recalculates PEC to validate responses and retries on mismatch.
- **Clock Stretching**: The board may stretch the clock up to 20 ms while assembling responses; host timeouts accommodate this.

An alternative transport would implement its own framing (e.g., `[Command][Seq][Length][Payload][CRC16]` inside a USB HID report) yet still expose the same decoded payload to the protocol client.

## 4. Message Envelope
Every logical command shares a consistent envelope so higher layers can parse uniformly, regardless of transport. The SMBus binding packs these fields into the `Command`/`Length`/`Payload` bytes issued via `CommControl`.

| Field         | Size | Notes                                                                                       |
|---------------|------|---------------------------------------------------------------------------------------------|
| Command Code  | 1 B  | Identifies the operation (ranges reserved per section 5).                                   |
| Length (L)    | 1 B  | Number of payload bytes following (0–158).                                                  |
| Payload       | L B  | Command-specific data. First byte is often `seq` (host-generated sequence number).          |
| Integrity     | 1–2 B| Link-specific checksum: PEC for SMBus; future transports may use CRC16/CRC32.               |

Responses prepend a status byte inside the payload:

```
Payload := [status:u8][seq:u8 (if request carried seq)][data...]
```

`status` values:

| Code | Meaning             | Handling                                                                 |
|------|---------------------|--------------------------------------------------------------------------|
| 0x00 | Success             | Proceed; data section is valid.                                          |
| 0x01 | Invalid Parameter   | Host validates inputs and prompts user.                                  |
| 0x02 | Busy                | Board occupied; host waits ≥250 ms and retries with same `seq`.          |
| 0x03 | Unsupported Command | Host hides feature or prompts for firmware upgrade.                      |
| 0x04 | Denied by State     | Command not allowed in current mode/state (e.g., trying to run while paused). |
| 0x05 | Integrity Error     | Board detected malformed payload or checksum mismatch; host retries.     |
| 0x10 | Internal Fault      | Board logged a fault; host surfaces event log.                           |

## 5. Command Map
The following high-level command families reflect the current UI workflow (configuration, execution, telemetry, data retrieval, and global control). The SMBus column indicates the recommended primitive today; other transports can stream the same payload without change.

### 5.1 Test Profile Configuration (0x30–0x3F)
| Command Code | SMBus Primitive | Purpose                                                                              | Request Payload (host → board)                                                                          | Response Payload (board → host)                                                   |
|--------------|-----------------|--------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------|
| 0x30 `SET_PROFILE_CONFIG` | Block Write      | Push charge/discharge/rest/OCV/ECM parameters to firmware.    | `[seq][profile_type][profile_id_lo][profile_id_hi][revision][record_count][TLV records...]`              | `[status][seq][applied_revision]`                                                 |
| 0x31 `GET_PROFILE_CONFIG` | Block Write-Read | Read back stored parameters for the selected profile.    | `[seq][profile_type][profile_id_lo][profile_id_hi]`                                                      | `[status][seq][revision][record_count][TLV records...]`                           |

**TLV Structure**: `[tag:u8][len:u8][value:len]`, with tags mapped to profile-specific fields (e.g., voltage mV, current mA, timeouts, comments). Little-endian applies to multi-byte values.

### 5.2 Test Execution (0x40–0x4F)
| Command Code | SMBus Primitive | Purpose                                                   | Request Payload                               | Response Payload                                  |
|--------------|-----------------|-----------------------------------------------------------|-----------------------------------------------|---------------------------------------------------|
| 0x40 `EXECUTE_PROFILE` | Block Write      | Kick off the selected profile when system is ready.     | `[seq][profile_type][profile_id_lo][profile_id_hi][repeat_lo][repeat_hi][loop_start][loop_end]` | `[status][seq][scheduled_slot][system_mode]`      |
| 0x41 `PAUSE_TEST`      | Word Write       | Pause current execution.                                | `[seq<<8 | 0x0001]` (seq in high byte)        | `[status][seq][test_state]` (returned via follow-up status query) |
| 0x42 `RESUME_TEST`     | Word Write       | Resume from paused state.                               | `[seq<<8 | 0x0002]`                            | `[status][seq][test_state]`                                   |
| 0x43 `ABORT_TEST`      | Block Write      | Abort execution immediately.                             | `[seq][reason]`                                | `[status][seq][test_state]`                                   |
| 0x44 `RESET_FAULT_STATE` | Word Write     | Clear fault condition and return to standby.            | `[seq<<8 | 0x0003]`                            | `[status][seq][system_mode]`                                 |

Word writes encode a one-byte command discriminator in the low byte and pack the host `seq` in the high byte for correlation; the board mirrors the `seq` in subsequent status responses.

### 5.3 Status & Telemetry (0x10–0x1F)
| Command Code | SMBus Primitive | Purpose                                             | Request Payload | Response Payload |
|--------------|-----------------|-----------------------------------------------------|-----------------|-----------------------------------------------------------------------------------------------------------------------------------|
| 0x10 `GET_SYSTEM_STATUS` | Block Write-Read | Snapshot of board mode/state/fault flags.           | `[seq]`         | `[status][seq][system_mode][test_state][fault_flags_lo][fault_flags_hi][supply_mv_lo][supply_mv_hi][board_temp_d1c_lo][board_temp_d1c_hi][timestamp_ms:4]` |
| 0x11 `GET_MEASUREMENTS` | Block Write-Read | Live telemetry (voltage/current/temperature).      | `[seq]`         | `[status][seq][cell_voltage_mv_lo][cell_voltage_mv_hi][cell_current_ma_lo][cell_current_ma_hi][ambient_temp_d1c_lo][ambient_temp_d1c_hi][soc_d1pct_lo][soc_d1pct_hi][soe_d1pct_lo][soe_d1pct_hi][timestamp_ms:4]` |
| 0x12 `GET_RUN_STATUS`   | Block Write-Read | Execution progress for UI timeline.                | `[seq]`         | `[status][seq][active_profile_type][active_profile_id_lo][active_profile_id_hi][step_index][step_elapsed_ms:4][step_remaining_ms:4][total_elapsed_ms:4][loop_depth][loop_iter_lo][loop_iter_hi]` |

Polling cadence: `GET_SYSTEM_STATUS` and `GET_MEASUREMENTS` every 200 ms (staggered) to keep UI responsive without saturating the bus.

### 5.4 Data Fetch (0x50–0x5F)
| Command Code | SMBus Primitive | Purpose                                                           | Request Payload                                                               | Response Payload                                                                                |
|--------------|-----------------|-------------------------------------------------------------------|-------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------|
| 0x50 `GET_LAST_TEST_RESULT` | Block Read      | Retrieve summary metrics of the most recent completed test.      | —                                                                             | `[status][seq][profile_type][profile_id_lo][profile_id_hi][completion_state][cycle_count_lo][cycle_count_hi][charge_mAh:4][discharge_mAh:4][energy_mWh:4][max_temp_d1c_lo][max_temp_d1c_hi]` |
| 0x51 `FETCH_DATA_BLOCK`    | Block Write-Read | Stream detailed measurement/log samples block-by-block.          | `[seq][stream_id][block_index_lo][block_index_hi][max_bytes]`                  | `[status][seq][total_blocks_lo][total_blocks_hi][payload_len][payload…][crc16_lo][crc16_hi]`                                                    |
| 0x52 `GET_OCV_TABLE_DATA`  | Block Write-Read | Pull OCV lookup table produced after OCV profile completion.     | `[seq][table_revision]`                                                       | `[status][seq][point_count][points…]` where each point = `{voltage_mv:2, soc_d1pct:2, rest_time_s:2}`                                           |

`FETCH_DATA_BLOCK` supports multiple logical streams (e.g., 0 = telemetry decimated, 1 = detailed samples, 2 = raw ADC). Host iterates `block_index` until it reaches `total_blocks - 1`.

### 5.5 Top-Level Control (0x20–0x2F)
| Command Code | SMBus Primitive | Purpose                                            | Request Payload        | Response Payload                      |
|--------------|-----------------|----------------------------------------------------|------------------------|---------------------------------------|
| 0x20 `SET_SYSTEM_MODE` | Block Write      | Switch between MODE_STANDBY and MODE_CALIBRATION. | `[seq][target_mode][auth_token_lo][auth_token_hi]` | `[status][seq][system_mode]`         |
| 0x21 `KEEP_ALIVE`       | Block Write-Read | Maintain session, exchange uptime.                | `[seq][host_uptime_ms:4]`               | `[status][seq][board_uptime_ms:4][last_error_code]` |

`SET_SYSTEM_MODE` rejects transitions while tests are running (`status=0x04`). `auth_token` allows future locking of critical transitions; set to zero if unused.

## 6. Firmware Modes & States
The board exposes two primary enums; the UI already binds to these.

### 6.1 `system_mode`
| Value | Description                          |
|-------|--------------------------------------|
| 0x00  | MODE_BOOT – Firmware initialization. |
| 0x01  | MODE_STANDBY – Idle, ready for commands. |
| 0x02  | MODE_OPERATION – Test executing/post-processing. |
| 0x03  | MODE_CALIBRATION – Calibration utilities active. |
| 0x04  | MODE_SHUTDOWN – Preparing for power off. |

### 6.2 `test_state`
| Value | Description                                          |
|-------|------------------------------------------------------|
| 0x00  | STATE_IDLE – No profile scheduled.                    |
| 0x01  | STATE_RUNNING – Profile executing.                    |
| 0x02  | STATE_PAUSED – Execution paused by host.             |
| 0x03  | STATE_COMPLETE – Last profile finished normally.     |
| 0x04  | STATE_FAULT – Execution stopped due to fault.        |
| 0x05  | STATE_ABORTED – Host issued abort.                   |

State transitions are driven by the execution commands above. `RESET_FAULT_STATE` returns `(system_mode=MODE_STANDBY, test_state=STATE_IDLE)` when successful.

## 7. Initialization & Runtime Flow
1. **Port Handshake**
   - Host opens serial interface via `CommControl.OpenSerial_Cs`.
   - Issue `WHO_AM_I` (0x00, block read) until success to allow firmware boot latency; response includes ASCII board ID and firmware version string.
   - Query `PROTOCOL_VERSION` (0x01) and `GET_CAPS` (0x02) to detect optional features and maximum block length.

2. **Monitoring Loop**
   - Alternate `GET_SYSTEM_STATUS` and `GET_MEASUREMENTS` every 200 ms.
   - Dispatch `KEEP_ALIVE` every 2 s; if it times out twice consecutively, close and reopen the port.

3. **Profile Management**
   - For each profile type (Charge, Discharge, Rest, OCV, ECM), the UI edits TLV records locally then sends `SET_PROFILE_CONFIG` when user saves.
   - On firmware change or app startup, call `GET_PROFILE_CONFIG` for each known profile to hydrate UI controls.

4. **Execution**
   - Ensure `(system_mode=MODE_STANDBY, test_state=STATE_IDLE)` before sending `EXECUTE_PROFILE`.
   - While running, poll `GET_RUN_STATUS` alongside telemetry to update progress bars and estimated time remaining.
   - Upon completion/fault/abort, call `GET_LAST_TEST_RESULT` and, if needed, iterate `FETCH_DATA_BLOCK` to retrieve detailed datasets.

5. **Error Recovery**
   - For `status=0x02` (Busy), exponential back-off starting at 250 ms.
   - For `status >= 0x03`, surface message to user and re-sync via `GET_SYSTEM_STATUS`.
   - If the transport reports a framing/timeout error, close the adapter, reopen, and restart handshake. Alternate transports should offer equivalent recovery hooks.

## 8. Extensibility Considerations
- Reserve command ranges (0x60–0x6F for board/protection settings, 0x70–0x7F for diagnostics/event logs) for future documents.
- Include a `capability_bitmap` in `GET_CAPS` so firmware can advertise optional features (e.g., ECM support, extended block size, high-speed SMBus, alternate transport support).
- All new commands should adhere to the same `[status][seq][data…]` response pattern to keep host-side parsing uniform.
- When introducing a new transport, expose its capabilities (maximum payload, latency expectations, checksum requirements) through the adapter so the protocol client can tune retries and batching without touching command logic.

## 9. Next Steps
- Define exact TLV tag catalogue for each profile type (enumerate required/optional parameters with units and ranges).
- Specify event log and protection-setting command payloads to finish the remaining command ranges.
- Prototype transactions using a loopback or firmware stub to validate `CommControl` wrapper compatibility and PEC handling.
- Design a proof-of-concept USB (CDC or HID) or TCP transport adapter to confirm that the layered architecture supports multiple physical links without altering the command dictionary.
