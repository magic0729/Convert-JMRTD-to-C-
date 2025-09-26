using System;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageCaptureDevice2DBlock : Block
	{
		public ushort SensorWidth { get; }
		public ushort SensorHeight { get; }
		public ushort DPI { get; }

		public FaceImageCaptureDevice2DBlock(ushort sensorWidth, ushort sensorHeight, ushort dpi)
		{
			SensorWidth = sensorWidth; SensorHeight = sensorHeight; DPI = dpi; Length = 6;
		}

	public override byte[] GetEncoded()
	{
		return new byte[]
		{
			(byte)(SensorWidth >> 8), (byte)(SensorWidth & 0xFF),
			(byte)(SensorHeight >> 8), (byte)(SensorHeight & 0xFF),
			(byte)(DPI >> 8), (byte)(DPI & 0xFF)
		};
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
