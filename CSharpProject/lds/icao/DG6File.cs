using System;
using System.IO;

namespace org.jmrtd.lds.icao
{
	public class DG6File : org.jmrtd.lds.DisplayedImageDataGroup
	{
		public DG6File(Stream inputStream) : base(102, inputStream)
		{
		}
	}
}
