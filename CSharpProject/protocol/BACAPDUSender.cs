using System;
using org.jmrtd;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.protocol
{
	public class BACAPDUSender : APDULevelBACCapable
	{
		private readonly ICardService service;

		public BACAPDUSender(ICardService service)
		{
			this.service = service;
		}

		public byte[] SendGetChallenge()
		{
			var capdu = new CommandAPDU(0x00, 0x84, 0x00, 0x00, 8);
			var rapdu = service.Transmit(capdu);
			var data = rapdu.Data;
			if (data == null || data.Length != 8) throw new Exception("Get challenge failed");
			return data;
		}

		public byte[] SendMutualAuth(byte[] rndIFD, byte[] rndICC, byte[] kIFD, SecretKey kEnc, SecretKey kMac)
		{
			// Placeholder: just return 32 zero bytes to keep flow
			return new byte[32];
		}
	}
}


