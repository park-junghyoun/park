using System;
using System.Threading;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace RBMS_Tool.BusinessLogics
{
    public class CommControl
    {
        #region commcontrolVariables

        // Battcom.dllにアクセスするためのClassをインスタンス化
        private static readonly WrapperBattcom WrapBatt = new WrapperBattcom();

        /* 固定値 */
        // 消去時の最大時間
        private const long ml_ERASE_TIME_MAX = 9000;
        //通信最大長
        public const int ga_VARIABLE_COM_MAX_SIZE = 160;

        /* Enum */
        public enum Result
        {
            COMM_SUCCESS,
            COMM_FAILURE,
            DETECT_COM_PORT,
            NOT_DETECT_COM_PORT,
            FAILED_TO_OPEN_COM_PORT,
            OPERATION_CANCELED,
            OTHER_ERROR,
            CATCH_EXCEPTION
        }

        public enum OP_MODE
        {
            NORMAL,
            FLASH_UP,
            PROTECTED,
            ERROR,
            MAX_NUM
        }

        #endregion

        // wait関数
        public static async Task Wait_time(int wait_time)
        {
            //            Console.WriteLine("Wait\n");
            await Task.Delay(wait_time);
        }

        // WrapperBattcom.Result を CommCtrl.Resultに変換する
        private static Result ConvertResult(WrapperBattcom.Result wrap_bat_ret)
        {
            Result result;

            switch (wrap_bat_ret)
            {
                case WrapperBattcom.Result.OK:
                    result = Result.COMM_SUCCESS;
                    break;
                case WrapperBattcom.Result.FAILED_TO_OPEN_COM_PORT:
                    result = Result.FAILED_TO_OPEN_COM_PORT;
                    break;
                case WrapperBattcom.Result.COM_ERROR:
                    result = Result.COMM_FAILURE;
                    break;
                case WrapperBattcom.Result.CATCH_EXCEPTION:
                    result = Result.CATCH_EXCEPTION;
                    break;
                default:
                    result = Result.OTHER_ERROR;
                    break;
            }
            return result;
        }

        // Battcomにある関数の外向けアクセス関数
        // Read Word
        public static Result ReadWord_Cs(int slave_adr, int command, ref int value, ref long battcom_ret)
        {
            return ConvertResult(WrapBatt.Wrapper_ReadWord(slave_adr, command, 0, ref value, ref battcom_ret));
        }

        // Write Word
        public static Result WriteWord_Cs(int slave_adr, int command, int value)
        {
            return ConvertResult(WrapBatt.Wrapper_WriteWord(slave_adr, command, 0, value));
        }

        // Read Block
        public static Result ReadBlock_Cs(int slave_adr, int command, ref byte[] Buffer, ref int length, ref long pecval)
        {
            return ConvertResult(WrapBatt.Wrapper_ReadBlock(slave_adr, command, 0, Buffer, ref length, ref pecval));
        }

        // Write Block
        public static Result WriteBlock_Cs(int slave_adr, int command, char[] Buffer, int length)
        {
            return ConvertResult(WrapBatt.Wrapper_WriteBlock(slave_adr, command, 0, Buffer, length));
        }

        // Page Read Binary
        public static bool PageReadBinary_Cs(int adr, ref byte Buffer, int read_len, bool enable_pec)
        {
            bool bret = true;

            if (Buffer.GetType().IsArray)
            {
                bret = false;
            }
            else if (read_len < 1)
            {
                bret = false;
            }
            else
            {
                // データ読み込み要求を送信
                if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_FlashRead_M(adr, read_len, enable_pec))
                {
                    // データが長い場合はウェイト
                    if (read_len > 64)
                    {
                        _ = Task.Delay(2);
                    }

                    // データを読み込む
                    if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_FlashRead_2(ref Buffer, read_len, enable_pec))
                    {
                        bret = true;
                    }
                    else
                    {
                        bret = false;
                    }
                }
                else
                {
                    bret = false;
                }
            }
            return bret;
        }

        // Page Wrtite Binary
        public static bool PageWriteBinary_Cs(int adr, ref byte Data, int write_len, bool enable_pec)
        {
            bool bret = true;

            if (WrapperBattcom.Result.OK != WrapBatt.Wrapper_FlashWrite_M(adr, ref Data, write_len, enable_pec))
            {
                bret = false;
            }

            return bret;
        }

        // ステータスクリア
        public static bool ClearStatus(int slave_address)
        {
            bool bret = false;
            char[] Buffer = new char[2];
            if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_WriteBlock(slave_address, 0x50, 0, Buffer, 0))
            {
                bret = true;
            }
            return bret;
        }

        // ステータスチェック
        public static bool ReadStatus(int slave_address, ref byte[] status_buf)
        {
            bool bret = true;
            long pecval = 0;
            int com_length = 0;
            byte[] Buffer = new byte[128];

            if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_ReadBlock(slave_address, 0x70, 0, Buffer, ref com_length, ref pecval))
            {
                if (com_length != 1)
                {
                    bret = false;
                }
                else
                {
                    bret = true;
                    status_buf[0] = Buffer[0];
                }
            }
            else
            {
                bret = false;
            }
            return bret;
        }

        // Open Serial
        public static Result OpenSerial_Cs(byte[] com_port)
        {
            return ConvertResult(WrapBatt.Wrapper_OpenSerial(com_port));
        }

        // Close  Serial
        public static Result CloseSerial_Cs()
        {
            return ConvertResult(WrapBatt.Wrapper_CloseSerial());
        }

        // RDM基板とM3A-7539基板を両方チェック
        public static Result SearchPorts_Cs(ref string[] port_name, ref int port_num_total)
        {
            Result result = Result.NOT_DETECT_COM_PORT;
            // SearchPorts_CsへのRef値
            port_name = new string[16];
            port_num_total = 0;

            // Wrapperからの引数
            string[] port_name_tmp = new string[16];
            int port_num_tmp = 0;
            int port_num_rdm = 0;

            // Wrapper for Battcom Classを通じて、RDM基板をサーチ
            if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_SearchPorts("023E", ref port_name_tmp, ref port_num_rdm))
            {
                for (int cnt = 0; cnt < port_num_rdm; cnt++)
                {
                    port_name[cnt] = port_name_tmp[cnt];
                }
                //見つかった数をTotalに格納
                port_num_total = port_num_rdm;
            }

            // Wrapper for Battcom Classを通じて、M3A-7539をサーチ
            if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_SearchPorts_7539(ref port_name_tmp, ref port_num_tmp))
            {
                for (int cnt = 0; cnt < port_num_tmp; cnt++)
                {
                    port_name[port_num_rdm + cnt] = port_name_tmp[cnt];
                }
                //見つかった数を返す
                port_num_total += port_num_tmp;
            }

            if (port_num_total == 0)
            {
                result = Result.NOT_DETECT_COM_PORT;
            }
            else
            {
                result = Result.DETECT_COM_PORT;
            }
            return result;
        }

        // コード領域消去
        public static bool BlockErase_CodeROM_Cs(int slave_address, byte start_block, byte end_block)
        {
            bool bret;
            byte erase_ret;

            bret = BlockEraseExec_CodeROM(slave_address, start_block, end_block);

            // 失敗？
            if (bret == false)
            {


                // ちょっとWait
                //Task.Delay(200);
                // Task waitが終了するまでここで待つ
                Task task_wait = Task.Run(() => Wait_time(200));
                while (task_wait.IsCompleted != true) { };

                // 再実行
                bret = BlockEraseExec_CodeROM(slave_address, start_block, end_block);
            }

            // 成功？
            if (bret == true)
            {
                // 確認
                erase_ret = EraseWait(slave_address);
                if (erase_ret != 0)
                {
                    bret = false;
                }
            }

            return bret;
        }

        // コード領域消去(Wrapperへのアクセス）
        private static bool BlockEraseExec_CodeROM(int slave_address, byte start_block, byte end_block)
        {
            bool bret = false;
            char[] tmp_data = new char[10];
            string ascii_data;
            byte[] status = new byte[64];

            // Format変換
            ascii_data = string.Format("{0,0:X2}{1,0:X2}", start_block, end_block);

            // string -> char変換
            tmp_data = ascii_data.ToCharArray();

            // 成功？
            if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_WriteBlock(slave_address, 0x20, 0, tmp_data, 2))
            {
                bret = true;
            }
            else
            {
                //              bret = ReadStatus(ref status);
            }

            return bret;
        }

        // DataFlash領域消去
        public static bool BlockErase_DataFlash_Cs(int slave_address, byte block_no)
        {
            bool bret;
            byte erase_ret;

            // 消去実行
            bret = BlockEraesExec_DataFlash(slave_address, block_no);

            // 失敗？
            if (bret == false)
            {
                // ちょっとWait
                //Task.Delay(200);
                // Task waitが終了するまでここで待つ
                Task task_wait = Task.Run(() => Wait_time(200));
                while (task_wait.IsCompleted != true) { };
                // 再実行
                bret = BlockEraesExec_DataFlash(slave_address, block_no);
            }

            // 成功？
            if (bret == true)
            {
                // 確認
                erase_ret = EraseWait(slave_address);
                if (erase_ret != 0)
                {
                    bret = false;
                }
            }
            return bret;
        }

        // DataFlash領域消去 (Wrapperへのアクセス）
        private static bool BlockEraesExec_DataFlash(int slave_address, byte block_no)
        {
            bool bret = false;
            char[] tmp_data = new char[10];
            string ascii_data;

            // Format変換
            ascii_data = string.Format("{0,0:X2}", block_no);

            // string -> char変換
            tmp_data = ascii_data.ToCharArray();

            //  成功？
            if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_WriteBlock(slave_address, 0x21, 0, tmp_data, 1))
            {
                bret = true;
            }
            return bret;
        }

        // 消去待ち
        private static Byte EraseWait(int slave_address)
        {
            const int sleep_time = 10;
            int time_cnt;
            byte[] tmp_data = new byte[10];

            for (time_cnt = 0; time_cnt < ml_ERASE_TIME_MAX; time_cnt += sleep_time)
            {
                // 若干Wait
                //Task.Delay(sleep_time);
                // Task waitが終了するまでここで待つ
                Task task_wait = Task.Run(() => Wait_time(sleep_time));
                while (task_wait.IsCompleted != true) { };

                // Status確認
                if (ReadStatus(slave_address, ref tmp_data) == true)
                {
                    break;
                }
                tmp_data[0] = 0xFF;
            }

            return tmp_data[0];
        }

        // Escape From Update
        public static bool EscapeFromUpdate(int slave_address)
        {
            bool bret = true;
            char[] Buffer = new char[2] { 'D', '0' };

            if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_WriteBlock(slave_address, 0x00, 0, Buffer, 1))
            {
                bret = true;
            }
            else
            {
                bret = false;
            }
            return bret;
        }

        // 通信基板バージョン情報の取得
        public bool GetBoardVersion_Cs(byte[] port, ref string version)
        {
            bool bRet = false;

            char char_version;
            byte[] byte_version = new byte[64];

            // 成功？
            if (WrapperBattcom.Result.OK == WrapBatt.Wrapper_GetBoardVersion(port, byte_version))
            {
                bRet = true;

                // byte -> char -> string
                for (int cnt = 0; cnt < byte_version.Length; cnt++)
                {
                    char_version = Convert.ToChar(byte_version[cnt]);

                    // nullだったら終了する
                    if (char_version == 0x00)
                    {
                        break;
                    }
                    version += char_version;
                }
            }

            return bRet;
        }

        public void OutPortB_Cs(ref byte Buffer, int ALen)
        {
            WrapBatt.Wrapper_OutPortB(ref Buffer, ALen);
        }

        public static Result GetSMBusBootVersion(ref int version)
        {
            Result com_ret;

            int ver;
            byte[] Buffer = new byte[256];
            int len = 0;
            int cnt;
            long pecval = 0;

            com_ret = ReadBlock_Cs(0x01, 0x55, ref Buffer, ref len, ref pecval);

            if (com_ret == Result.COMM_SUCCESS)
            {
                //com_ret = Serial.ReadBlock_Cs(READ_CTRL_BLOCK, BOOT_S_ADR, 0x55, (long*)com_buf, &com_length, &pec);

                ver = 0;

                /* Boot Versionによってサイズが異なるので、Loopで回す */
                for (cnt = 0; cnt < len; cnt++)
                {
                    ver |= (Buffer[cnt] << (cnt * 8));
                }

                version = ver;
            }
            return com_ret;
        }

        // DLLアクセスするためのクラス
        private class WrapperBattcom
        {
            public enum Result
            {
                OK,
                COM_ERROR,
                FAILED_TO_OPEN_COM_PORT,
                OTHER_ERROR,

                CATCH_EXCEPTION
            }

            // COM_ERROR
            public const int COM_ERROR_DRV = 0x10000000;

            [DllImport("Battcom.dll")]
            static extern void SearchPorts_7539(IntPtr[] DeviceDesc, ref int nnum);

            [DllImport("Battcom.dll")]
            static extern void SearchPorts(string adevname, IntPtr[] DeviceDesc, ref int nnum);

            [DllImport("Battcom.dll")]
            static extern int ReadBlock(int DevAddr, int DevCmd, int hWnd, byte[] Buffer, ref int Len);

            [DllImport("Battcom.dll")]
            static extern int WriteBlock(int DevAddr, int DevCmd, int hWnd, char[] Buffer, int Len);

            [DllImport("Battcom.dll")]
            static extern int ReadWord(int DevAddr, int DevCmd, int hWnd, ref int lpValue);

            [DllImport("Battcom.dll")]
            static extern int WriteWord(int DevAAdr, int DecCmd, int hWnd, int lValue);

            [DllImport("Battcom.dll")]
            static extern bool OpenSerial(byte[] Buffer);

            [DllImport("Battcom.dll")]
            static extern void CloseSerial();

            [DllImport("Battcom.dll")]
            static extern int FlashRead_M(int LAdr, int ALen);

            [DllImport("Battcom.dll")]
            static extern int FlashRead_2(ref byte Buffer, int ALen);

            [DllImport("Battcom.dll")]
            static extern int FlashWrite_M(int LAdr, ref byte Data, int ALen);

            [DllImport("Battcom.dll")]
            static extern void SetPECEnable(char enable);

            [DllImport("Battcom.dll")]
            static extern int SetClock();

            [DllImport("Battcom.dll")]
            static extern int GetBoardVersion(byte[] port, byte[] version, int size);

            [DllImport("Battcom.dll")]
            static extern int FlashRead_M_PEC(int LAdr, int ALene);

            [DllImport("Battcom.dll")]
            static extern int FlashWrite_M_PEC(int LAdr, ref byte Data, int ALen);

            [DllImport("Battcom.dll")]
            static extern int FlashRead_2_PEC(ref byte Buffer, int ALen);

            [DllImport("Battcom.dll")]
            static extern int OutPortB(ref byte Buffer, int ALen);

            // Serialを開く関数
            public Result Wrapper_OpenSerial(byte[] Buffer)
            {
                Result ret;
                bool battcom_ret;

                try
                {
                    // Access to Battcom.dll
                    battcom_ret = OpenSerial(Buffer);
                    if (battcom_ret == false)
                    {
                        ret = Result.FAILED_TO_OPEN_COM_PORT;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }

            // Serialを閉じる関数
            public Result Wrapper_CloseSerial()
            {
                Result ret;

                try
                {
                    // Access to Battcom.dll
                    CloseSerial();

                    ret = Result.OK;
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }

            //    M3A-7539をサーチする関数
            public Result Wrapper_SearchPorts_7539(ref string[] array_deviceDesc, ref int nnum)
            {
                Result ret;
                IntPtr[] deviceDesc = new IntPtr[16];
                string str_deviceDesc;

                ret = Result.OK;

                try
                {
                    SearchPorts_7539(deviceDesc, ref nnum);

                    for (int cnt = 0; cnt < deviceDesc.Length; cnt++)
                    {
                        str_deviceDesc = Marshal.PtrToStringAnsi(deviceDesc[cnt]);

                        array_deviceDesc[cnt] = str_deviceDesc;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }
            public Result Wrapper_SearchPorts(string PID, ref string[] array_deviceDesc, ref int nnum)
            {
                Result ret;
                IntPtr[] deviceDesc;
                string str_deviceDesc;

                deviceDesc = new IntPtr[16];

                ret = Result.OK;
                try
                {
                    // BATTCOMにアクセスする
                    SearchPorts(PID, deviceDesc, ref nnum);

                    for (int cnt = 0; cnt < deviceDesc.Length; cnt++)
                    {
                        str_deviceDesc = Marshal.PtrToStringAnsi(deviceDesc[cnt]);

                        array_deviceDesc[cnt] = str_deviceDesc;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }
            public Result Wrapper_ReadBlock(int DevAddr, int DevCmd, int hWnd, byte[] Buffer, ref int Len, ref long battcom_ret)
            {
                Result result = Result.OK;

                try
                {
                    battcom_ret = ReadBlock(DevAddr, DevCmd, hWnd, Buffer, ref Len);
                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        result = Result.COM_ERROR;
                    }
                    else
                    {
                        result = Result.OK;
                    }
                }
                catch
                {
                    result = Result.CATCH_EXCEPTION;
                }
                return result;
            }
            public Result Wrapper_WriteBlock(int DevAddr, int DevCmd, int hWnd, char[] Buffer, int Len)
            {
                Result ret;
                int battcom_ret;

                try
                {
                    // Access to Battcom.dll
                    battcom_ret = WriteBlock(DevAddr, DevCmd, hWnd, Buffer, Len);
                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        ret = Result.COM_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }
            public Result Wrapper_WriteWord(int DevAddr, int DevCmd, int hWnd, int lValue)
            {
                Result ret;
                int battcom_ret;

                try
                {
                    // Access to Battcom.dll
                    battcom_ret = WriteWord(DevAddr, DevCmd, hWnd, lValue);
                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        ret = Result.COM_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }
            public Result Wrapper_ReadWord(int DevAddr, int DevCmd, int hWnd, ref int lpValue, ref long battcom_ret)
            {
                Result ret;

                try
                {
                    // Access to Battcom.dll
                    battcom_ret = ReadWord(DevAddr, DevCmd, hWnd, ref lpValue);
                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        ret = Result.COM_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }
            public Result Wrapper_FlashWrite_M(int LAdr, ref byte Data, int ALen, bool enable_pec)
            {
                Result ret;
                int battcom_ret;

                try
                {
                    // PEC通信は無効？
                    if (enable_pec == false)
                    {
                        // Access to Battcom.dll
                        battcom_ret = FlashWrite_M(LAdr, ref Data, ALen);
                    }
                    else
                    {
                        battcom_ret = FlashWrite_M_PEC(LAdr, ref Data, ALen);
                    }

                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        ret = Result.COM_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }

            public Result Wrapper_FlashRead_M(int LAdr, int ALen, bool enable_pec)
            {
                Result ret;
                int battcom_ret;

                try
                {
                    // PEC通信は無効？
                    if (enable_pec == false)
                    {
                        battcom_ret = FlashRead_M(LAdr, ALen);
                    }
                    else
                    {
                        battcom_ret = FlashRead_M_PEC(LAdr, ALen);
                    }

                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        ret = Result.COM_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }

            public Result Wrapper_FlashRead_2(ref byte Buffer, int ALen, bool enable_pec)
            {
                Result ret;
                int battcom_ret;

                try
                {
                    // PEC通信は無効？
                    if (enable_pec == false)
                    {
                        battcom_ret = FlashRead_2(ref Buffer, ALen);
                    }
                    else
                    {
                        battcom_ret = FlashRead_2_PEC(ref Buffer, ALen);
                    }

                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        ret = Result.COM_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }

            public Result Wrapper_SetClock()
            {
                Result ret;
                int battcom_ret;

                try
                {
                    battcom_ret = SetClock();

                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        ret = Result.COM_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }

            public Result Wrapper_GetBoardVersion(byte[] port, byte[] read_buf)
            {
                Result ret;
                int read_length;

                try
                {
                    read_length = GetBoardVersion(port, read_buf, read_buf.Length);
                    if (read_length == 0)
                    {
                        ret = Result.OTHER_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }

                return ret;
            }

            public Result Wrapper_OutPortB(ref byte Buffer, int ALen)
            {
                Result ret;
                int battcom_ret;

                try
                {
                    battcom_ret = OutPortB(ref Buffer, ALen);

                    if (battcom_ret == COM_ERROR_DRV)
                    {
                        ret = Result.COM_ERROR;
                    }
                    else
                    {
                        ret = Result.OK;
                    }
                }
                catch
                {
                    ret = Result.CATCH_EXCEPTION;
                }
                return ret;
            }

            public void Wrapper_SetPECEnable(char ctrl)
            {
                SetPECEnable(ctrl);
            }
        }
    }
}
