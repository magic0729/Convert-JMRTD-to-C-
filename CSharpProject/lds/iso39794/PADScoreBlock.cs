using System;

namespace org.jmrtd.lds.iso39794
{
	public class PADScoreBlock : Block
	{
		public ushort AlgorithmId { get; }
		public ushort Score { get; }

		public PADScoreBlock(ushort algorithmId, ushort score)
		{
			AlgorithmId = algorithmId; Score = score; Length = 4;
		}

	public override byte[] GetEncoded()
	{
		return new byte[]
		{
			(byte)(AlgorithmId >> 8), (byte)(AlgorithmId & 0xFF),
			(byte)(Score >> 8), (byte)(Score & 0xFF)
		};
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
