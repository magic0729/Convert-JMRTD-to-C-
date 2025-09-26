using System;
using System.Text;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageAnnotationBlock : Block
	{
		public string Text { get; }

		public FaceImageAnnotationBlock(string text)
		{
			Text = text ?? string.Empty;
			Length = Encoding.UTF8.GetByteCount(Text);
		}

		public override byte[] GetEncoded() => Encoding.UTF8.GetBytes(Text);

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
