using System;

namespace org.jmrtd.lds.iso39794
{
	public abstract class Block
	{
		public int Length { get; protected set; }
		public abstract byte[] GetEncoded();
		internal abstract object GetASN1Object();
	}
}
