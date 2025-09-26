using System;
using System.IO;
using org.jmrtd.cbeff;

namespace org.jmrtd.lds.iso39794
{
	public class FingerImageDataBlock : BiometricDataBlock
	{
		private readonly StandardBiometricHeader sbh;
		private readonly byte[] encoded;

		public FingerImageDataBlock(StandardBiometricHeader sbh, Stream input)
		{
			this.sbh = sbh ?? throw new System.ArgumentNullException(nameof(sbh));
			using var ms = new MemoryStream();
			input.CopyTo(ms);
			encoded = ms.ToArray();
		}

		public StandardBiometricHeader GetStandardBiometricHeader() => sbh;
		public byte[] GetEncoded() => encoded;
	}
}
