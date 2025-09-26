using System;
using System.IO;

namespace org.jmrtd.lds
{
	public abstract class AbstractTaggedLDSFile : DataGroup
	{
		protected AbstractTaggedLDSFile(short tag) : base(tag)
		{
		}

		protected AbstractTaggedLDSFile(short tag, Stream inputStream) : base(tag)
		{
			if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
			ReadContent(inputStream);
		}

		// Inherit GetTag() from AbstractLDSFile to avoid hiding warnings
	}
}
