using System;

namespace Isic.SerialProtocol.Exceptions
{
	/// <summary>
	/// Part number not valid exception
	/// </summary>
	public class PartNoNotValidException : Exception
	{/// <summary>
	 /// Zero argument constructor
	 /// </summary>
		public PartNoNotValidException()
		{

		}

		/// <summary>
		/// One argument constructor
		/// </summary>
		/// <param name="Message">Describing message</param>
		public PartNoNotValidException(String Message)
				: base(Message)
			{

		}

		/// <summary>
		/// Two argument constructor
		/// </summary>
		/// <param name="Message">Describing message</param>
		/// <param name="InnerException">Child exception</param>
		public PartNoNotValidException(String Message, Exception InnerException)
				: base(Message, InnerException)
			{

		}
	}
}
