using System;

namespace Isic.SerialProtocol.Exceptions
{
	/// <summary>
	/// Board not recognised exception
	/// </summary>
	public class BoardNotRecognisedException : Exception
	{
		/// <summary>
		/// Zero argument constructor
		/// </summary>
		public BoardNotRecognisedException()
		{

		}

		/// <summary>
		/// One argument constructor
		/// </summary>
		/// <param name="Message">Describing message</param>
		public BoardNotRecognisedException(String Message)
				: base(Message)
			{

		}

		/// <summary>
		/// Two argument constructor
		/// </summary>
		/// <param name="Message">Describing message</param>
		/// <param name="InnerException">Child exception</param>
		public BoardNotRecognisedException(String Message, Exception InnerException)
				: base(Message, InnerException)
			{

		}
	}
}
