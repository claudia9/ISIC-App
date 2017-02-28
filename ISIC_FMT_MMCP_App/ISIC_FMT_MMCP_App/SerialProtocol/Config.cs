using ISIC_FMT_MMCP_App;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Isic.SerialProtocol
{
	/// <summary>
	/// Class wrapping the ISIC serial protocol config commands
	/// </summary>
	public class Config : IComparable
	{

		public byte CfgCmd { get; set; }
		public String Name { get; set; }
		public byte[] Address { get; set; }
		public byte[] Value { get; set; }

		#region Constructors
		/// <summary>
		/// Constructor taking a byte array with the address of the config data and a byte array with the value in ASCII characters... smfh
		/// </summary>
		/// <param name="cfgCmd"></param>
		/// <param name="addr"></param>
		/// <param name="value"></param>
		public Config(byte cfgCmd, byte[] addr, byte[] value)
		{
			this.CfgCmd = cfgCmd;
			this.Address = addr;
			this.Value = value;
		}

		/// <summary>
		/// Constructor taking a byte array with the address of the config data and a byte array with the value in ASCII characters... smfh
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="value"></param>
		public Config(byte[] addr, byte[] value) : this(ISIC_SCP_IF.BYTE_DATA_CFG_CMD_C, addr, value) { }

		/// <summary>
		/// Constructor taking a byte array with the address of the config data and a string with the value in ASCII... smfh
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="value"></param>
		public Config(byte[] addr, String value) : this(addr, value.GetBytes()) { }

		/// <summary>
		/// Constructor taking a string with the address in ASCII of the config data and a byte array with the value in ASCII characters... smfh
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="value"></param>
		public Config(String addr, byte[] value) : this(addr.GetBytes(), value) { }

		/// <summary>
		/// Constructor taking a string with the address in ASCII of the config data and a string with the value in ASCII... smfh
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="value"></param>
		public Config(String addr, String value) : this(addr, value.GetBytes()) { }

		/// <summary>
		/// Constructor taking a String with the name, a byte array with the address of the config data and a byte array with the value in ASCII characters... smfh
		/// </summary>
		/// <param name="name"></param>
		/// <param name="addr"></param>
		/// <param name="value"></param>
		public Config(String name, byte[] addr, byte[] value) : this(ISIC_SCP_IF.BYTE_DATA_CFG_CMD_C, addr, value)
		{
			this.Name = name;
		}
		#endregion // Constructors

		/// <summary>
		/// Enters the config
		/// </summary>
		/// <param name="sp"></param>
		/// <param name="expectedReply"></param>
		/// <param name="deviceAddr"></param>
		/// <param name="timeout"></param>
		/// <returns>status of operation</returns>
		public async static Task<bool> Enter(ICharacteristic characteristic, byte expectedReply = 0x06, int timeout = 500, byte deviceAddr = ISIC_SCP_IF.BYTE_BROADCAST_ADDR)
		{
			byte[] replyArr = null;
            if (await characteristic.WriteAsync(new Command(deviceAddr, ISIC_SCP_IF.CMD_CFG).GetBytes()))
            {
                return replyArr != null ? replyArr[0] == expectedReply : false;
            }
			//if (Comm.Write(sp, new Command(deviceAddr, ISIC_SCP_IF.CMD_CFG).GetBytes(), out replyArr, 1, timeout))
			//if (new Command(ISIC_SCP_IF.BYTE_BROADCAST_ADDR, ISIC_SCP_IF.CMD_CFG).Send(sp, out replyArr, 50))
			return false;
		}

		/// <summary>
		/// Exits the config
		/// </summary>
		/// <param name="sp"></param>
		/// <param name="expectedReply"></param>
		/// <returns>status of operation</returns>
		public static bool Exit(ICharacteristic characteristic, byte expectedReply = 0x06, int timeout = 500)
		{
			byte[] replyArr = null;
			characteristic.WriteAsync(ISIC_SCP_IF.CMD_CFG_EXIT);

			return replyArr != null ? replyArr[0] == expectedReply : false;
		}

		/// <summary>
		/// Get the complete config in a byte array
		/// </summary>
		/// <returns></returns>
		public byte[] GetBytes()
		{
			byte[] bytes = new byte[10]; bytes[0] = ISIC_SCP_IF.BYTE_DATA_CFG_ATTN;
			bytes[1] = CfgCmd;
			bytes[2] = Address[0];
			bytes[3] = Address[1];
			if (Value != null)
			{
				for (int i = 0; i < 4; i++)
				{
					bytes[i + 4] = 3 - i < Value.Length ? Value[Value.Length - 1 - (3 - i)] : (byte)'0';
				}
			}
			SetChecksumCFG(bytes);
			return bytes;
		}

        /// <summary>
        /// Sends the config and checks the reply
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="checkReply"></param>
        /// <param name="expectedReply"></param>
        /// <returns>status of operation</returns>
        public async Task<bool> Send(ICharacteristic characteristic, bool checkReply = true, byte expectedReply = 0x06)
        {
            byte[] bytes = GetBytes();
            try
            {
                if (checkReply)
                {
                    byte[] replyArr;
                    await characteristic.WriteAsync(bytes);
                    replyArr = await characteristic.ReadAsync();
                    return replyArr != null ? replyArr[0] == expectedReply : false;
                }
                await characteristic.WriteAsync(bytes);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred while trying to execute CFG: [{0}]. Exception:\n{1}\n{2}", bytes.GetHexString(), ex.Message, ex.StackTrace);

                return false;
            }
        }

        /// <summary>
        /// Writes the compiled values to the supplied port
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        /// <returns>status of operation</returns>
        private async Task<bool> ComWriteCFG(ICharacteristic characteristic, String addr, String value)
        {
            return await ComWriteCFG(characteristic, addr.GetBytes(), value != null ? value.GetBytes() : null);
        }

        /// <summary>
        /// Writes the compiled values to the supplied port
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="addr"></param>
        /// <param name="value"></param>
        /// <returns>status of operation</returns>
        private async Task<bool> ComWriteCFG(ICharacteristic characteristic, byte[] addr, byte[] value)
        {
            byte[] cmd = new byte[10];
            try
            {
                cmd[0] = ISIC_SCP_IF.BYTE_DATA_CFG_ATTN;
                cmd[1] = ISIC_SCP_IF.BYTE_DATA_CFG_CMD_C;
                cmd[2] = addr[0];
                cmd[3] = addr[1];
                for (int i = 0; i < 4; i++)
                {
                    cmd[i + 4] = value != null && 3 - i < value.Length ? value[value.Length - 1 - (3 - i)] : (byte)'0';
                }
                SetChecksumCFG(cmd);
                byte[] replyArr;
                await characteristic.WriteAsync(cmd);
                replyArr = await characteristic.ReadAsync();
                return replyArr != null ? replyArr[0] == 0x06 : false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred while trying to execute CFG: [{0}]. Exception:\n{1}\n{2}", cmd.GetHexString(), ex.Message, ex.StackTrace);

                return false;
            }
        }

        public static void SetChecksumCFG(byte[] cmd)
		{
			byte sum = 0;
			for (int i = 1; i < cmd.Length - 2; i++)
			{
				sum += cmd[i];
			}
			String sum_s = sum.ToString("X2");
			cmd[cmd.Length - 2] = (byte)sum_s[0];
			cmd[cmd.Length - 1] = (byte)sum_s[1];
		}


		public bool AddressEquals(object obj)
		{
			Config other = obj as Config;
			return other != null & this.Address.SequenceEqual(other.Address);
		}

		public override bool Equals(object obj)
		{
			Config other = obj as Config;
			return other != null & this.Name.Equals(other.Name) & this.Address.SequenceEqual(other.Address) & this.Value.SequenceEqual(other.Value);
		}

		public int CompareTo(object obj)
		{
			Config other = obj as Config;
			if (other != null)
			{
				int localAddr = Convert.ToInt16(Address.GetString(), 16);
				int otherAddr = Convert.ToInt16(other.Address.GetString(), 16);
				return localAddr.CompareTo(otherAddr);
			}
			return -2;
		}

		public override string ToString()
		{
			return String.Format("   {0}\tAddress: 0x{1}\tValue: 0x{2}", Name.PadRight(25, ' '), Address.GetString(), Value.GetString());
		}

		/// <summary>
		/// Formats content to html
		/// </summary>
		/// <note>
		/// Not tested
		/// </note>
		/// <returns></returns>
		public string ToHtmlString()
		{
			return String.Format("<tr><td>{0}</td><td>Address:</td><td>0x{1}</td><td>Value:</td><td>0x{2}</td></tr>", Name.PadRight(25, ' '), Address.GetString(), Value.GetString());
		}
	}
}
