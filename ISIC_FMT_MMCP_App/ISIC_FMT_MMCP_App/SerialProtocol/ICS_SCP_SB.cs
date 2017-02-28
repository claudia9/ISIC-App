namespace Isic.SerialProtocol
{
	/// <summary>
	/// Wrapper class for ICS menu serial commands
	/// Baud rate ?
	/// </summary>
	public static class ICS_SCP_SB
	{
		public static byte[] ICSUp
		{
			get
			{
				return new byte[] { 0x06, 0x00, 0x4B, 0x4D, 0x55, 0x0D };
			}
		}

		public static byte[] ICSDown
		{
			get
			{
				return new byte[] { 0x06, 0x00, 0x4B, 0x4D, 0x44, 0x1E };
			}
		}

		public static byte[] ICSMenu
		{
			get
			{
				return new byte[] { 0x06, 0x00, 0x4B, 0x4D, 0x4E, 0x14 };
			}
		}

		public static byte[] ICSEnter
		{
			get
			{
				return new byte[] { 0x06, 0x00, 0x4B, 0x4D, 0x4F, 0x13 };
			}
		}

		public static byte[] ICSPower
		{
			get
			{
				return new byte[] { 0x06, 0x00, 0x4B, 0x50, 0x57, 0x08 };
			}
		}
	}
}
