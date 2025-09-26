using System;
using System.Collections.Generic;
using System.IO;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds
{
	public abstract class DisplayedImageDataGroup : DataGroup
	{
		private const int DISPLAYED_IMAGE_COUNT_TAG = 0x02;
		private int displayedImageTagToUse;
		private List<DisplayedImageInfo> imageInfos;

		protected DisplayedImageDataGroup(short dataGroupTag, List<DisplayedImageInfo> imageInfos, int displayedImageTagToUse)
			: base(dataGroupTag)
		{
			if (imageInfos == null) throw new System.ArgumentNullException(nameof(imageInfos));
			this.displayedImageTagToUse = displayedImageTagToUse;
			this.imageInfos = new List<DisplayedImageInfo>(imageInfos);
			CheckTypesConsistentWithTag();
		}

		protected DisplayedImageDataGroup(short dataGroupTag, Stream inputStream)
			: base(dataGroupTag, inputStream)
		{
			if (imageInfos == null) imageInfos = new List<DisplayedImageInfo>();
			CheckTypesConsistentWithTag();
		}

		protected override void ReadContent(Stream inputStream)
		{
			var tlvIn = inputStream as TLVInputStream ?? new TLVInputStream(inputStream);
			int countTag = tlvIn.ReadTag();
			if (countTag != DISPLAYED_IMAGE_COUNT_TAG) throw new System.ArgumentException($"Expected tag 0x{DISPLAYED_IMAGE_COUNT_TAG:X}");
			int countLength = tlvIn.ReadLength();
			if (countLength != 1) throw new System.ArgumentException("DISPLAYED_IMAGE_COUNT should have length 1");
			int count = tlvIn.ReadValue()[0] & 0xFF;
			for (int i = 0; i < count; i++)
			{
				var imageInfo = new DisplayedImageInfo(tlvIn);
				if (i == 0) displayedImageTagToUse = imageInfo.GetDisplayedImageTag();
				else if (imageInfo.GetDisplayedImageTag() != displayedImageTagToUse) throw new IOException("Mixed displayed image tags in datagroup");
				Add(imageInfo);
			}
		}

		protected override void WriteContent(Stream outputStream)
		{
			var tlvOut = outputStream as TLVOutputStream ?? new TLVOutputStream(outputStream);
			tlvOut.WriteTag(DISPLAYED_IMAGE_COUNT_TAG);
			tlvOut.WriteValue(new byte[] { (byte)imageInfos.Count });
			foreach (var imageInfo in imageInfos)
			{
				imageInfo.WriteObject(tlvOut);
			}
		}

		public List<DisplayedImageInfo> GetImages() => new List<DisplayedImageInfo>(imageInfos);

		private void Add(DisplayedImageInfo image)
		{
			if (imageInfos == null) imageInfos = new List<DisplayedImageInfo>();
			imageInfos.Add(image);
		}

		private void CheckTypesConsistentWithTag()
		{
			foreach (var imageInfo in imageInfos)
			{
				if (imageInfo == null) throw new System.ArgumentException("Found a null image info");
				switch (imageInfo.GetTypeCode())
				{
					case 1:
						if (displayedImageTagToUse != 0x5F43) throw new System.ArgumentException("'Portrait' cannot be part of 'Signature' DG");
						break;
					case 0:
						if (displayedImageTagToUse != 0x5F40) throw new System.ArgumentException("'Signature' cannot be part of 'Portrait' DG");
						break;
				}
			}
		}
	}
}
