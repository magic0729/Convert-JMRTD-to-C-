using System;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageInformation2DBlock : Block
	{
		public FaceImagePropertiesBlock Properties { get; }
		public FaceImagePositionCode Position { get; }

		public FaceImageInformation2DBlock(FaceImagePropertiesBlock properties, FaceImagePositionCode position)
		{
			Properties = properties ?? throw new System.ArgumentNullException(nameof(properties));
			Position = position ?? throw new System.ArgumentNullException(nameof(position));
			Length = Properties.Length + 1; // plus 1 byte for position code (example)
		}

	public override byte[] GetEncoded()
	{
		var props = Properties.GetEncoded();
		var result = new byte[props.Length + 1];
		Buffer.BlockCopy(props, 0, result, 0, props.Length);
			result[^1] = (byte)Position.Code;
		return result;
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
