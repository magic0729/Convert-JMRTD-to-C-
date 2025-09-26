using System;
using System.IO;
using org.jmrtd.cbeff;

namespace org.jmrtd.lds.iso19794
{
	public class FingerImageInfo : BiometricDataBlock
	{
		private readonly StandardBiometricHeader sbh;
		private readonly byte[] data;

		public FingerImageInfo(StandardBiometricHeader sbh, Stream input)
		{
			this.sbh = sbh ?? throw new System.ArgumentNullException(nameof(sbh));
			using var ms = new MemoryStream();
			input.CopyTo(ms);
			data = ms.ToArray();
		}

		public StandardBiometricHeader GetStandardBiometricHeader() => sbh;
		public void WriteObject(Stream output) => output.Write(data, 0, data.Length);
	}
}
