using System;

namespace org.jmrtd.lds.iso39794
{
	public abstract class EncodableEnum
	{
		public int Code { get; }
		protected EncodableEnum(int code) { Code = code; }
		public abstract string Name { get; }
		public override string ToString() => $"{Name}({Code})";
	}
}
