using System;
using System.Collections.Generic;

namespace org.jmrtd.lds.iso39794
{
	public class FingerImageSegmentBlock : Block
	{
		public ushort X { get; }
		public ushort Y { get; }
		public ushort Width { get; }
		public ushort Height { get; }

		public FingerImageSegmentBlock(ushort x, ushort y, ushort width, ushort height)
		{
			X = x; Y = y; Width = width; Height = height; Length = 8;
		}

	public override byte[] GetEncoded()
	{
		return new byte[]
		{
			(byte)(X >> 8), (byte)(X & 0xFF),
			(byte)(Y >> 8), (byte)(Y & 0xFF),
			(byte)(Width >> 8), (byte)(Width & 0xFF),
			(byte)(Height >> 8), (byte)(Height & 0xFF)
		};
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}

	public class FingerImageSegmentationBlock : Block
	{
		public List<FingerImageSegmentBlock> Segments { get; }

		public FingerImageSegmentationBlock(IEnumerable<FingerImageSegmentBlock> segments)
		{
			Segments = new List<FingerImageSegmentBlock>(segments);
			Length = 2 + Segments.Count * 8;
		}

	public override byte[] GetEncoded()
	{
		var result = new List<byte>();
		result.Add((byte)(Segments.Count >> 8));
		result.Add((byte)(Segments.Count & 0xFF));
		foreach (var s in Segments) result.AddRange(s.GetEncoded());
		return result.ToArray();
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
