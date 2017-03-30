using Isic.SerialProtocol.Exceptions;
using ISIC_FMT_MMCP_App;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Isic.SerialProtocol
{
	public enum BOARD_VERSION
	{
		NOT_SET,
		NUC100,
		PIC,
		ERROR
	}

	/// <summary>
	/// Wrapper class for all the constants for the ISIC serial protocol
	/// Standard protocol structure:						| ATTN| ADDR| CMD | CMD | CMD | LEN |IHCHK|    DATA...   |IDCHK|
	/// Config protocol structure:							| ATTN| CMD | ADDR|DATA1|DATA2|DATA3|DATA4|    CSUM   |
	/// </summary>
	public class ISIC_SCP_IF
	{
		/// <summary>
		/// Protocol:						
		/// </summary>
		public const int HEADER_LENGTH = 7;

		/// <summary>
		/// Returns the board version (chip based) based on the part number
		/// </summary>
		/// <param name="partNo"></param>
		/// <returns></returns>
		public static BOARD_VERSION GetBoardVersion(String partNo)
		{
			/*if (!Validater.Validater.ValidatePartNo(partNo))
			{
				throw new PartNoNotValidException(String.Format("partNo '{0}' is not a valid ISIC part no.", partNo));
			}*/

			switch (partNo.Substring(0, 5))
			{
				case "07050":
					return BOARD_VERSION.PIC;

				case "07203":
					return BOARD_VERSION.NUC100;

				default:
					throw new BoardNotRecognisedException(String.Format("partNo '{0}' is not recognised as a valid interface board part no.", partNo));
					return BOARD_VERSION.ERROR;
			}
		}

        /// <summary>
        /// Attempts to reset the NUC100 based device
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="delay"></param>
        public static async void Reset(ICharacteristic characteristic, int delay = 50)
        {
            try
            {
                int[] baudArr = new int[] { 9600, 19200, 115200, 460800 };
                for (int i = 0; i < baudArr.Length; i++)
                {
                    //CNC@ISIC -> TODO: Insert Baud Rate
                    //sp.BaudRate = baudArr[i];
                    new Command(ISIC_SCP_IF.CMD_RST).Send(characteristic);
                    await new Config(ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_RESET, (byte[])null).Send(characteristic, false);
                    await Task.Delay(delay);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error resetting board Exception: {0}\nStacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Attempts to erase the EEPROM
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="delay"></param>
        public static async void EraseEeprom(ICharacteristic characteristic, int delay = 50)
        {
            try
            {
                Debug.WriteLine("Erasing EEPROM...");
                int[] baudArr = new int[] { 9600, 19200, 115200, 460800 };
                for (int i = 0; i < baudArr.Length; i++)
                {
                    //CNC@ISIC -> TODO: Select BaudRate
                    //sp.BaudRate = baudArr[i];
                    byte[] cfgValue = ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_LOCK_OFF.ToString("X2").GetBytes();
                    new Command(ISIC_SCP_IF.CMD_MCC, ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_LOCK, cfgValue[0], cfgValue[1]).Send(characteristic);
                    new Command(ISIC_SCP_IF.CMD_CFG).Send(characteristic);
                    await new Config(ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_ERASE_EEPROM, new byte[4]).Send(characteristic); ;
                    await Task.Delay(delay);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex + " Error erasing EEPROM");
            }
        }

        /// <summary>
        /// Gets the data from the received reply
        /// </summary>
        /// <param name="cmdReply"></param>
        /// <returns></returns>
        public static byte[] GetDataStringFromCmdReply(byte[] cmdReply)
		{
			if (cmdReply == null)
			{
				throw new ArgumentNullException("cmdReply cannot be null");
			}
			if (cmdReply.Length < 9)
			{
				return null;
				throw new IndexOutOfRangeException("cmdReply does not contain enough data to be a command with data");
			}
			return cmdReply.SubArray(7, cmdReply.Length - 8);
		}

        /// <summary>
        /// Tries to set the LockMonID locn in the interface board
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="locked"></param>
        /// <param name="retries"></param>
        /// <param name="deviceAddr"></param>
        /// <returns></returns>
        public static void SetLockMonID(ICharacteristic characteristic, bool locked, int retries = 5, byte deviceAddr = ISIC_SCP_IF.BYTE_BROADCAST_ADDR)
        {
            byte[] rArr;// = new byte[11];
            byte[] value = locked ? ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_LOCK_ON.ToString("X2").GetBytes() : ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_LOCK_OFF.ToString("X2").GetBytes();
            bool result = false;
            /*do
            {
                Task.Delay(10);
                Debug.WriteLine("Unlocking board. retries left: {0}", retries);
            } while (!(result = new Command(ISIC_SCP_IF.CMD_MCC, new byte[] { ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_LOCK, value[0], value[1] }).Send(characteristic)) && retries-- > 0);
            return result;*/
            new Command(ISIC_SCP_IF.CMD_MCC, new byte[] { ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_LOCK, value[0], value[1] }).Send(characteristic);
        }

        #region internal commmands
        public const String CMD_INT_DEBUG_ENABLED = "DEBUGEN";
		public const String CMD_INT_DEBUG_DISABLED = "DEBUGDE";
		public const String CMD_INT_CET = "CET";
		#endregion // internal commmands

		public enum CMD
		{
			/// <summary>
			/// Enter bootloader on PIC based boards, not implemented on NUC100 based boards
			/// </summary>
			BLD,
			/// <summary>
			/// Set brightness/backlight
			/// </summary>
			BRT,
			/// <summary>
			/// Set brightness/backlight minimum
			/// </summary>
			BRI,
			/// <summary>
			/// Set brightness/backlight maximum
			/// </summary>
			BRM,
			/// <summary>
			/// Set buzzer status and/or frequency
			/// </summary>
			BZZ,
			/// <summary>
			/// Set the board in config mode
			/// </summary>
			CFG,
			/// <summary>
			/// Downloads a certain datapacket from the EEPROM
			/// </summary>
			DLN,
			/// <summary>
			/// Asks for the number of 32byte packages
			/// </summary>
			DLQ,
			/// <summary>
			/// Set the external baud rate
			/// </summary>
			EBR,
			/// <summary>
			/// Set ECDUS value (day/dusk/night)
			/// </summary>
			ECD,
			/// <summary>
			/// Get estimated runtime
			/// </summary>
			ETC,
			/// <summary>
			/// Set fan state
			/// </summary>
			FAN,
			/// <summary>
			/// Get ECDIS backlight value
			/// </summary>
			GEB,
			/// <summary>
			/// Get soft dimming cut off value
			/// </summary>
			GCO,
			/// <summary>
			/// Get lightsensor value
			/// </summary>
			LIS,
			/// <summary>
			/// Get manufacturer data
			/// </summary>
			MAN,
			/// <summary>
			/// Send monitor control command
			/// </summary>
			MCC,
			/// <summary>
			/// Execute monitor operation
			/// </summary>
			MEO,
			/// <summary>
			/// Set the PWM3 frequency and dutycycle
			/// </summary>
			PWM3,
			/// <summary>
			/// Get/set the reference number
			/// </summary>
			REF,
			/// <summary>
			/// Reset the MCU
			/// </summary>
			RST,
			/// <summary>
			/// Set soft dimming cutoff value
			/// </summary>
			SCO,
			/// <summary>
			/// Set device address
			/// </summary>
			SDA,
			/// <summary>
			/// No idea what this does?????????????????????????
			/// </summary>
			SDC,
			/// <summary>
			/// Set ECDIS backlight value
			/// </summary>
			SEB,
			/// <summary>
			/// Get/set the serial number
			/// </summary>
			SNB,
			/// <summary>
			/// Get running temperature
			/// </summary>
			TMP,
			/// <summary>
			/// Get product type
			/// </summary>
			TYP,
			/// <summary>
			/// Get firmware version
			/// </summary>
			VER,
			/// <summary>
			/// Uploads data packages to the eeprom.
			/// </summary>
			WEE,
			MAX
		}

		//private String[] _CMD = {
		//	"BRT", "BZZ", "CFG", "DLN", "DL?",
		//	"EBR", "ECD", "ETC", "FAN", "GEB",
		//	"GCO", "LIS", "MAN", "MCC", "MEO",
		//	"PWM", "REF", "RST", "SCO", "SDA",
		//	"SDC", "SEB", "SNB", "TMP", "TYP",
		//	"VER", "WEE" };

		public String this[CMD cmd]
		{
			get
			{
				return Enum.GetName(typeof(CMD), cmd);
				//return _CMD[(int)cmd];
			}
		}

		public int Length
		{
			get
			{
				return (int)CMD.MAX;
				//return _CMD.Length;
			}
		}

		#region CMD strings
		/// <summary>
		/// Set brightness/backlight
		/// </summary>
		public const String CMD_422 = "422";
		/// <summary>
		/// Enter bootloader on PIC based boards, not implemented on NUC100 based boards
		/// </summary>
		public const String CMD_BLD = "BLD";
		/// <summary>
		/// Set brightness/backlight
		/// </summary>
		public const String CMD_BRT = "BRT";
		/// <summary>
		/// Set brightness/backlight minimum
		/// </summary>
		public const String CMD_BRI = "BRI";
		/// <summary>
		/// Set brightness/backlight maximu,
		/// </summary>
		public const String CMD_BRM = "BRM";
		/// <summary>
		/// Set buzzer status and/or frequency
		/// </summary>
		public const String CMD_BZZ = "BZZ";
		/// <summary>
		/// Set the board in config mode
		/// </summary>
		public const String CMD_CFG = "CFG";
		/// <summary>
		/// Downloads a certain datapacket from the EEPROM
		/// </summary>
		public const String CMD_DLN = "DLN";
		/// <summary>
		/// Asks for the number of 32byte packages
		/// </summary>
		public const String CMD_DLQ = "DL?";
		/// <summary>
		/// Set the external baud rate
		/// </summary>
		public const String CMD_EBR = "EBR";
		/// <summary>
		/// Set ECDUS value (day/dusk/night)
		/// </summary>
		public const String CMD_ECD = "ECD";
		/// <summary>
		/// Get estimated runtime
		/// </summary>
		public const String CMD_ETC = "ETC";
		/// <summary>
		/// Set fan state
		/// </summary>
		public const String CMD_FAN = "FAN";
		/// <summary>
		/// Get ECDIS backlight value
		/// </summary>
		public const String CMD_GEB = "GEB";
		/// <summary>
		/// Get soft dimming cut off value
		/// </summary>
		public const String CMD_GCO = "GCO";
		/// <summary>
		/// Get lightsensor value
		/// </summary>
		public const String CMD_LIS = "LIS";
		/// <summary>
		/// Get manufacturer data
		/// </summary>
		public const String CMD_MAN = "MAN";
		/// <summary>
		/// Send monitor control command
		/// </summary>
		public const String CMD_MCC = "MCC";
		/// <summary>
		/// Execute monitor operation
		/// </summary>
		public const String CMD_MEO = "MEO";
		/// <summary>
		/// Set the PWM3 frequency and dutycycle
		/// </summary>
		public const String CMD_PWM3 = "PWM";
		/// <summary>
		/// Get/set the reference number
		/// </summary>
		public const string CMD_REF = "REF";
		/// <summary>
		/// Reset the MCU
		/// </summary>
		public const String CMD_RST = "RST";
		/// <summary>
		/// Set soft dimming cutoff value
		/// </summary>
		public const String CMD_SCO = "SCO";
		/// <summary>
		/// Set device address
		/// </summary>
		public const String CMD_SDA = "SDA";
		/// <summary>
		/// No idea what this does?????????????????????????
		/// </summary>
		public const String CMD_SDC = "SDC";
		/// <summary>
		/// Set ECDIS backlight value
		/// </summary>
		public const string CMD_SEB = "SEB";
		/// <summary>
		/// Get/set the serial number
		/// </summary>
		public const string CMD_SNB = "SNB";
		/// <summary>
		/// Get running temperature
		/// </summary>
		public const String CMD_TMP = "TMP";
		/// <summary>
		/// Test command for debug purpose only
		/// </summary>
		public const String CMD_TST = "TST";
		/// <summary>
		/// Get product type
		/// </summary>
		public const String CMD_TYP = "TYP";
		/// <summary>
		/// Get firmware version
		/// </summary>
		public const String CMD_VER = "VER";
		/// <summary>
		/// Uploads data packages to the eeprom.
		/// </summary>
		public const String CMD_WEE = "WEE";
		#endregion // CMD strings


		/// <summary>
		/// Attention value for config
		/// </summary>
		public const byte BYTE_ATTN_CFG = 0x02;
		/// <summary>
		/// Attention ack value
		/// </summary>
		public const byte BYTE_ATTN_ACK = 0x06;
		/// <summary>
		/// Attention value for command
		/// </summary>
		public const byte BYTE_ATTN_CMD = 0x07;
		/// <summary>
		/// ISIC broadcast device address
		/// </summary>
		public const byte BYTE_BROADCAST_ADDR = 0xFF;

		/// <summary>
		/// Indexes of the bytes in the header
		/// </summary>
		/// {
		public const byte BYTE_INDEX_ATTN = 0;
		public const byte BYTE_INDEX_ADDR = 1;
		public const byte BYTE_INDEX_CMD1 = 2;
		public const byte BYTE_INDEX_CMD2 = 3;
		public const byte BYTE_INDEX_CMD3 = 4;
		public const byte BYTE_INDEX_LEN = 5;
		public const byte BYTE_INDEX_IHCHK = 6;
		/// }

		#region Data bytes
		public const byte BYTE_DATA_ON = 0xff;
		public const byte BYTE_DATA_OFF = 0x00;
		public const byte BYTE_DATA_FAN_AUTO = 0x01;
		public const byte BYTE_DATA_ETC_TOTAL = 0x30;
		public const byte BYTE_DATA_ETC_60 = 0x31;
		public const byte BYTE_DATA_ETC_60_70 = 0x32;
		public const byte BYTE_DATA_ETC_70 = 0x33;
		public const byte BYTE_DATA_ETC_MIN = 0x34;
		public const byte BYTE_DATA_TMP_READ = (byte)'R';
		#endregion // Data bytes

		#region External Baud Rate
		public const byte BYTE_DATA_EBR_9K6 = 0x01;
		public const byte BYTE_DATA_EBR_19K2 = 0x00;
		public const byte BYTE_DATA_EBR_115K2 = 0x02;
		public const byte BYTE_DATA_EBR_460K8 = 0x03;
		public const byte BYTE_DATA_EBR_DEFAULT = 0xFF;
		#endregion // External Baud Rate

		#region ECDIS
		public const byte BYTE_DATA_ECD_DAY = 0x00;
		public const byte BYTE_DATA_ECD_DUSK = 0x01;
		public const byte BYTE_DATA_ECD_NIGHT = 0x02;
		public const byte BYTE_DATA_ECD_OFF = 0xFF;
		#endregion // ECDIS

		#region DLN
		public enum DLN
		{
			_DLN_NOT_ACTIVE = 0x00,
			_DLN_READY = 0x01,
			_DLN_WAITING = 0x02,
			_DLN_NO_PK = 0x05
		}
		#endregion // DLN

		#region MEO
		public const byte BYTE_DATA_MEO_AAVGA = 0x02; // AutoAdjustVGA
		public const byte BYTE_DATA_MEO_VGAS = 0x03; // VGASave
		public const byte BYTE_DATA_MEO_VGAL = 0x04; // VGALoad
		public const byte BYTE_DATA_MEO_ACA = 0x05; // AutoColorAdjust
		public const byte BYTE_DATA_MEO_MS = 0x06; // MonitorSave
		public const byte BYTE_DATA_MEO_ML = 0x07; // MonitorLoad
		public const byte BYTE_DATA_MEO_LFD = 0x08; // LoadFactoryDefaults
		#endregion // MEO

		#region MCC
		public const byte BYTE_DATA_MCC_ADDR_MPC = 0x98;
		public const byte BYTE_DATA_MCC_ADDR_BKL = 0x59;
		public const byte BYTE_DATA_MCC_ADDR_BRI = 0x81;
		public const byte BYTE_DATA_MCC_ADDR_CON = 0x82;
		public const byte BYTE_DATA_MCC_ADDR_PWR = 0x9E;
		public const byte BYTE_DATA_MCC_ADDR_CT = 0xB3;
		public const byte BYTE_DATA_MCC_ADDR_GR = 0xB4;
		public const byte BYTE_DATA_MCC_ADDR_GG = 0xB5;
		public const byte BYTE_DATA_MCC_ADDR_GB = 0xB6;
		public const byte BYTE_DATA_MCC_ADDR_GMA = 0x9D;
		public const byte BYTE_DATA_MCC_ADDR_LOCK = 0x3A;

		public const byte BYTE_DATA_MCC_VALUE_MPC_VGA = 0x00;
		public const byte BYTE_DATA_MCC_VALUE_MPC_DVI = 0x01;
		public const byte BYTE_DATA_MCC_VALUE_MPC_DP = 0x02;
		public const byte BYTE_DATA_MCC_VALUE_MPC_RAC = 0xFF;
		public const byte BYTE_DATA_MCC_VALUE_PWR_OFF = 0x00;
		public const byte BYTE_DATA_MCC_VALUE_PWR_ON = 0x01;
		public const byte BYTE_DATA_MCC_VALUE_CT_4200K = 0x00;
		public const byte BYTE_DATA_MCC_VALUE_CT_5000K = 0x01;
		public const byte BYTE_DATA_MCC_VALUE_CT_5400K = 0x02;
		public const byte BYTE_DATA_MCC_VALUE_CT_6500K = 0x03;
		public const byte BYTE_DATA_MCC_VALUE_CT_7500K = 0x04;
		public const byte BYTE_DATA_MCC_VALUE_CT_9300K = 0x05;
		public const byte BYTE_DATA_MCC_VALUE_CT_USER = 0x06;
		public const byte BYTE_DATA_MCC_VALUE_GMA_NATIVE = 0x00;
		public const byte BYTE_DATA_MCC_VALUE_GMA_2_2 = 0x01;
		public const byte BYTE_DATA_MCC_VALUE_GMA_CUSTOM = 0x02;
		public const byte BYTE_DATA_MCC_VALUE_LOCK_ON = 0x00;
		public const byte BYTE_DATA_MCC_VALUE_LOCK_OFF = 0x01;
		#endregion // MCC

		#region Config Mode
		/// <summary>
		/// Config protocol structure:							| ATTN| CMD |ADDR0|ADDR1|DATA0|DATA1|DATA2|DATA3|CSUM0|CSUM1|
		/// </summary>
		public static readonly byte[] CMD_CFG_ENTER = { 0x00, 0x00, 0x43, 0x46, 0x47, 0x00, 0x00 };
		public static readonly byte[] CMD_CFG_EXIT = { 0x02, 0x58, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x37, 0x38 };

		public const byte BYTE_DATA_CFG_ATTN = 0x02;

		public const byte BYTE_DATA_CFG_CMD_C = (byte)'C'; // Config?
		public const byte BYTE_DATA_CFG_CMD_P = (byte)'P'; // Packlight Curve?
		public const byte BYTE_DATA_CFG_CMD_R = (byte)'R'; // Read?

		public const String BYTE_DATA_CFG_ADDR_EXIT = "00";
		public const String BYTE_DATA_CFG_ADDR_ECDIS_LED_MODE = "01";
		public const String BYTE_DATA_CFG_ADDR_EBR = "02";
		public const String BYTE_DATA_CFG_ADDR_ADDR = "03";
		public const String BYTE_DATA_CFG_ADDR_DUAL_CALIBRATION = "04";
		public const String BYTE_DATA_CFG_ADDR_BZZ_VOLT = "05";
		public const String BYTE_DATA_CFG_ADDR_BZZ_FRQ = "06";
		public const String BYTE_DATA_CFG_ADDR_RS4XX = "07";
		public const String BYTE_DATA_CFG_ADDR_BACKLIGHT_CUTOFF = "08";
		public const String BYTE_DATA_CFG_ADDR_LOGO_SHOW_TIME = "09";
		public const String BYTE_DATA_CFG_ADDR_PANEL = "0A";
		public const String BYTE_DATA_CFG_ADDR_BACKLIGHT_MAXIMUM = "0B";
		public const String BYTE_DATA_CFG_ADDR_HMI_MENU_DISABLED = "0C";
		public const String BYTE_DATA_CFG_ADDR_DEVICE_TYPE = "0D";
		public const String BYTE_DATA_CFG_ADDR_BRT_ECDIS_DEADZONE = "0E";
		public const String BYTE_DATA_CFG_ADDR_BRT_PWM_INVERTED = "0F";
		public const String BYTE_DATA_CFG_ADDR_DEBUG = "FC";
		public const String BYTE_DATA_CFG_ADDR_RESET = "FD";
		public const String BYTE_DATA_CFG_ADDR_ERASE_EEPROM = "FE";

		public const String BYTE_DATA_CFG_VALUE_DEVICE_TYPE_NOT_SET = "FFFF";
		public const String BYTE_DATA_CFG_VALUE_DEVICE_TYPE_IFB16 = "0000";
		public const String BYTE_DATA_CFG_VALUE_DEVICE_TYPE_SB15 = "0001";
		public const String BYTE_DATA_CFG_VALUE_DEVICE_TYPE_IFB16_4K = "0002";

		public const String BYTE_DATA_CFG_VALUE_BZZ_VOLTAGE_26_4 = "0000";
		public const String BYTE_DATA_CFG_VALUE_BZZ_VOLTAGE_35_5 = "0001";

		public const String BYTE_DATA_CFG_VALUE_ECDIS_LED_NORM_DUAL = "0000";
		public const String BYTE_DATA_CFG_VALUE_ECDIS_LED_NORM_SING = "0001";
		public const String BYTE_DATA_CFG_VALUE_ECDIS_LED_INV_DUAL = "0010";
		public const String BYTE_DATA_CFG_VALUE_ECDIS_LED_INV_SING = "0011";

		public const String BYTE_DATA_CFG_VALUE_BAUD_9K6 = "0001";
		public const String BYTE_DATA_CFG_VALUE_BAUD_19K2 = "0000";
		public const String BYTE_DATA_CFG_VALUE_BAUD_115K2 = "0002";
		public const String BYTE_DATA_CFG_VALUE_BAUD_460K8 = "0003";
		public const String BYTE_DATA_CFG_VALUE_BAUD_DEFAULT = "00FF";

		public const String BYTE_DATA_CFG_VALUE_RS4XX_OFF = "0000";
		public const String BYTE_DATA_CFG_VALUE_RS4XX_422 = "0001";
		public const String BYTE_DATA_CFG_VALUE_RS4XX_485 = "0002";

		public const String BYTE_DATA_CFG_VALUE_PANEL_SIZE_15 = "0000"; // 1024x768
		public const String BYTE_DATA_CFG_VALUE_PANEL_SIZE_19 = "0001"; // 1280x1024
		public const String BYTE_DATA_CFG_VALUE_PANEL_SIZE_22_WVA = "0002"; // 1680x1050
		public const String BYTE_DATA_CFG_VALUE_PANEL_SIZE_24 = "0003"; // 1920x1080
		public const String BYTE_DATA_CFG_VALUE_PANEL_SIZE_26 = "0004"; // 1920x1200
		public const String BYTE_DATA_CFG_VALUE_PANEL_SIZE_27 = "0005"; // 1920x1080
		public const String BYTE_DATA_CFG_VALUE_PANEL_SIZE_22_TN = "0006"; // 1680x1050

		public const String BYTE_DATA_CFG_VALUE_PANEL_1280x1024 = "0000";
		public const String BYTE_DATA_CFG_VALUE_PANEL_1920x1080 = "0002";
		public const String BYTE_DATA_CFG_VALUE_PANEL_1920x1200 = "0008";

		public const String BYTE_DATA_CFG_VALUE_ENABLED = "0001";
		public const String BYTE_DATA_CFG_VALUE_DISABLED = "0000";

		public const String BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_OFF = "00FF";
		public const String BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_0 = "0000";
		public const String BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_1 = "0001";
		public const String BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_2 = "0002";
		public const String BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_3 = "0003";
		public const String BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_4 = "0004";
		public const String BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_5 = "0005";

		public const String BYTE_DATA_CFG_VALUE_BRT_PWM_INVERTED_YES = "0001";
		public const String BYTE_DATA_CFG_VALUE_BRT_PWM_INVERTED_NO = "0000";

		#region Old legacy bytes for PIC based boards PIC18F67K22
		public const String BYTE_DATA_CFG_LEGACY_TYPE = "00";
		public const String BYTE_DATA_CFG_LEGACY_BAUD = "0C";
		public const String BYTE_DATA_CFG_LEGACY_BRT = "0D";

		public const String BYTE_DATA_CFG_LEGACY_VALUE_TYPE_MON = "0000";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_TYPE_PPC = "0001";

		public const String BYTE_DATA_CFG_LEGACY_VALUE_BAUD_19K2 = "0000";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BAUD_9K6 = "0001";

		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_ISIC = "0000";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_SPERRY_RADAR = "0001";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_SPERRY_ECDIS = "0002";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_KONGSBERG_LED = "0003";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_TRANSAS = "0005";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_OLD_ECDIS = "0009";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_ABB_KEYBOARD = "000.";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_ROLLS_ROYCE = "000;";
		public const String BYTE_DATA_CFG_LEGACY_VALUE_BRT_BRUNVOLL = "000?";

		/// <summary>
		/// List containing the external baud rate command parameters
		/// </summary>
		public class CfgLegacyTypeList : List<ParamContainer>
		{
			private static CfgLegacyTypeList instance;
			public String Address { get; set; }
			private CfgLegacyTypeList()
			{
				Address = ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_TYPE;
				Add(new ParamContainer("DuraMON", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_TYPE_MON));
				Add(new ParamContainer("DuraPANEL", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_TYPE_PPC));
			}

			/// <summary>
			/// Returns the singleton instance of the list
			/// </summary>
			public static CfgLegacyTypeList Instance
			{
				get
				{
					if (instance == null)
					{
						instance = new CfgLegacyTypeList();
					}
					return instance;
				}
			}
		}

		/// <summary>
		/// List containing the external baud rate command parameters
		/// </summary>
		public class CfgLegacyBaudList : List<ParamContainer>
		{
			private static CfgLegacyBaudList instance;
			public String Address { get; set; }
			private CfgLegacyBaudList()
			{
				Address = ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_BAUD;
				Add(new ParamContainer("19200", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BAUD_19K2));
				Add(new ParamContainer("9600", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BAUD_9K6));
			}

			/// <summary>
			/// Returns the singleton instance of the list
			/// </summary>
			public static CfgLegacyBaudList Instance
			{
				get
				{
					if (instance == null)
					{
						instance = new CfgLegacyBaudList();
					}
					return instance;
				}
			}
		}

		/// <summary>
		/// List containing the external baud rate command parameters
		/// </summary>
		public class CfgLegacyBrtList : List<ParamContainer>
		{
			private static CfgLegacyBrtList instance;
			public String Address { get; set; }
			private CfgLegacyBrtList()
			{
				Address = ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_BRT;
				Add(new ParamContainer("Standard ISIC", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_ISIC));
				Add(new ParamContainer("Sperry Radar", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_SPERRY_RADAR));
				Add(new ParamContainer("Sperry ECDIS", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_SPERRY_ECDIS));
				Add(new ParamContainer("Kongsberg LED", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_KONGSBERG_LED));
				Add(new ParamContainer("Transas", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_TRANSAS));
				Add(new ParamContainer("Old ECDIS", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_OLD_ECDIS));
				Add(new ParamContainer("ABB Keyboard", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_ABB_KEYBOARD));
				Add(new ParamContainer("Rolls Royce", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_ROLLS_ROYCE));
				Add(new ParamContainer("Brunvoll", ISIC_SCP_IF.BYTE_DATA_CFG_LEGACY_VALUE_BRT_BRUNVOLL));
			}

			/// <summary>
			/// Returns the singleton instance of the list
			/// </summary>
			public static CfgLegacyBrtList Instance
			{
				get
				{
					if (instance == null)
					{
						instance = new CfgLegacyBrtList();
					}
					return instance;
				}
			}
		}
		#endregion // Old legacy bytes for PIC based boards PIC18F67K22
		#endregion // Config Mode

		#region WEE
		public enum WEE
		{
			_ISIC_EEP_WEE_NOT_ACTIVE = 0,
			_ISIC_EEP_WEE_OK = 1,
			_ISIC_EEP_WEE_ERR_TYPE = 2,
			_ISIC_EEP_WEE_ERR_NO = 3,
			_ISIC_EEP_WEE_ERR_ADDR = 4,
			_ISIC_EEP_WEE_ERR_SIZE = 5,
			_ISIC_EEP_WEE_ERR_CHK = 6,
			_ISIC_EEP_WEE_ERR_WRONG_RESPONSE = 20,
			_ISIC_EEP_WEE_WAITING_REPLY = 7
		}

		public enum WEE_TYPE
		{
			ColorMap = 0,
			EDID = 1,
			ScalerFirmware = 2
		}

		public enum WEE_EDID_MAPS
		{
			VGA,
			DVI,
			HDMI
		}
		#endregion // WEE
	}

	#region CFG Lists
	/// <summary>
	/// List containing all the config parameters
	/// </summary>
	public class CfgCmdList : List<ParamContainer>
	{
		private static CfgCmdList instance;
		private CfgCmdList()
		{
			Add(new ParamContainer("Device Type", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_DEVICE_TYPE));
			Add(new ParamContainer("ECDIS LED Mode", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_ECDIS_LED_MODE));
			Add(new ParamContainer("External Baud Rate", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_EBR));
			Add(new ParamContainer("Device Address", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_ADDR));
			Add(new ParamContainer("Dual Calibration", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_DUAL_CALIBRATION));
			Add(new ParamContainer("BRT ECDIS Deadzone", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_BRT_ECDIS_DEADZONE));
			Add(new ParamContainer("Buzzer Voltage", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_BZZ_VOLT));
			Add(new ParamContainer("Buzzer frequency", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_BZZ_FRQ));
			Add(new ParamContainer("RS4XX", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_RS4XX));
			Add(new ParamContainer("Backlight Cutoff", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_BACKLIGHT_CUTOFF));
			Add(new ParamContainer("Backlight Maximum", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_BACKLIGHT_MAXIMUM));
			Add(new ParamContainer("BRT PWM Inverted", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_BRT_PWM_INVERTED));
			Add(new ParamContainer("Logo show time", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_LOGO_SHOW_TIME));
			Add(new ParamContainer("Panel", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_PANEL));
			Add(new ParamContainer("HMI Menu", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_HMI_MENU_DISABLED));
			Add(new ParamContainer("Debug Enabled", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_DEBUG));
			Add(new ParamContainer("Reset MCU", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_RESET));
			Add(new ParamContainer("Erase EEPROM", ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_ERASE_EEPROM));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgCmdList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgCmdList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the device type command parameters
	/// </summary>
	public class CfgDeviceType : List<ParamContainer>
	{
		private static CfgDeviceType instance;
		public String Address { get; set; }
		private CfgDeviceType()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_DEVICE_TYPE;
			Add(new ParamContainer("Not set", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_DEVICE_TYPE_NOT_SET));
			Add(new ParamContainer("IFB16", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_DEVICE_TYPE_IFB16));
			Add(new ParamContainer("IFB16 4K", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_DEVICE_TYPE_IFB16_4K));
			Add(new ParamContainer("SB15", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_DEVICE_TYPE_SB15));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgDeviceType Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgDeviceType();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the ECDIS LED mode command parameters
	/// </summary>
	public class CfgEcdisLedMode : List<ParamContainer>
	{
		private static CfgEcdisLedMode instance;
		public String Address { get; set; }
		private CfgEcdisLedMode()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_ECDIS_LED_MODE;
			Add(new ParamContainer("Normal dual led (Classic)", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_ECDIS_LED_NORM_DUAL));
			Add(new ParamContainer("Normal single led (Glass)", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_ECDIS_LED_NORM_SING));
			Add(new ParamContainer("Inverted dual led (Classic)", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_ECDIS_LED_INV_DUAL));
			Add(new ParamContainer("Inverted single led (Glass)", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_ECDIS_LED_INV_SING));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgEcdisLedMode Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgEcdisLedMode();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the external baud rate command parameters
	/// </summary>
	public class CfgBaudList : List<ParamContainer>
	{
		private static CfgBaudList instance;
		public String Address { get; set; }
		private CfgBaudList()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_EBR;
			//Add(new ParamContainer("Default", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BAUD_DEFAULT));
			Add(new ParamContainer("9600", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BAUD_9K6));
			Add(new ParamContainer("19200", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BAUD_19K2));
			Add(new ParamContainer("115200", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BAUD_115K2));
			Add(new ParamContainer("460800", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BAUD_460K8));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgBaudList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgBaudList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the dual calibration command parameters
	/// </summary>
	public class CfgDualCalibrationList : List<ParamContainer>
	{
		private static CfgDualCalibrationList instance;
		public String Address { get; set; }
		private CfgDualCalibrationList()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_DUAL_CALIBRATION;
			Add(new ParamContainer("Disabled", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_DISABLED));
			Add(new ParamContainer("Enabled", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_ENABLED));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgDualCalibrationList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgDualCalibrationList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the dual calibration command parameters
	/// </summary>
	public class CfgBrtEcdisDeadzone : List<ParamContainer>
	{
		private static CfgBrtEcdisDeadzone instance;
		public String Address { get; set; }
		private CfgBrtEcdisDeadzone()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_DUAL_CALIBRATION;
			Add(new ParamContainer("Off", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_OFF));
			Add(new ParamContainer("\u00B10 Step", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_0));
			Add(new ParamContainer("\u00B11 Step", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_1));
			Add(new ParamContainer("\u00B12 Step", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_2));
			Add(new ParamContainer("\u00B13 Step", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_3));
			Add(new ParamContainer("\u00B14 Step", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_4));
			Add(new ParamContainer("\u00B15 Step", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_ECDIS_DEADZONE_5));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgBrtEcdisDeadzone Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgBrtEcdisDeadzone();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the buzzer voltage command parameters
	/// </summary>
	public class CfgBzzVoltageList : List<ParamContainer>
	{
		private static CfgBzzVoltageList instance;
		public String Address { get; set; }
		private CfgBzzVoltageList()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_BZZ_VOLT;
			Add(new ParamContainer("26.4v", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BZZ_VOLTAGE_26_4));
			Add(new ParamContainer("31.5v", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BZZ_VOLTAGE_35_5));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgBzzVoltageList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgBzzVoltageList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the RS4XX command parameters
	/// </summary>
	public class CfgRS4XXList : List<ParamContainer>
	{
		private static CfgRS4XXList instance;
		public String Address { get; set; }
		private CfgRS4XXList()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_RS4XX;
			Add(new ParamContainer("Off", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_RS4XX_OFF));
			Add(new ParamContainer("RS422", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_RS4XX_422));
			Add(new ParamContainer("RS485", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_RS4XX_485));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgRS4XXList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgRS4XXList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the panel command parameters
	/// </summary>
	public class CfgPanelList : List<ParamContainer>
	{
		private static CfgPanelList instance;
		public String Address { get; set; }
		private CfgPanelList()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_PANEL;
			Add(new ParamContainer("DuraMon 15", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_SIZE_15));
			Add(new ParamContainer("DuraMon 19", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_SIZE_19));
			Add(new ParamContainer("DuraMon 22 WVA", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_SIZE_22_WVA));
			Add(new ParamContainer("DuraMon 22 TN", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_SIZE_22_TN));
			Add(new ParamContainer("DuraMon 24", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_SIZE_24));
			Add(new ParamContainer("DuraMon 26", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_SIZE_26));
			Add(new ParamContainer("DuraMon 27", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_SIZE_27));
			//Add(new ParamContainer("1280x1024", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_1280x1024));
			//Add(new ParamContainer("1920x1080", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_1920x1080));
			//Add(new ParamContainer("1920x1200", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_PANEL_1920x1200));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgPanelList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgPanelList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the HMI lock command parameters
	/// </summary>
	public class CfgHmiMenuList : List<ParamContainer>
	{
		private static CfgHmiMenuList instance;
		public String Address { get; set; }
		private CfgHmiMenuList()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_HMI_MENU_DISABLED;
			Add(new ParamContainer("Enabled", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_DISABLED));
			Add(new ParamContainer("Disabled", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_ENABLED));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgHmiMenuList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgHmiMenuList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the BRT PWM Inverted command parameters
	/// </summary>
	public class CfgBrtPwmInverted : List<ParamContainer>
	{
		private static CfgBrtPwmInverted instance;
		public String Address { get; set; }
		private CfgBrtPwmInverted()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_BRT_PWM_INVERTED;
			Add(new ParamContainer("Yes", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_PWM_INVERTED_YES));
			Add(new ParamContainer("No", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_BRT_PWM_INVERTED_NO));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgBrtPwmInverted Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgBrtPwmInverted();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the debug command parameters
	/// </summary>
	public class CfgDebugEnabledList : List<ParamContainer>
	{
		private static CfgDebugEnabledList instance;
		public String Address { get; set; }
		private CfgDebugEnabledList()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_DEBUG;
			Add(new ParamContainer("Enabled", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_ENABLED));
			Add(new ParamContainer("Disabled", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_DISABLED));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static CfgDebugEnabledList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CfgDebugEnabledList();
				}
				return instance;
			}
		}
	}

	//public class CfgConfigByteList : List<ParamContainer> {
	//	private static CfgConfigByteList instance;
	//	private CfgConfigByteList() {
	//		Add(new ParamContainer("Default", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_ISIC_DEFAULT_B1, ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_ISIC_DEFAULT_B0));
	//		Add(new ParamContainer("ISIC Mode", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_ISIC_B0));
	//		Add(new ParamContainer("Sperry Radar", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_SPERRY_RADAR_B0));
	//		Add(new ParamContainer("Sperry ECDIS", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_SPERRY_ECDIS_B0));
	//		Add(new ParamContainer("Transas", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_TRANSAS_B0));
	//		Add(new ParamContainer("Old ECDIS", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_OLD_ECDIS_B0));
	//		Add(new ParamContainer("ABB Keyboard", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_ABB_KEYB_B0));
	//		Add(new ParamContainer("Brunvol Panel PC", ISIC_SCP_IF.BYTE_DATA_CFG_VALUE_CFG_BYTE_BRUNVOL_B0));
	//	}

	//	public static CfgConfigByteList Instance {
	//		get {
	//			if (instance == null) {
	//				instance = new CfgConfigByteList();
	//			}
	//			return instance;
	//		}
	//	}
	//}

	/// <summary>
	/// List containing the external baud rate command parameters
	/// </summary>
	public class EbrBaudList : List<ParamContainer>
	{
		private static EbrBaudList instance;
		public String Address { get; set; }
		private EbrBaudList()
		{
			Address = ISIC_SCP_IF.BYTE_DATA_CFG_ADDR_EBR;
			Add(new ParamContainer("Default", ISIC_SCP_IF.BYTE_DATA_EBR_19K2));
			Add(new ParamContainer("9600", ISIC_SCP_IF.BYTE_DATA_EBR_9K6));
			Add(new ParamContainer("19200", ISIC_SCP_IF.BYTE_DATA_EBR_19K2));
			Add(new ParamContainer("115200", ISIC_SCP_IF.BYTE_DATA_EBR_115K2));
			Add(new ParamContainer("460800", ISIC_SCP_IF.BYTE_DATA_EBR_460K8));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static EbrBaudList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new EbrBaudList();
				}
				return instance;
			}
		}
	}
	#endregion // CFG Lists

	#region ECDIS Lists
	/// <summary>
	/// List containing the ECDIS time of day command parameters
	/// </summary>
	public class EcdisCmdList : List<ParamContainer>
	{
		private static EcdisCmdList instance;
		private EcdisCmdList()
		{
			Add(new ParamContainer("Day", ISIC_SCP_IF.BYTE_DATA_ECD_DAY));
			Add(new ParamContainer("Dusk", ISIC_SCP_IF.BYTE_DATA_ECD_DUSK));
			Add(new ParamContainer("Night", ISIC_SCP_IF.BYTE_DATA_ECD_NIGHT));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static EcdisCmdList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new EcdisCmdList();
				}
				return instance;
			}
		}
	}
	#endregion ECDIS Lists

	#region MCC Lists
	/// <summary>
	/// List containing the MCC command parameters
	/// </summary>
	public class MccCmdList : List<ParamContainer>
	{
		private static MccCmdList instance;
		private MccCmdList()
		{
			Add(new ParamContainer("MainPictureChannel", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_MPC));
			Add(new ParamContainer("Backlight", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_BKL));
			Add(new ParamContainer("Brightness", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_BRI));
			Add(new ParamContainer("Contrast", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_CON));
			Add(new ParamContainer("Power", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_PWR));
			Add(new ParamContainer("ColorTemp", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_CT));
			Add(new ParamContainer("GainRed", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_GR));
			Add(new ParamContainer("GainGreen", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_GG));
			Add(new ParamContainer("GainBlue", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_GB));
			Add(new ParamContainer("Gamma", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_GMA));
			Add(new ParamContainer("LockMonID", ISIC_SCP_IF.BYTE_DATA_MCC_ADDR_LOCK));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static MccCmdList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MccCmdList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the main picture channel command parameters
	/// </summary>
	public class MccMPCList : List<ParamContainer>
	{
		private static MccMPCList instance;
		private MccMPCList()
		{
			Add(new ParamContainer("VGA", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_VGA));
			Add(new ParamContainer("DVI", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_DVI));
			Add(new ParamContainer("DP/HDMI", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_DP));
			Add(new ParamContainer("Return active ch", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_MPC_RAC));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static MccMPCList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MccMPCList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the power command parameters
	/// </summary>
	public class MccPWRList : List<ParamContainer>
	{
		private static MccPWRList instance;
		private MccPWRList()
		{
			Add(new ParamContainer("ON", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_PWR_ON));
			Add(new ParamContainer("OFF", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_PWR_OFF));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static MccPWRList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MccPWRList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the colour temperature command parameters
	/// </summary>
	public class MccCTList : List<ParamContainer>
	{
		private static MccCTList instance;
		private MccCTList()
		{
			Add(new ParamContainer("4200K", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_CT_4200K));
			Add(new ParamContainer("5000K", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_CT_5000K));
			Add(new ParamContainer("5400K", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_CT_5400K));
			Add(new ParamContainer("6500K", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_CT_6500K));
			Add(new ParamContainer("7500K", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_CT_7500K));
			Add(new ParamContainer("9300K", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_CT_9300K));
			Add(new ParamContainer("USER", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_CT_USER));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static MccCTList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MccCTList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the gamma command parameters
	/// </summary>
	public class MccGMAList : List<ParamContainer>
	{
		private static MccGMAList instance;
		private MccGMAList()
		{
			Add(new ParamContainer("Native", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_GMA_NATIVE));
			Add(new ParamContainer("2.2", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_GMA_2_2));
			Add(new ParamContainer("Custom", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_GMA_CUSTOM));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static MccGMAList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MccGMAList();
				}
				return instance;
			}
		}
	}

	/// <summary>
	/// List containing the LockMonID command parameters
	/// </summary>
	public class MccLOCKList : List<ParamContainer>
	{
		private static MccLOCKList instance;
		private MccLOCKList()
		{
			Add(new ParamContainer("0", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_LOCK_ON));
			Add(new ParamContainer("1", ISIC_SCP_IF.BYTE_DATA_MCC_VALUE_LOCK_OFF));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static MccLOCKList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MccLOCKList();
				}
				return instance;
			}
		}
	}
	#endregion // MCC Lists

	#region MEO Lists
	/// <summary>
	/// List containing the MEO command parameters
	/// </summary>
	public class MeoCmdList : List<ParamContainer>
	{
		private static MeoCmdList instance;
		private MeoCmdList()
		{
			Add(new ParamContainer("AutoAdjustVGA", ISIC_SCP_IF.BYTE_DATA_MEO_AAVGA));
			Add(new ParamContainer("VGASave", ISIC_SCP_IF.BYTE_DATA_MEO_VGAS));
			Add(new ParamContainer("VGALoad", ISIC_SCP_IF.BYTE_DATA_MEO_VGAL));
			Add(new ParamContainer("AutoCOlorAdjust", ISIC_SCP_IF.BYTE_DATA_MEO_ACA));
			Add(new ParamContainer("MonitorSave", ISIC_SCP_IF.BYTE_DATA_MEO_MS));
			Add(new ParamContainer("MonitorLoad", ISIC_SCP_IF.BYTE_DATA_MEO_ML));
			Add(new ParamContainer("LoadFactoryDefaults", ISIC_SCP_IF.BYTE_DATA_MEO_LFD));
		}

		/// <summary>
		/// Returns the singleton instance of the list
		/// </summary>
		public static MeoCmdList Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MeoCmdList();
				}
				return instance;
			}
		}
	}
	#endregion // MEO Lists

	/// <summary>
	/// Class containing the parameters for various lists
	/// </summary>
	public class ParamContainer
	{
		public byte[] Data { get; set; }
		public String Name { get; set; }

		public ParamContainer(String name, params byte[] data)
		{
			this.Data = data;
			this.Name = name;
		}

		public ParamContainer(String name, String data)
		{
			this.Name = name;
			this.Data = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				this.Data[i] = (byte)data[i];
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}