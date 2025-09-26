using System;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageCaptureDeviceBlock : Block
	{
		public ushort Vendor { get; }
		public ushort Type { get; }

		public FaceImageCaptureDeviceBlock(ushort vendor, ushort type)
		{
			Vendor = vendor; Type = type; Length = 4;
		}

	public override byte[] GetEncoded()
	{
		return new byte[]
		{
			(byte)(Vendor >> 8), (byte)(Vendor & 0xFF),
			(byte)(Type >> 8), (byte)(Type & 0xFF)
		};
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
