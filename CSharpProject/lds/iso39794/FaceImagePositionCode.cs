using System;

namespace org.jmrtd.lds.iso39794
{
	public sealed class FaceImagePositionCode : EncodableEnum
	{
		private readonly string name;
		private FaceImagePositionCode(int code, string name) : base(code) { this.name = name; }
		public override string Name => name;

		public static readonly FaceImagePositionCode FRONTAL = new FaceImagePositionCode(0, "Frontal");
		public static readonly FaceImagePositionCode PROFILE_LEFT = new FaceImagePositionCode(1, "ProfileLeft");
		public static readonly FaceImagePositionCode PROFILE_RIGHT = new FaceImagePositionCode(2, "ProfileRight");
	}
}
