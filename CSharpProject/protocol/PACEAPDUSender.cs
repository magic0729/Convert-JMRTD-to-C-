using System;
using org.jmrtd;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.protocol
{
	public class PACEAPDUSender : APDULevelPACECapable
	{
		public const byte NO_PACE_KEY_REFERENCE = 0;
		public const byte MRZ_PACE_KEY_REFERENCE = 1;
		public const byte CAN_PACE_KEY_REFERENCE = 2;
		public const byte PIN_PACE_KEY_REFERENCE = 3;
		public const byte PUK_PACE_KEY_REFERENCE = 4;

		private readonly SecureMessagingAPDUSender secureMessagingSender;

		public PACEAPDUSender(ICardService service)
		{
			secureMessagingSender = new SecureMessagingAPDUSender(service);
		}

		public void SendMSESetATMutualAuth(APDUWrapper wrapper, string oid, int refPublicKeyOrSecretKey, byte[]? refPrivateKeyOrForComputingSessionKey)
		{
			if (oid == null) throw new ArgumentException("OID cannot be null");
			// Minimal encoding: tag 0x80 for OID is not implemented in TLV here; send empty data
			var capdu = new CommandAPDU(0x00, 0x22, 0xC1, 0xA4, Array.Empty<byte>());
			var rapdu = secureMessagingSender.transmit(wrapper, capdu);
			if (rapdu.StatusWord != 0x9000) throw new Exception("Sending MSE AT failed");
		}

		public byte[] SendGeneralAuthenticate(APDUWrapper wrapper, byte[] data, int le, bool isLast)
		{
			int cla = isLast ? 0x00 : 0x10;
			var capdu = new CommandAPDU(cla, 0x8A, 0x00, 0x00, data, le);
			var rapdu = secureMessagingSender.transmit(wrapper, capdu);
			if (rapdu.StatusWord != 0x9000) throw new Exception("Sending general authenticate failed");
			return rapdu.Data ?? Array.Empty<byte>();
		}
	}
}


