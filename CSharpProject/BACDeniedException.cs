using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public class BACDeniedException : CardServiceException
	{
		private readonly BACKeySpec bacKey;

		public BACDeniedException(string message, BACKeySpec bacKey, int statusWord)
			: base(message)
		{
			this.bacKey = bacKey;
		}

		public BACKeySpec GetBACKey() => bacKey;
	}
}


