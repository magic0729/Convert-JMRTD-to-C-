using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public class AccessDeniedException : CardServiceException
	{
		private readonly AccessKeySpec? bacKey;

		public AccessDeniedException(string message, int statusWord)
			: this(message, null, statusWord)
		{
		}

		public AccessDeniedException(string message, AccessKeySpec? bacKey, int statusWord)
			: base(message)
		{
			this.bacKey = bacKey;
		}

		public AccessKeySpec? GetAccessKey() => bacKey;
	}
}


