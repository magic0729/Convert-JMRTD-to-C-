using System;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageExpressionBlock : Block
	{
		public byte ExpressionCode { get; }
		public byte Intensity { get; }

		public FaceImageExpressionBlock(byte expressionCode, byte intensity)
		{
			ExpressionCode = expressionCode; Intensity = intensity; Length = 2;
		}

		public override byte[] GetEncoded() => new byte[] { ExpressionCode, Intensity };

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
