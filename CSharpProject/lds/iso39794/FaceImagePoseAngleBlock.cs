using System;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImagePoseAngleBlock : Block
	{
		public short Pitch { get; }
		public short Yaw { get; }
		public short Roll { get; }

		public FaceImagePoseAngleBlock(short pitch, short yaw, short roll)
		{
			Pitch = pitch; Yaw = yaw; Roll = roll; Length = 6;
		}

	public override byte[] GetEncoded()
	{
		return new byte[]
		{
			(byte)(Pitch >> 8), (byte)(Pitch & 0xFF),
			(byte)(Yaw >> 8), (byte)(Yaw & 0xFF),
			(byte)(Roll >> 8), (byte)(Roll & 0xFF)
		};
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
