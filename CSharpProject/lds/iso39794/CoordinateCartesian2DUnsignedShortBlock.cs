using System;

namespace org.jmrtd.lds.iso39794
{
	public class CoordinateCartesian2DUnsignedShortBlock : Block
	{
		public ushort X { get; }
		public ushort Y { get; }

		public CoordinateCartesian2DUnsignedShortBlock(ushort x, ushort y)
		{
			X = x; Y = y; Length = 4;
		}

	public override byte[] GetEncoded()
	{
		return new byte[]
		{
			(byte)(X >> 8), (byte)(X & 0xFF),
			(byte)(Y >> 8), (byte)(Y & 0xFF)
		};
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
