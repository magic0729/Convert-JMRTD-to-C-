using System;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.protocol
{
	[Serializable]
	public class BACResult
	{
		private readonly IAccessKeySpec? bacKey;
		private readonly SecureMessagingWrapper wrapper;

		public BACResult(SecureMessagingWrapper wrapper) : this(null, wrapper) { }

		public BACResult(IAccessKeySpec? bacKey, SecureMessagingWrapper wrapper)
		{
			this.bacKey = bacKey;
			this.wrapper = wrapper;
		}

		public IAccessKeySpec? GetBACKey() => bacKey;
		public SecureMessagingWrapper GetWrapper() => wrapper;
		public IAccessKeySpec? BACKey => bacKey;
		public SecureMessagingWrapper Wrapper => wrapper;

		public override string ToString() => $"BACResult [bacKey: {(bacKey is null ? "-" : bacKey.ToString())}, wrapper: {wrapper}]";
	}
}


