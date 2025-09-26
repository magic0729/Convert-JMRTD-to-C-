using System;

namespace org.jmrtd.CustomJavaAPI
{
	// Shim for net.sf.scuba.smartcards.CardServiceException from Java.
	// Provides a status word and standard Exception behavior for .NET.
	public class CardServiceException : Exception
	{
		public int StatusWord { get; }
		public int SW => StatusWord; // Compatibility property

		public CardServiceException(string message)
			: base(message)
		{
			StatusWord = -1;
		}

		public CardServiceException(string message, Exception? innerException)
			: base(message, innerException)
		{
			StatusWord = -1;
		}

		public CardServiceException(string message, int statusWord)
			: base(message)
		{
			StatusWord = statusWord;
		}

		public CardServiceException(string message, Exception? innerException, int statusWord)
			: base(message, innerException)
		{
			StatusWord = statusWord;
		}
	}
}


