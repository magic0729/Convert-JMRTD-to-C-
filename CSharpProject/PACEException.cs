namespace org.jmrtd
{
	public class PACEException : CardServiceProtocolException
	{
		public PACEException(string message, int step) : base(message, step) { }
		public PACEException(string message, int step, System.Exception cause) : base(message, step, cause) { }
		public PACEException(string message, int step, int statusWord) : base(message, step, statusWord) { }
		public PACEException(string message, int step, System.Exception cause, int statusWord) : base(message, step, cause, statusWord) { }
	}
}


