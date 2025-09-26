using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public class CardServiceProtocolException : CardServiceException
	{
		private readonly int step;

		public CardServiceProtocolException(string message, int step)
			: base(message)
		{
			this.step = step;
		}

		public CardServiceProtocolException(string message, int step, System.Exception cause)
			: base(message, cause)
		{
			this.step = step;
		}

		public CardServiceProtocolException(string message, int step, int statusWord)
			: base(message)
		{
			this.step = step;
		}

		public CardServiceProtocolException(string message, int step, System.Exception cause, int statusWord)
			: base(message, cause)
		{
			this.step = step;
		}

		public int GetStep() => step;

		public override string Message => base.Message + " (" + "step: " + step + ")";
	}
}


