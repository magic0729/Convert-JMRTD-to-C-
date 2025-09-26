using System;
using System.Security.Cryptography;
using org.jmrtd;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.protocol
{
	public class BACProtocol
	{
		private readonly APDULevelBACCapable service;
		private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
		private readonly int maxTranceiveLength;
		private readonly bool shouldCheckMAC;

		public BACProtocol(APDULevelBACCapable service, int maxTranceiveLength, bool shouldCheckMAC)
		{
			this.service = service;
			this.maxTranceiveLength = maxTranceiveLength;
			this.shouldCheckMAC = shouldCheckMAC;
		}

		public BACResult DoBAC(IAccessKeySpec bacKey)
		{
			byte[] keySeed = bacKey.GetKey();
			var kEnc = Util.DeriveKey(keySeed, Util.ENC_MODE);
			var kMac = Util.DeriveKey(keySeed, Util.MAC_MODE);
			var wrapper = DoBACStep(kEnc, kMac);
			return new BACResult(bacKey, new SecureMessagingWrapperShim(wrapper));
		}

		public BACResult DoBAC(SecretKey kEnc, SecretKey kMac)
		{
			var wrapper = DoBACStep(kEnc, kMac);
			return new BACResult(new SecureMessagingWrapperShim(wrapper));
		}

		private SecureMessagingWrapper DoBACStep(SecretKey kEnc, SecretKey kMac)
		{
			byte[] rndICC = service.SendGetChallenge();
			byte[] rndIFD = new byte[8]; rng.GetBytes(rndIFD);
			byte[] kIFD = new byte[16]; rng.GetBytes(kIFD);
			byte[] response = service.SendMutualAuth(rndIFD, rndICC, kIFD, kEnc, kMac);
			byte[] kICC = new byte[16]; Array.Copy(response, 16, kICC, 0, 16);
			byte[] keySeed = new byte[16];
			for (int i = 0; i < 16; i++) keySeed[i] = (byte)(kIFD[i] ^ kICC[i]);
			var ksEnc = Util.DeriveKey(keySeed, Util.ENC_MODE);
			var ksMac = Util.DeriveKey(keySeed, Util.MAC_MODE);
			long ssc = ComputeSendSequenceCounter(rndICC, rndIFD);
			return new SecureMessagingWrapperShim(new SecureMessagingWrapperShim(ksEnc, ksMac, maxTranceiveLength, shouldCheckMAC, ssc));
		}

		public static long ComputeSendSequenceCounter(byte[] rndICC, byte[] rndIFD)
		{
			if (rndICC == null || rndICC.Length != 8 || rndIFD == null || rndIFD.Length != 8)
				throw new InvalidOperationException("Wrong length input");
			long ssc = 0;
			for (int i = 4; i < 8; i++) { ssc = (ssc << 8) + (rndICC[i] & 0xFF); }
			for (int i = 4; i < 8; i++) { ssc = (ssc << 8) + (rndIFD[i] & 0xFF); }
			return ssc;
		}

		// Temporary wrapper shim until DESede/AES secure messaging is ported
		private sealed class SecureMessagingWrapperShim : SecureMessagingWrapper
		{
			public SecureMessagingWrapperShim(SecretKey ksEnc, SecretKey ksMac, int maxLen, bool checkMac, long ssc) : base(ksEnc, ksMac, maxLen, checkMac, ssc) { }
			public SecureMessagingWrapperShim(SecureMessagingWrapper inner) : base(inner.GetEncryptionKey(), inner.GetMACKey(), inner.GetMaxTranceiveLength(), inner.ShouldCheckMAC(), inner.GetSendSequenceCounter()) { }
			
			public override CommandAPDU Wrap(CommandAPDU command) => command;
			public override ResponseAPDU Unwrap(ResponseAPDU response) => response;
			public override object Type => "Shim";
			
			public override CommandAPDU WrapCommand(CommandAPDU command) => Wrap(command);
			public override ResponseAPDU UnwrapResponse(ResponseAPDU response) => Unwrap(response);
		}
	}
}


