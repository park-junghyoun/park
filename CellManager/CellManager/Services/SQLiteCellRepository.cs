using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using CellManager.Models;
using System.Globalization;
using System.Linq;

namespace CellManager.Services
{
    /// <summary>SQLite-backed implementation of <see cref="ICellRepository"/>.</summary>
    public class SQLiteCellRepository : ICellRepository
    {
        private const string DbFileName = "cell_library.db";
        private static readonly string DbFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CellManager", DbFileName);

        public SQLiteCellRepository()
        {
            InitializeDatabase();
        }

        /// <summary>Creates the cell database and upgrades schema as needed.</summary>
        private void InitializeDatabase()
        {
            try
            {
                string dataDirectoryPath = Path.GetDirectoryName(DbFilePath)!;
                if (!Directory.Exists(dataDirectoryPath))
                {
                    Directory.CreateDirectory(dataDirectoryPath);
                }

                if (!File.Exists(DbFilePath))
                {
                    SQLiteConnection.CreateFile(DbFilePath);
                }
                using (var conn = new SQLiteConnection($"Data Source={DbFilePath};Version=3;"))
                {
                    conn.Open();
                    string createCellsTableSql = @"
                        CREATE TABLE IF NOT EXISTS Cells (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ModelName TEXT NOT NULL,
                            Manufacturer TEXT,
                            SerialNumber TEXT,
                            PartNumber TEXT,
                            RatedCapacity REAL,
                            NominalVoltage REAL,
                            SelfDischarge REAL,
                            MaxVoltage REAL,
                            CycleLife INTEGER,
                            InitialACImpedance REAL,
                            InitialDCResistance REAL,
                            EnergyWh REAL,
                            CellType TEXT,
                            Weight REAL,
                            Diameter REAL,
                            Thickness REAL,
                            Width REAL,
                            Height REAL,
                            ExpansionBehavior TEXT,
                            ChargingVoltage REAL,
                            CutOffCurrent_Charge REAL,
                            MaxChargingCurrent REAL,
                            MaxChargingTemp REAL,
                            ChargeTempHigh REAL,
                            ChargeTempLow REAL,
                            DischargeCutOffVoltage REAL,
                            MaxDischargingCurrent REAL,
                            DischargeTempHigh REAL,
                            DischargeTempLow REAL,
                            ConstantCurrent_PreCharge REAL,
                            PreChargeStartVoltage REAL,
                            PreChargeEndVoltage REAL,
                            LastUpdated TEXT
                        );";
                    using (var cmd = new SQLiteCommand(createCellsTableSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Ensure LastUpdated column exists for older databases
                    using (var checkCmd = new SQLiteCommand("PRAGMA table_info(Cells);", conn))
                    using (var reader = checkCmd.ExecuteReader())
                    {
                        bool hasLastUpdated = false;
                        while (reader.Read())
                        {
                            if (reader["name"].ToString() == "LastUpdated")
                            {
                                hasLastUpdated = true;
                                break;
                            }
                        }
                        if (!hasLastUpdated)
                        {
                            using var alterCmd = new SQLiteCommand("ALTER TABLE Cells ADD COLUMN LastUpdated TEXT;", conn);
                            alterCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cell Database initialization failed: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public List<Cell> LoadCells()
        {
            var cells = new List<Cell>();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={DbFilePath};Version=3;"))
                {
                    conn.Open();
                    string sql = "SELECT * FROM Cells ORDER BY Id";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cells.Add(new Cell
                            {
                                Id = reader.GetInt32(0),
                                ModelName = reader.GetString(1),
                                Manufacturer = !reader.IsDBNull(2) ? reader.GetString(2) : string.Empty,
                                SerialNumber = !reader.IsDBNull(3) ? reader.GetString(3) : string.Empty,
                                PartNumber = !reader.IsDBNull(4) ? reader.GetString(4) : string.Empty,
                                RatedCapacity = !reader.IsDBNull(5) ? reader.GetDouble(5) : 0.0,
                                NominalVoltage = !reader.IsDBNull(6) ? reader.GetDouble(6) : 0.0,
                                SelfDischarge = !reader.IsDBNull(7) ? reader.GetDouble(7) : 0.0,
                                MaxVoltage = !reader.IsDBNull(8) ? reader.GetDouble(8) : 0.0,
                                CycleLife = !reader.IsDBNull(9) ? reader.GetInt32(9) : 0,
                                InitialACImpedance = !reader.IsDBNull(10) ? reader.GetDouble(10) : 0.0,
                                InitialDCResistance = !reader.IsDBNull(11) ? reader.GetDouble(11) : 0.0,
                                EnergyWh = !reader.IsDBNull(12) ? reader.GetDouble(12) : 0.0,
                                CellType = !reader.IsDBNull(13) ? reader.GetString(13) : string.Empty,
                                Weight = !reader.IsDBNull(14) ? reader.GetDouble(14) : 0.0,
                                Diameter = !reader.IsDBNull(15) ? reader.GetDouble(15) : 0.0,
                                Thickness = !reader.IsDBNull(16) ? reader.GetDouble(16) : 0.0,
                                Width = !reader.IsDBNull(17) ? reader.GetDouble(17) : 0.0,
                                Height = !reader.IsDBNull(18) ? reader.GetDouble(18) : 0.0,
                                ExpansionBehavior = !reader.IsDBNull(19) ? reader.GetString(19) : string.Empty,
                                ChargingVoltage = !reader.IsDBNull(20) ? reader.GetDouble(20) : 0.0,
                                CutOffCurrent_Charge = !reader.IsDBNull(21) ? reader.GetDouble(21) : 0.0,
                                MaxChargingCurrent = !reader.IsDBNull(22) ? reader.GetDouble(22) : 0.0,
                                MaxChargingTemp = !reader.IsDBNull(23) ? reader.GetDouble(23) : 0.0,
                                ChargeTempHigh = !reader.IsDBNull(24) ? reader.GetDouble(24) : 0.0,
                                ChargeTempLow = !reader.IsDBNull(25) ? reader.GetDouble(25) : 0.0,
                                DischargeCutOffVoltage = !reader.IsDBNull(26) ? reader.GetDouble(26) : 0.0,
                                MaxDischargingCurrent = !reader.IsDBNull(27) ? reader.GetDouble(27) : 0.0,
                                DischargeTempHigh = !reader.IsDBNull(28) ? reader.GetDouble(28) : 0.0,
                                DischargeTempLow = !reader.IsDBNull(29) ? reader.GetDouble(29) : 0.0,
                                ConstantCurrent_PreCharge = !reader.IsDBNull(30) ? reader.GetDouble(30) : 0.0,
                                PreChargeStartVoltage = !reader.IsDBNull(31) ? reader.GetDouble(31) : 0.0,
                                PreChargeEndVoltage = !reader.IsDBNull(32) ? reader.GetDouble(32) : 0.0,
                                LastUpdated = !reader.IsDBNull(33) ? DateTime.Parse(reader.GetString(33), CultureInfo.InvariantCulture) : DateTime.MinValue
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadCells error: {ex.Message}");
            }
            return cells;
        }

        /// <inheritdoc />
        public int GetNextCellId()
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={DbFilePath};Version=3;"))
                {
                    conn.Open();
                    using var cmd = new SQLiteCommand("SELECT IFNULL(MAX(Id), 0) + 1 FROM Cells;", conn);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetNextCellId error: {ex.Message}");
                return 0;
            }
        }

        /// <inheritdoc />
        public void SaveCell(Cell cell)
        {
            if (cell == null) throw new ArgumentNullException(nameof(cell));
            if (string.IsNullOrWhiteSpace(cell.ModelName)) throw new ArgumentException("Model name is required", nameof(cell));
            if (cell.RatedCapacity <= 0) throw new ArgumentException("Rated capacity must be greater than 0", nameof(cell));
            if (cell.NominalVoltage <= 0) throw new ArgumentException("Nominal voltage must be greater than 0", nameof(cell));

            try
            {
                using (var conn = new SQLiteConnection($"Data Source={DbFilePath};Version=3;"))
                {
                    conn.Open();

                    cell.LastUpdated = DateTime.Now;

                    if (cell.Id == 0)
                    {
                        string insertSql = @"
                            INSERT INTO Cells (
                                ModelName, Manufacturer, SerialNumber, PartNumber, RatedCapacity, NominalVoltage,
                                SelfDischarge, MaxVoltage, CycleLife, InitialACImpedance, InitialDCResistance,
                                EnergyWh, CellType, Weight, Diameter, Thickness, Width, Height,
                                ExpansionBehavior, ChargingVoltage, CutOffCurrent_Charge, MaxChargingCurrent,
                                MaxChargingTemp, ChargeTempHigh, ChargeTempLow, DischargeCutOffVoltage,
                                MaxDischargingCurrent, DischargeTempHigh, DischargeTempLow,
                                ConstantCurrent_PreCharge, PreChargeStartVoltage, PreChargeEndVoltage, LastUpdated
                            ) VALUES (
                                @ModelName, @Manufacturer, @SerialNumber, @PartNumber, @RatedCapacity, @NominalVoltage,
                                @SelfDischarge, @MaxVoltage, @CycleLife, @InitialACImpedance, @InitialDCResistance,
                                @EnergyWh, @CellType, @Weight, @Diameter, @Thickness, @Width, @Height,
                                @ExpansionBehavior, @ChargingVoltage, @CutOffCurrent_Charge, @MaxChargingCurrent,
                                @MaxChargingTemp, @ChargeTempHigh, @ChargeTempLow, @DischargeCutOffVoltage,
                                @MaxDischargingCurrent, @DischargeTempHigh, @DischargeTempLow,
                                @ConstantCurrent_PreCharge, @PreChargeStartVoltage, @PreChargeEndVoltage, @LastUpdated
                                )";
                        using (var cmd = new SQLiteCommand(insertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ModelName", cell.ModelName);
                            cmd.Parameters.AddWithValue("@Manufacturer", cell.Manufacturer ?? string.Empty);
                            cmd.Parameters.AddWithValue("@SerialNumber", cell.SerialNumber ?? string.Empty);
                            cmd.Parameters.AddWithValue("@PartNumber", cell.PartNumber ?? string.Empty);
                            cmd.Parameters.AddWithValue("@RatedCapacity", cell.RatedCapacity);
                            cmd.Parameters.AddWithValue("@NominalVoltage", cell.NominalVoltage);
                            cmd.Parameters.AddWithValue("@SelfDischarge", cell.SelfDischarge);
                            cmd.Parameters.AddWithValue("@MaxVoltage", cell.MaxVoltage);
                            cmd.Parameters.AddWithValue("@CycleLife", cell.CycleLife);
                            cmd.Parameters.AddWithValue("@InitialACImpedance", cell.InitialACImpedance);
                            cmd.Parameters.AddWithValue("@InitialDCResistance", cell.InitialDCResistance);
                            cmd.Parameters.AddWithValue("@EnergyWh", cell.EnergyWh);
                            cmd.Parameters.AddWithValue("@CellType", cell.CellType ?? string.Empty);
                            cmd.Parameters.AddWithValue("@Weight", cell.Weight);
                            cmd.Parameters.AddWithValue("@Diameter", cell.Diameter);
                            cmd.Parameters.AddWithValue("@Thickness", cell.Thickness);
                            cmd.Parameters.AddWithValue("@Width", cell.Width);
                            cmd.Parameters.AddWithValue("@Height", cell.Height);
                            cmd.Parameters.AddWithValue("@ExpansionBehavior", cell.ExpansionBehavior ?? string.Empty);
                            cmd.Parameters.AddWithValue("@ChargingVoltage", cell.ChargingVoltage);
                            cmd.Parameters.AddWithValue("@CutOffCurrent_Charge", cell.CutOffCurrent_Charge);
                            cmd.Parameters.AddWithValue("@MaxChargingCurrent", cell.MaxChargingCurrent);
                            cmd.Parameters.AddWithValue("@MaxChargingTemp", cell.MaxChargingTemp);
                            cmd.Parameters.AddWithValue("@ChargeTempHigh", cell.ChargeTempHigh);
                            cmd.Parameters.AddWithValue("@ChargeTempLow", cell.ChargeTempLow);
                            cmd.Parameters.AddWithValue("@DischargeCutOffVoltage", cell.DischargeCutOffVoltage);
                            cmd.Parameters.AddWithValue("@MaxDischargingCurrent", cell.MaxDischargingCurrent);
                            cmd.Parameters.AddWithValue("@DischargeTempHigh", cell.DischargeTempHigh);
                            cmd.Parameters.AddWithValue("@DischargeTempLow", cell.DischargeTempLow);
                            cmd.Parameters.AddWithValue("@ConstantCurrent_PreCharge", cell.ConstantCurrent_PreCharge);
                            cmd.Parameters.AddWithValue("@PreChargeStartVoltage", cell.PreChargeStartVoltage);
                            cmd.Parameters.AddWithValue("@PreChargeEndVoltage", cell.PreChargeEndVoltage);
                            cmd.Parameters.AddWithValue("@LastUpdated", cell.LastUpdated.ToString("o"));
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SQLiteCommand("SELECT last_insert_rowid();", conn))
                        {
                            cell.Id = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    else
                    {
                        const string updateSql = @"
                            UPDATE Cells SET
                                ModelName = @ModelName,
                                Manufacturer = @Manufacturer,
                                SerialNumber = @SerialNumber,
                                PartNumber = @PartNumber,
                                RatedCapacity = @RatedCapacity,
                                NominalVoltage = @NominalVoltage,
                                SelfDischarge = @SelfDischarge,
                                MaxVoltage = @MaxVoltage,
                                CycleLife = @CycleLife,
                                InitialACImpedance = @InitialACImpedance,
                                InitialDCResistance = @InitialDCResistance,
                                EnergyWh = @EnergyWh,
                                CellType = @CellType,
                                Weight = @Weight,
                                Diameter = @Diameter,
                                Thickness = @Thickness,
                                Width = @Width,
                                Height = @Height,
                                ExpansionBehavior = @ExpansionBehavior,
                                ChargingVoltage = @ChargingVoltage,
                                CutOffCurrent_Charge = @CutOffCurrent_Charge,
                                MaxChargingCurrent = @MaxChargingCurrent,
                                MaxChargingTemp = @MaxChargingTemp,
                                ChargeTempHigh = @ChargeTempHigh,
                                ChargeTempLow = @ChargeTempLow,
                                DischargeCutOffVoltage = @DischargeCutOffVoltage,
                                MaxDischargingCurrent = @MaxDischargingCurrent,
                                DischargeTempHigh = @DischargeTempHigh,
                                DischargeTempLow = @DischargeTempLow,
                                ConstantCurrent_PreCharge = @ConstantCurrent_PreCharge,
                                PreChargeStartVoltage = @PreChargeStartVoltage,
                                PreChargeEndVoltage = @PreChargeEndVoltage,
                                LastUpdated = @LastUpdated
                            WHERE Id = @Id;
                            ";

                        using (var cmd = new SQLiteCommand(updateSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ModelName", cell.ModelName);
                            cmd.Parameters.AddWithValue("@Manufacturer", cell.Manufacturer);
                            cmd.Parameters.AddWithValue("@SerialNumber", cell.SerialNumber);
                            cmd.Parameters.AddWithValue("@PartNumber", cell.PartNumber);
                            cmd.Parameters.AddWithValue("@RatedCapacity", cell.RatedCapacity);
                            cmd.Parameters.AddWithValue("@NominalVoltage", cell.NominalVoltage);
                            cmd.Parameters.AddWithValue("@SelfDischarge", cell.SelfDischarge);
                            cmd.Parameters.AddWithValue("@MaxVoltage", cell.MaxVoltage);
                            cmd.Parameters.AddWithValue("@CycleLife", cell.CycleLife);
                            cmd.Parameters.AddWithValue("@InitialACImpedance", cell.InitialACImpedance);
                            cmd.Parameters.AddWithValue("@InitialDCResistance", cell.InitialDCResistance);
                            cmd.Parameters.AddWithValue("@EnergyWh", cell.EnergyWh);
                            cmd.Parameters.AddWithValue("@CellType", cell.CellType);
                            cmd.Parameters.AddWithValue("@Weight", cell.Weight);
                            cmd.Parameters.AddWithValue("@Diameter", cell.Diameter);
                            cmd.Parameters.AddWithValue("@Thickness", cell.Thickness);
                            cmd.Parameters.AddWithValue("@Width", cell.Width);
                            cmd.Parameters.AddWithValue("@Height", cell.Height);
                            cmd.Parameters.AddWithValue("@ExpansionBehavior", cell.ExpansionBehavior);
                            cmd.Parameters.AddWithValue("@ChargingVoltage", cell.ChargingVoltage);
                            cmd.Parameters.AddWithValue("@CutOffCurrent_Charge", cell.CutOffCurrent_Charge);
                            cmd.Parameters.AddWithValue("@MaxChargingCurrent", cell.MaxChargingCurrent);
                            cmd.Parameters.AddWithValue("@MaxChargingTemp", cell.MaxChargingTemp);
                            cmd.Parameters.AddWithValue("@ChargeTempHigh", cell.ChargeTempHigh);
                            cmd.Parameters.AddWithValue("@ChargeTempLow", cell.ChargeTempLow);
                            cmd.Parameters.AddWithValue("@DischargeCutOffVoltage", cell.DischargeCutOffVoltage);
                            cmd.Parameters.AddWithValue("@MaxDischargingCurrent", cell.MaxDischargingCurrent);
                            cmd.Parameters.AddWithValue("@DischargeTempHigh", cell.DischargeTempHigh);
                            cmd.Parameters.AddWithValue("@DischargeTempLow", cell.DischargeTempLow);
                            cmd.Parameters.AddWithValue("@ConstantCurrent_PreCharge", cell.ConstantCurrent_PreCharge);
                            cmd.Parameters.AddWithValue("@PreChargeStartVoltage", cell.PreChargeStartVoltage);
                            cmd.Parameters.AddWithValue("@PreChargeEndVoltage", cell.PreChargeEndVoltage);
                            cmd.Parameters.AddWithValue("@LastUpdated", cell.LastUpdated.ToString("o"));
                            cmd.Parameters.AddWithValue("@Id", cell.Id);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveCell error: {ex.Message}");
            }
        }

        public void DeleteCell(Cell cell)
        {
            try
            {
                if (cell == null || cell.Id <= 0)
                {
                    Console.WriteLine("Error: Invalid cell provided for deletion.");
                    return;
                }

                using (var conn = new SQLiteConnection($"Data Source={DbFilePath};Version=3;"))
                {
                    conn.Open();
                    string sql = "DELETE FROM Cells WHERE Id = @Id";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", cell.Id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Successfully deleted {rowsAffected} row(s) for Cell ID: {cell.Id}");
                        }
                        else
                        {
                            Console.WriteLine($"No rows were deleted for Cell ID: {cell.Id}. This might indicate the ID does not exist in the database.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete operation failed: {ex.Message}");
            }
        }
    }
}