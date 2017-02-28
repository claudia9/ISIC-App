using System;

namespace Isic.SerialProtocol
{
	/// <summary>
	/// Class for calculating the checksum for the ISIC serial protocol commands
	/// </summary>
	public static class Checksum
	{
		private static byte _ref = 0xFF;

		/// <summary>
		/// Checks if the header checksum is correct
		/// </summary>
		/// <remarks>
		/// Returns null if cmd is not long enough to be a header
		/// </remarks>
		/// <param name="cmd"></param>
		/// <returns>status of the checksum</returns>
		public static bool? IHCHK_CHK(byte[] cmd)
		{
			return cmd.Length > 6 ? (bool?)(cmd[ISIC_SCP_IF.BYTE_INDEX_IHCHK] == IHCHK_GEN(cmd)) : null;
		}

		/// <summary>
		/// Checks if the data checksum is correct
		/// </summary>
		/// <remarks>
		/// Returns null if cmd is not long enough to have data
		/// </remarks>
		/// <param name="cmd"></param>
		/// <returns>status of the checksum</returns>
		public static bool? IDCHK_CHK(byte[] cmd)
		{
			return cmd.Length > 8 ? (bool?)(cmd[cmd.Length - 1] == IDCHK_GEN(cmd)) : null; ;
		}

		/// <summary>
		/// Returns the calculated checksum for the header
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns>the checksum byte</returns>
		public static byte IHCHK_GEN(byte[] cmd)
		{
			if (cmd.Length < ISIC_SCP_IF.BYTE_INDEX_IHCHK + 1)
			{
				throw new IndexOutOfRangeException(String.Format("The command has to be at least {0} characters long", ISIC_SCP_IF.BYTE_INDEX_IHCHK + 1));
			}
			byte sum = 0;
			for (int i = 0; i < ISIC_SCP_IF.BYTE_INDEX_IHCHK; i++)
			{
				sum += cmd[i];
			}
			return (byte)(_ref - sum);
		}

		/// <summary>
		/// Returns the calculated checksum for the data
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns>the checksum byte</returns>
		public static byte IDCHK_GEN(byte[] cmd)
		{
			int dataLength = cmd[ISIC_SCP_IF.BYTE_INDEX_LEN];
			if (dataLength < 1 || cmd.Length < dataLength + 8)
			{
				throw new IndexOutOfRangeException(String.Format("This command has to be at least {0} characters long", dataLength + 8));
			}
			byte sum = 0;
			for (int i = ISIC_SCP_IF.BYTE_INDEX_IHCHK + 1; i < cmd.Length - 1; i++)
			{
				sum += cmd[i];
			}
			return (byte)(_ref - sum);
		}
	}
}
