using System;

namespace org.jmrtd.lds.iso39794
{
	public class CoordinateCartesian3DUnsignedShortBlock : Block
	{
		public ushort X { get; }
		public ushort Y { get; }
		public ushort Z { get; }

		public CoordinateCartesian3DUnsignedShortBlock(ushort x, ushort y, ushort z)
		{
			X = x; Y = y; Z = z; Length = 6;
		}

	public override byte[] GetEncoded()
	{
		return new byte[]
		{
			(byte)(X >> 8), (byte)(X & 0xFF),
			(byte)(Y >> 8), (byte)(Y & 0xFF),
			(byte)(Z >> 8), (byte)(Z & 0xFF)
		};
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
