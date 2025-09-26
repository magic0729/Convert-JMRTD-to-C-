using System;
using System.IO;

namespace org.jmrtd.lds
{
	public abstract class LDSFile : AbstractLDSFile
	{
		protected LDSFile(short tag) : base(tag) { }
		protected LDSFile(short tag, Stream inputStream) : base(tag, inputStream) { }
	}
}
