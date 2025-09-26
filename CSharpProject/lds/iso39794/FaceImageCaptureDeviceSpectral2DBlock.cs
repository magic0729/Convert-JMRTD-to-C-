using System;
using System.Buffers.Binary;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageCaptureDeviceSpectral2DBlock : Block
	{
		public float WavelengthStartNm { get; }
		public float WavelengthEndNm { get; }
		public float BandwidthNm { get; }

		public FaceImageCaptureDeviceSpectral2DBlock(float start, float end, float bandwidth)
		{
			WavelengthStartNm = start; WavelengthEndNm = end; BandwidthNm = bandwidth; Length = 12;
		}

	public override byte[] GetEncoded()
	{
		byte[] buf = new byte[12];
		BinaryPrimitives.WriteSingleBigEndian(buf.AsSpan(0, 4), WavelengthStartNm);
		BinaryPrimitives.WriteSingleBigEndian(buf.AsSpan(4, 4), WavelengthEndNm);
		BinaryPrimitives.WriteSingleBigEndian(buf.AsSpan(8, 4), BandwidthNm);
		return buf;
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
