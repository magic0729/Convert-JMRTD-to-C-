using System;
using System.IO;
using org.jmrtd.cbeff;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds.iso19794
{
	public class FingerInfo : BiometricDataBlock
	{
		private readonly StandardBiometricHeader sbh;
		private readonly byte[] data;

		public FingerInfo(StandardBiometricHeader sbh, Stream input)
		{
			this.sbh = sbh ?? throw new System.ArgumentNullException(nameof(sbh));
			// Read all remaining bytes from input
			using var ms = new MemoryStream();
			input.CopyTo(ms);
			data = ms.ToArray();
		}

		public StandardBiometricHeader GetStandardBiometricHeader() => sbh;

		public void WriteObject(Stream output)
		{
			if (output == null) throw new System.ArgumentNullException(nameof(output));
			output.Write(data, 0, data.Length);
		}
	}
}
