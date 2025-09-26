using System;
using System.Collections.Generic;
using System.IO;

namespace org.jmrtd.lds.icao
{
	public class DG7File : org.jmrtd.lds.DisplayedImageDataGroup
	{
		public DG7File(List<org.jmrtd.lds.DisplayedImageInfo> images)
			: base(103, images, 0x5F43) // 24387
		{
		}

		public DG7File(Stream inputStream) : base(103, inputStream)
		{
		}
	}
}
