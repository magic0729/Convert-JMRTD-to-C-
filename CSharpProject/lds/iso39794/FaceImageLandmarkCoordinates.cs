using System;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageLandmarkCoordinates
	{
		public ushort X { get; }
		public ushort Y { get; }
		public FaceImageLandmarkKind Kind { get; }

		public FaceImageLandmarkCoordinates(ushort x, ushort y, FaceImageLandmarkKind kind)
		{
			X = x; Y = y; Kind = kind;
		}
	}
}
