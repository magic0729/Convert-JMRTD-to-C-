using System;

namespace org.jmrtd
{
	public interface AccessKeySpec : System.Runtime.Serialization.ISerializable
	{
		string Algorithm { get; }
		byte[] GetKey();
	}
}


