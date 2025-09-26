using System;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImagePropertiesBlock : Block
	{
		public ushort Width { get; }
		public ushort Height { get; }
		public byte ColorSpace { get; }
		public byte BitDepth { get; }

		public FaceImagePropertiesBlock(ushort width, ushort height, byte colorSpace, byte bitDepth)
		{
			Width = width; Height = height; ColorSpace = colorSpace; BitDepth = bitDepth; Length = 6;
		}

	public override byte[] GetEncoded()
	{
		return new byte[]
		{
			(byte)(Width >> 8), (byte)(Width & 0xFF),
			(byte)(Height >> 8), (byte)(Height & 0xFF),
			ColorSpace, BitDepth
		};
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
