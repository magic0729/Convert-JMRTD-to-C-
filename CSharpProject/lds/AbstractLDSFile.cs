using System;
using System.IO;

namespace org.jmrtd.lds
{
	public abstract class AbstractLDSFile : AbstractLDSInfo
	{
		private readonly short tag;

		protected AbstractLDSFile(short tag)
		{
			this.tag = tag;
		}

		protected AbstractLDSFile(short tag, Stream inputStream)
		{
			this.tag = tag;
			if (inputStream == null) throw new System.ArgumentNullException(nameof(inputStream));
			ReadObject(inputStream);
		}

		public short GetTag() => tag;

		public override void WriteObject(Stream outputStream)
		{
			WriteContent(outputStream);
		}

		protected abstract void ReadContent(Stream inputStream);
		protected abstract void WriteContent(Stream outputStream);

		private void ReadObject(Stream inputStream)
		{
			ReadContent(inputStream);
		}
	}
}
