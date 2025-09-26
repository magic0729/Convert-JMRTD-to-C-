using System;
using System.Collections.Generic;
using System.IO;

namespace org.jmrtd.lds.icao
{
	public class DG5File : org.jmrtd.lds.DisplayedImageDataGroup
	{
		public DG5File(List<org.jmrtd.lds.DisplayedImageInfo> images)
			: base(101, images, 0x5F20) // 24384
		{
		}

		public DG5File(Stream inputStream) : base(101, inputStream)
		{
		}
	}
}
