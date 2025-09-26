using System;

namespace org.jmrtd.lds.iso39794
{
	public class PADDataBlock : Block
	{
		private readonly byte[] data;
		public PADDataBlock(byte[] data)
		{
			this.data = data ?? Array.Empty<byte>();
			Length = this.data.Length;
		}
		public override byte[] GetEncoded() => (byte[])data.Clone();

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
