using ISIC_FMT_MMCP_App;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Text;
using System.Threading.Tasks;

namespace Isic.SerialProtocol
{
	/// <summary>
	/// Class wrapping the ISIC serial protocol commands
	/// </summary>
	public class Command : IComparable
	{
		public byte[] _cmd { get; set; }

		/// <summary>
		/// Gets or sets the Attn byte
		/// </summary>
		public byte Attn
		{
			get
			{
				return _cmd[ISIC_SCP_IF.BYTE_INDEX_ATTN];
			}
			set
			{
				_cmd[ISIC_SCP_IF.BYTE_INDEX_ATTN] = value;
				SetIHCHK();
			}
		}

		/// <summary>
		/// Gets or sets the Addr byte
		/// </summary>
		public byte Addr
		{
			get
			{
				return _cmd[ISIC_SCP_IF.BYTE_INDEX_ADDR];
			}
			set
			{
				_cmd[ISIC_SCP_IF.BYTE_INDEX_ADDR] = value;
				SetIHCHK();
			}
		}

		public byte Length
		{
			get { return _cmd[ISIC_SCP_IF.BYTE_INDEX_LEN]; }
		}

		public String Cmd
		{
			get
			{
				StringBuilder s = new StringBuilder();
				for (int i = ISIC_SCP_IF.BYTE_INDEX_CMD1; i < ISIC_SCP_IF.BYTE_INDEX_CMD3 + 1; i++)
				{
					s.Append((char)_cmd[i]);
				}
				return s.ToString();
			}
			set
			{
				int i = 0;
				foreach (char c in value)
				{
					_cmd[ISIC_SCP_IF.BYTE_INDEX_CMD1 + i++] = (byte)c;
				}
			}
		}

		public byte[] Data
		{
			get
			{
				byte length = _cmd[ISIC_SCP_IF.BYTE_INDEX_LEN];
				byte[] ret = new byte[length];
				Array.Copy(_cmd, ISIC_SCP_IF.BYTE_INDEX_IHCHK + 1, ret, 0, length);
				return ret;
			}
			set
			{
				if (value != null)
				{
					Array.Copy(value, 0, _cmd, ISIC_SCP_IF.BYTE_INDEX_IHCHK + 1, value.Length);
					int i = 0;
					foreach (byte b in value)
					{
						_cmd[ISIC_SCP_IF.BYTE_INDEX_IHCHK + 1 + i++] = b;
					}
					_cmd[ISIC_SCP_IF.BYTE_INDEX_LEN] = (byte)value.Length;
					SetIDCHK();
				}
			}
		}

		//public byte Attn { get; set; }
		//public byte Addr { get; set; }
		//public String Cmd { get; set; }
		//public byte Lenght { get; set; }
		//public byte[] Data { get; set; }

		#region Constructors
		/// <summary>
		/// Zero argument constructor
		/// </summary>
		public Command()
		{
		}

		/// <summary>
		/// Constructor taking a String containing the command
		/// </summary>
		/// <param name="cmd"></param>
		//public Command(String cmd) : this(ISIC_SCP_IF.BYTE_BROADCAST_ADDR, cmd) { }

		/// <summary>
		/// Constructor taking a String containing the command and a byte array containing the data
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="data"></param>
		public Command(String cmd, params byte[] data) : this(ISIC_SCP_IF.BYTE_BROADCAST_ADDR, cmd, data) { }

		/// <summary>
		/// Constructor taking a byte with the device address and a String containing the command
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="cmd"></param>
		public Command(byte addr, String cmd) : this(ISIC_SCP_IF.BYTE_ATTN_CMD, addr, cmd, null) { }

		/// <summary>
		/// Constructor taking a byte with the device address, a string containing the command and a byte array containing the data
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="cmd"></param>
		/// <param name="data"></param>
		public Command(byte addr, String cmd, params byte[] data) : this(ISIC_SCP_IF.BYTE_ATTN_CMD, addr, cmd, data) { }

		/// <summary>
		/// Constructor taking a byte with the attention value, a byte with the device address, a string containing the command and a byte array containing the data
		/// </summary>
		/// <param name="attn"></param>
		/// <param name="addr"></param>
		/// <param name="cmd"></param>
		/// <param name="data"></param>
		public Command(byte attn, byte addr, String cmd, params byte[] data) : this()
		{
			int len = data == null || data.Length == 0 ? (byte)0x00 : (byte)data.Length + 1;
			_cmd = new byte[ISIC_SCP_IF.HEADER_LENGTH + len];
			Attn = attn;
			Addr = addr;
			Cmd = cmd;
			Data = data;
			SetIHCHK();
		}
		#endregion // Constructors

		#region Send command
		/// <summary>
		/// Sends the command on the port supplied
		/// </summary>
		/// <param name="port"></param>
		/// <returns>status of the transmission</returns>
		public async void Send(ICharacteristic characteristic, Action<bool> callback = null)
		{
            if(characteristic.CanWrite)
            {
                try
                {
                    Byte[] bytes = GetBytes();
                    Debug.WriteLine("Sending: " + bytes.GetHexString() + " Characteristic params: " + characteristic.Name + characteristic.Value);
                    //System.FormatException: Index (zero based) must be greater than or equal to zero and less than the size of the argument list.
                    try
                    {
                        await characteristic.WriteAsync(bytes);
                        Debug.WriteLine("Sent successfully");
                    } catch (Exception e)
                    {
                        Debug.WriteLine("Command not send! - " + e.Message);
                    }
                    //callback?.Invoke(await characteristic.WriteAsync(bytes));
                    //Debug.WriteLine("After callback of WriteAsync.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error sending:\nMessage: {0}\nStackTrace:\n{1}\nInner ex:\n{2}\nData:\n{3}", ex.Message, ex.StackTrace, ex.InnerException, ex.Data);
                    //callback?.Invoke(false);
                }
            } else
            {
                Debug.WriteLine("Characteristic " + characteristic.Value + " cannot write right now.");
            }
		}

		/// <summary>
		/// Sends the command on the port supplied and returns the reply using the timeout supplied
		/// </summary>
		/// <param name="port"></param>
		/// <param name="reply"></param>
		/// <param name="timeout"></param>
		/// <returns>status of the transmission</returns>
		//public bool Send(ICharacteristic characteristic)
		//{
		//	return Send(port, true, out reply, timeout);
		//}

		/// <summary>
		/// Sends the command on the port supplied and returns the reply using the timeout supplied, if waitForRepply is true
		/// </summary>
		/// <remarks>
		/// enables debugging in the log if debug is true
		/// </remarks>
		/// <param name="port"></param>
		/// <param name="waitForReply"></param>
		/// <param name="reply"></param>
		/// <param name="timeout"></param>
		/// <param name="debug"></param>
		/// <returns>status of the transmission</returns>
		//public bool Send(ICharacteristic characteristic, bool waitForReply, out byte[] reply, int timeout = 500, bool debug = false)
		//{
		//	byte dataLength = 0;
		//	byte[] header = new byte[ISIC_SCP_IF.HEADER_LENGTH];

		//	reply = null;
  //          //Task<bool> t = new Task<bool>(() => Send(null));
		//	//if (!Send(characteristic))
		//	//{
		//		//return false;
		//	//}

		//	if (waitForReply)
		//	{
		//		Stopwatch sw = new Stopwatch();
		//		sw.Start();
		//		if (debug) Debug.WriteLine("Waiting for reply...");
		//		/*while (characteristic.BytesToRead < ISIC_SCP_IF.HEADER_LENGTH)
		//		{
		//			if (sw.Elapsed.TotalMilliseconds > timeout)
		//			{
		//				Debug.WriteLine("Timeout. Bytes to read: {0}", characteristic.BytesToRead);
		//				reply = null;
		//				return false;
		//			}
		//		}*/
		//		try
		//		{
		//			if (debug) Debug.WriteLine("Trying to read header: bytes to read: - header.Length: {0}", header.Length);
		//			characteristic.ReadAsync();

		//			if (Checksum.IHCHK_CHK(header) != true)
		//			{
		//				reply = null;
		//				//Debug.WriteLine("Checksum error. Reply: " + header.GetHexString());
		//				return false;
		//			}
		//			dataLength = header[ISIC_SCP_IF.BYTE_INDEX_LEN];
		//			reply = new byte[ISIC_SCP_IF.HEADER_LENGTH + dataLength + 1];
		//			Array.Copy(header, reply, ISIC_SCP_IF.HEADER_LENGTH);
		//			if (reply == null)
		//			{
		//				Debug.WriteLine("Array.Copy failed: header.Length: {0} - reply.Length: {1} - size to be copied: {2}", header.Length, reply.Length, ISIC_SCP_IF.HEADER_LENGTH);
		//				throw new NullReferenceException("Array was not copied properly");
		//			}
		//			sw.Restart();
		//			if (dataLength > 0)
		//			{
		//				/*while (port.BytesToRead < dataLength + 1)
		//				{
		//					if (sw.Elapsed.TotalMilliseconds > timeout)
		//					{
		//						Debug.WriteLine("Error waiting for data. Bytes sent: {0}\n# of bytes to read: {1}", this.GetBytes(), port.BytesToRead);
		//						reply = null;
		//						return false;
		//					}
		//				}*/
		//				sw.Reset();
		//				if (debug) Debug.WriteLine("Trying to read data: bytes to read: {0} - dataLength: {1} - reply.Length: {2}", dataLength, reply.Length);
		//				try
		//				{
		//					characteristic.ReadAsync();
		//				}
		//				catch (Exception ex)
		//				{
		//					Debug.WriteLine("Exception trying to read serial data. replyLength: {0}, dataLength: {1}", reply.Length, dataLength);
		//					Debug.WriteLine("Exception {0}", ex);
		//					Debug.WriteLine("Data currently in reply buf: {0}", reply.GetHexString());
		//					List<byte> ex_tmp = new List<byte>();
		//					//while (port.BytesToRead > 0)
		//					//{
		//						ex_tmp.Add(Convert.ToByte(characteristic.ReadAsync()));
		//					//}
		//					Debug.WriteLine("Data left in port buf: {0}", ex_tmp.ToArray().GetHexString());
		//					throw;
		//				}
		//				if (Checksum.IDCHK_CHK(reply) != true)
		//				{
		//					Debug.WriteLine("Error in data chk. Reply: " + reply.GetHexString());
		//					reply = null;
		//					return false;
		//				}
		//			}
		//		}
		//		catch (Exception ex)
		//		{
		//			Debug.WriteLine("Trying to read data: bytes to read: {0} - dataLength: {1} - reply.Length: {2}", port.BytesToRead, dataLength, reply.Length);
		//			Debug.WriteLine("Exception: {0}", ex.StackTrace);
		//			reply = null;
		//			return false;
		//		}
		//	}

		//	return true;
		//}
		#endregion // Send command

		#region Checksum
		/// <summary>
		/// Sets the header checksum
		/// </summary>
		private void SetIHCHK()
		{
			_cmd[ISIC_SCP_IF.BYTE_INDEX_IHCHK] = Checksum.IHCHK_GEN(_cmd);
		}

		/// <summary>
		/// Sets the data checksum
		/// </summary>
		private void SetIDCHK()
		{
			if (Length > 0)
			{
				_cmd[_cmd.Length - 1] = Checksum.IDCHK_GEN(_cmd);
			}
		}
		#endregion // Checksum

		/// <summary>
		/// Gets the bytes in the command
		/// </summary>
		/// <returns>the bytes in the command</returns>
		public byte[] GetBytes()
		{
			byte[] copy = new byte[_cmd.Length];
			Array.Copy(_cmd, copy, _cmd.Length);
			return copy;
		}

		/// <summary>
		/// Equals
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			Command other = obj as Command;
			return other != null & this.Cmd.Equals(other.Cmd);
		}

		/// <summary>
		/// CompareTo
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj)
		{
			Command other = obj as Command;
			if (other != null)
			{
				return this.Cmd.CompareTo(other.Cmd);
			}
			return -2;
		}
	}
}
