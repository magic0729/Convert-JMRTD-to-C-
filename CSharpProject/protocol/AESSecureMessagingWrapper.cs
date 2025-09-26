using org.jmrtd.CustomJavaAPI;
using System.Security.Cryptography;
using System;

namespace org.jmrtd.protocol
{
	public sealed class AESSecureMessagingWrapper : SecureMessagingWrapper
	{
		public AESSecureMessagingWrapper(SecretKey ksEnc, SecretKey ksMac, int maxTranceiveLength, bool shouldCheckMAC, long ssc)
			: base(ksEnc, ksMac, maxTranceiveLength, shouldCheckMAC, ssc) { }

		public AESSecureMessagingWrapper(AESSecureMessagingWrapper other)
			: base(other.GetEncryptionKey(), other.GetMACKey(), other.GetMaxTranceiveLength(), other.ShouldCheckMAC(), other.GetSendSequenceCounter()) { }

		public override CommandAPDU Wrap(CommandAPDU command)
		{
			// Build protected header and DOs per ISO 7816-4 SM (simplified, no chaining)
			byte[] ssc = GetSscBlockAndIncrement(16);
			byte[] header = BuildProtectedHeader(command.CLA, command.INS, command.P1, command.P2);
			byte[] do87 = Array.Empty<byte>();
			byte[] data = command.Data ?? Array.Empty<byte>();
			if (data.Length > 0)
			{
				byte[] encData = EncryptCBC(GetEncryptionKey().GetEncoded(), data);
				do87 = TLV.WrapDO(0x87, Concat(new byte[] { 0x01 }, encData));
			}
			byte[] do97 = command.Ne > 0 ? TLV.WrapDO(0x97, new byte[] { (byte)(command.Ne == 65536 ? 0x00 : Math.Min(command.Ne, 256)) }) : Array.Empty<byte>();
			byte[] mInput = Concat(Concat(ssc, header), Concat(do87, do97));
			byte[] do8e = TLV.WrapDO(0x8E, AESCMAC.Compute(GetMACKey().GetEncoded(), mInput, 16));
			byte[] smData = Concat(Concat(header, do87), Concat(do97, do8e));
			return new CommandAPDU(smData);
		}

		public override ResponseAPDU Unwrap(ResponseAPDU response)
		{
			byte[] rapdu = response.Bytes;
			if (rapdu.Length < 2) return response;
			int sw1 = rapdu[^2]; int sw2 = rapdu[^1];
			byte[] tlvs = new byte[Math.Max(0, rapdu.Length - 2)]; Array.Copy(rapdu, 0, tlvs, 0, tlvs.Length);
			// Verify MAC in DO8E over (SSC||DO87||DO99)
			byte[] ssc = GetSscBlockAndIncrement(16);
			byte[] do99 = TLV.WrapDO(0x99, new byte[] { (byte)sw1, (byte)sw2 });
			// Parse incoming TLVs: optional DO87 and required DO8E
			byte[] do87 = Array.Empty<byte>();
			byte[] do8e = Array.Empty<byte>();
			try { do87 = TLV.UnwrapDO(0x87, tlvs); } catch { }
			try { do8e = TLV.UnwrapDO(0x8E, tlvs); } catch { }
			byte[] mInput = Concat(ssc, Concat(do87, do99));
			byte[] mac = do8e;
			byte[] expect = AESCMAC.Compute(GetMACKey().GetEncoded(), mInput, 16);
			if (mac.Length == expect.Length)
			{
				int diff = 0; for (int i = 0; i < mac.Length; i++) diff |= mac[i] ^ expect[i];
				if (diff != 0 && ShouldCheckMAC()) return new ResponseAPDU(new byte[] { 0x69, 0x88 });
			}
			byte[] data = Array.Empty<byte>();
			if (do87.Length > 0)
			{
				// DO87 value is 0x01 || encData
				byte[] enc = new byte[do87.Length - 1]; Array.Copy(do87, 1, enc, 0, enc.Length);
				data = DecryptCBC(GetEncryptionKey().GetEncoded(), enc);
			}
			return new ResponseAPDU(Concat(data, new byte[] { (byte)sw1, (byte)sw2 }));
		}
		public override object Type => "AES";

		public override CommandAPDU WrapCommand(CommandAPDU command) => Wrap(command);
		public override ResponseAPDU UnwrapResponse(ResponseAPDU response) => Unwrap(response);
	}
}


