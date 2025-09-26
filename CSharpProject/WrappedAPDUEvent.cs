using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public class WrappedAPDUEvent : APDUEvent
	{
		private readonly CommandAPDU plainTextCommandAPDU;
		private readonly ResponseAPDU plainTextResponseAPDU;

		public WrappedAPDUEvent(
			object source,
			object? type,
			int sequenceNumber,
			CommandAPDU plainTextCommandAPDU,
			ResponseAPDU plainTextResponseAPDU,
			CommandAPDU wrappedCommandAPDU,
			ResponseAPDU wrappedResponseAPDU) : base(source, type, sequenceNumber, wrappedCommandAPDU, wrappedResponseAPDU)
		{
			this.plainTextCommandAPDU = plainTextCommandAPDU;
			this.plainTextResponseAPDU = plainTextResponseAPDU;
		}

		public CommandAPDU GetPlainTextCommandAPDU() => plainTextCommandAPDU;
		public ResponseAPDU GetPlainTextResponseAPDU() => plainTextResponseAPDU;
	}
}


