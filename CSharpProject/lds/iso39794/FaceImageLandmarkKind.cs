using System;

namespace org.jmrtd.lds.iso39794
{
	public sealed class FaceImageLandmarkKind : EncodableEnum
	{
		private readonly string name;
		private FaceImageLandmarkKind(int code, string name) : base(code) { this.name = name; }
		public override string Name => name;

		public static readonly FaceImageLandmarkKind EYE_LEFT = new FaceImageLandmarkKind(0, "EyeLeft");
		public static readonly FaceImageLandmarkKind EYE_RIGHT = new FaceImageLandmarkKind(1, "EyeRight");
		public static readonly FaceImageLandmarkKind NOSE_TIP = new FaceImageLandmarkKind(2, "NoseTip");
	}
}
