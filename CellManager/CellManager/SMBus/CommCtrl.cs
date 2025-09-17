using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace MWC_REF_CSHARP
{
	class CommCtrl
	{
		// Communication board name
		private const string STR_COMM_M3A_7539 = "M3A-7539";
		private const string STR_COMM_RTK = "RTK0EF0029Z0000xBx";
		private const string STR_COMM_NOT_CONNECTED = "Not connected";

		// for Communication board ID
		private const int ID_ADDRESS = 0x1800;
		private const int ID_SIZE = 1;
		
		// Instance WrapperBattcom Class
		private readonly WrapperBattcom WrapBatt = new WrapperBattcom();

		// Redefine MAX_COM_PORT_NUM
		public const int MAX_COM_PORT_NUM = WrapperBattcom.WRAP_MAX_COM_PORT_NUM;

		// Result Code
		public enum RESULT
		{
			OK,
			FAILED_TO_COMMUNICATE,
			FAILED_TO_OPEN_COM_PORT,
			COM_PORT_IS_NOT_EXIST,
			OTHER_ERROR,

			CATCH_EXCEPTION
		}

		/// <summary>
		/// Convert WrapResult to RESULT
		/// </summary>
		/// <param name="wrap_result"></param>
		/// <returns></returns>
		private RESULT ConvertWrapResult2Result( WrapperBattcom.RESULT wrap_result )
		{
			RESULT result;

			switch ( wrap_result ) {
				case WrapperBattcom.RESULT.OK:
					result = RESULT.OK;
					break;
				case WrapperBattcom.RESULT.COM_ERROR:
					result = RESULT.FAILED_TO_COMMUNICATE;
					break;
				case WrapperBattcom.RESULT.FAILED_TO_OPEN_COM_PORT:
					result = RESULT.FAILED_TO_OPEN_COM_PORT;
					break;
				case WrapperBattcom.RESULT.OTHER_ERROR:
				default:
					result = RESULT.OTHER_ERROR;
					break;
				case WrapperBattcom.RESULT.CATCH_EXCEPTION:
					result = RESULT.CATCH_EXCEPTION;
					break;
			}

			return result;
		}

		/// <summary>
		/// Open specified COM Port
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public RESULT OpenCOMPort( byte[] port )
		{
			return ConvertWrapResult2Result( WrapBatt.Wrapper_OpenSerial( port ) );
		}

		/// <summary>
		/// Close COM Port
		/// </summary>
		/// <returns></returns>
		public RESULT CloseCOMPort()
		{
			return ConvertWrapResult2Result( WrapBatt.Wrapper_CloseSerial() );
		}

		/// <summary>
		/// Search and obtain connected COM port
		/// </summary>
		/// <param name="port"></param>
		/// <param name="board"></param>
		/// <param name="port_num"></param>
		/// <returns></returns>
		public RESULT SearchCOMPort( ref string[] port, ref string[] board, ref int port_num )
		{
			RESULT ret;
			string[] port_tmp = new string[ MAX_COM_PORT_NUM ];
			int port_num_m3a = 0;
			int port_num_rtk = 0;

			// Initialize board name
			for ( int cnt = 0; cnt < MAX_COM_PORT_NUM; cnt++ ) {
				board[ cnt ] = STR_COMM_NOT_CONNECTED;
			}
			
			// Search M3A-7539
			if ( WrapperBattcom.RESULT.OK == WrapBatt.Wrapper_SearchPorts_7539( ref port_tmp, ref port_num_m3a ) )
			{
				for ( int cnt = 0; cnt < port_num_m3a; cnt++ ) {
					port[ cnt ] = port_tmp[ cnt ];

					board[ cnt ] = STR_COMM_M3A_7539;
				}
				port_num += port_num_m3a;
			}

			// Search RTK0EF0029Z0000xBx
			if ( WrapperBattcom.RESULT.OK == WrapBatt.Wrapper_SearchPorts( "023E", ref port_tmp, ref port_num_rtk ) )
			{
				for ( int cnt = 0; cnt < port_num_rtk; cnt++ ) {
					port[ port_num_m3a + cnt ] = port_tmp[ cnt ];

					board[ port_num_m3a + cnt ] = STR_COMM_RTK;
				}
				port_num += port_num_rtk;
			}

			// Is detected COM port 0?
			if ( port_num == 0 ) {
				// Return Error
				ret = RESULT.COM_PORT_IS_NOT_EXIST;
			} else {
				ret = RESULT.OK;
			}

			return ret;
		}

		/// <summary>
		/// Read ID that is written in the communication board that is specified by COM Port.
		/// </summary>
		/// <param name="port"></param>
		/// <returns></returns>
		public int ReadWrittenId( byte[] port )
		{
			RESULT ret;
			int id = 0;
			
			ret = OpenCOMPort( port );
			if ( ret == RESULT.OK ) {
				// Read specified address
				if ( WrapperBattcom.RESULT.OK == WrapBatt.Wrapper_Read_7539Flash( ID_ADDRESS, ID_SIZE, ref id ) ) {
					// Check the range of the obtained value.
					if ( id < WrapperBattcom.MIN_ID_VALUE || WrapperBattcom.MAX_ID_VALUE < id ) {
						// If range over, set to 0xFF that means not written ID.
						id = 0xFF;
					}
				} else {
					// If failed to communication, set to 0xFF that means not written ID.
					id = 0xFF;
				}
				CloseCOMPort();
			} else {
				// If failed to open COM Port, set to 0xFF that means not written ID.
				id = 0xFF;
			}

			return id;
		}

		/// <summary>
		/// Wrapper class for battcom.dll
		/// </summary>
		private class WrapperBattcom
		{
			public enum RESULT
			{
				OK,
				COM_ERROR,
				FAILED_TO_OPEN_COM_PORT,
				OTHER_ERROR,

				CATCH_EXCEPTION
			}

			public const int WRAP_MAX_COM_PORT_NUM = 32;
			public const int MAX_ID_VALUE = 64;
			public const int MIN_ID_VALUE = 1;
			public const int COM_ERROR_VALUE = 0x10000000;

			[DllImport( "Battcom.dll" )]
			static extern void SearchPorts_7539( IntPtr[] DeviceDesc, ref int num );

			[DllImport( "Battcom.dll" )]
			static extern void SearchPorts( string PID, IntPtr[] DeviceDesc, ref int num );

			[DllImport( "Battcom.dll" )]
			static extern bool OpenSerial( byte[] Port );

			[DllImport( "Battcom.dll" )]
			static extern void CloseSerial();

			[DllImport( "Battcom.dll" )]
			static extern int Read_7539Flash( ushort address, byte length, IntPtr[] data );


			/// <summary>
			/// Wrapper for SearchPorts_7539()
			/// </summary>
			/// <param name="array_DeviceDesc"></param>
			/// <param name="num"></param>
			/// <returns></returns>
			public RESULT Wrapper_SearchPorts_7539( ref string[] array_DeviceDesc, ref int num )
			{
				RESULT ret;
				IntPtr[] device_desc = new IntPtr[ WRAP_MAX_COM_PORT_NUM ];

				ret = RESULT.OK;

				try
				{
					SearchPorts_7539( device_desc, ref num );

					for ( int cnt = 0; cnt < num; cnt++ ) {
						array_DeviceDesc[ cnt ]= Marshal.PtrToStringAnsi( device_desc[ cnt ] );
					}
				}
				catch
				{
					ret = RESULT.CATCH_EXCEPTION;
				}

				return ret;
			}

			/// <summary>
			/// Wrapper for SearchPorts
			/// </summary>
			/// <param name="PID"></param>
			/// <param name="array_DeviceDesc"></param>
			/// <param name="num"></param>
			/// <returns></returns>
			public RESULT Wrapper_SearchPorts( string PID, ref string[] array_DeviceDesc, ref int num )
			{
				RESULT ret;
				IntPtr[] device_desc = new IntPtr[ WRAP_MAX_COM_PORT_NUM ];

				ret = RESULT.OK;

				try
				{
					SearchPorts( PID, device_desc, ref num );

					for ( int cnt = 0; cnt < num; cnt++ ) {
						array_DeviceDesc[ cnt ] = Marshal.PtrToStringAnsi( device_desc[ cnt ] );
					}
				}
				catch
				{
					ret = RESULT.CATCH_EXCEPTION;
				}

				return ret;
			}

			/// <summary>
			/// Wrapper for OpenSerial()
			/// </summary>
			/// <param name="Port"></param>
			/// <returns></returns>
			public RESULT Wrapper_OpenSerial( byte[] Port )
			{
				RESULT ret;
				bool battcom_ret;

				try
				{
					battcom_ret = OpenSerial( Port );
					if ( battcom_ret == false ) {
						ret = RESULT.FAILED_TO_OPEN_COM_PORT;
					} else {
						ret = RESULT.OK;
					}
				}
				catch
				{
					ret = RESULT.CATCH_EXCEPTION;
				}

				return ret;
			}

			/// <summary>
			/// Wrapper CloseSerial()
			/// </summary>
			/// <returns></returns>
			public RESULT Wrapper_CloseSerial()
			{
				RESULT ret;
				
				ret = RESULT.OK;

				try
				{
					CloseSerial();
				}
				catch
				{
					ret = RESULT.CATCH_EXCEPTION;
				}

				return ret;
			}

			/// <summary>
			/// Wrapper Read_7539Flash()
			/// </summary>
			/// <param name="address"></param>
			/// <param name="length"></param>
			/// <param name="data"></param>
			/// <returns></returns>
			public RESULT Wrapper_Read_7539Flash( ushort address, byte length, ref int data )
			{
				RESULT ret;
				int battcom_ret;
				IntPtr[] id_ptr = new IntPtr[ 2 ];
				
				ret = RESULT.OK;

				try
				{
					battcom_ret = Read_7539Flash( address, length, id_ptr );
					if ( battcom_ret == COM_ERROR_VALUE ) {
						ret = RESULT.COM_ERROR;
					} else {
						data = (int)id_ptr[ 0 ];
					}
				}
				catch
				{
					ret = RESULT.CATCH_EXCEPTION;
				}

				return ret;
			}
		}
	}
}
