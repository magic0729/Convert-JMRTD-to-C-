using System;
using System.Buffers.Binary;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageReferenceColourMappingBlock : Block
	{
		public float RScale { get; }
		public float GScale { get; }
		public float BScale { get; }

		public FaceImageReferenceColourMappingBlock(float r, float g, float b)
		{
			RScale = r; GScale = g; BScale = b; Length = 12;
		}

	public override byte[] GetEncoded()
	{
		byte[] buf = new byte[12];
		BinaryPrimitives.WriteSingleBigEndian(buf.AsSpan(0, 4), RScale);
		BinaryPrimitives.WriteSingleBigEndian(buf.AsSpan(4, 4), GScale);
		BinaryPrimitives.WriteSingleBigEndian(buf.AsSpan(8, 4), BScale);
		return buf;
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
