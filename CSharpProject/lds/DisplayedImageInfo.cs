using System;
using System.IO;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds
{
	public class DisplayedImageInfo : AbstractImageInfo
	{
		public const int DISPLAYED_PORTRAIT_TAG = 0x5F40; // 24384
		public const int DISPLAYED_SIGNATURE_OR_MARK_TAG = 0x5F43; // 24387
		private int displayedImageTag;

		public DisplayedImageInfo(int type, byte[] imageBytes)
			: base(type, GetMimeTypeFromType(type))
		{
			displayedImageTag = GetDisplayedImageTagFromType(type);
			SetImageBytes(imageBytes);
		}

		public DisplayedImageInfo(Stream inputStream)
		{
			ReadObjectCore(inputStream);
		}

		protected override void ReadObjectCore(Stream inputStream)
		{
			var tlvIn = inputStream as TLVInputStream ?? new TLVInputStream(inputStream);
			displayedImageTag = tlvIn.ReadTag();
			if (displayedImageTag != DISPLAYED_PORTRAIT_TAG && displayedImageTag != DISPLAYED_SIGNATURE_OR_MARK_TAG)
				throw new System.ArgumentException("Unexpected displayed image tag");
			int type = GetTypeFromDisplayedImageTag(displayedImageTag);
			SetType(type);
			SetMimeType(GetMimeTypeFromType(type));
			int imageLength = tlvIn.ReadLength();
			ReadImage(tlvIn, imageLength);
		}

		protected override void WriteObjectCore(Stream outputStream)
		{
			var tlvOut = outputStream as TLVOutputStream ?? new TLVOutputStream(outputStream);
			tlvOut.WriteTag(GetDisplayedImageTagFromType(GetTypeCode()));
			WriteImage(tlvOut);
			tlvOut.WriteValueEnd();
		}

		public int GetDisplayedImageTag() => displayedImageTag;

		public long GetRecordLength()
		{
			int imageLength = GetImageLength();
			// Approximate length encoding
			return 1 + 4 + imageLength;
		}

		private static string GetMimeTypeFromType(int type)
		{
			switch (type)
			{
				case 0: return "image/jpeg";
				case 2: return "image/x-wsq";
				case 1: return "image/jpeg";
			}
			throw new System.ArgumentException("Unknown type");
		}

		private static int GetDisplayedImageTagFromType(int type)
		{
			switch (type)
			{
				case 0: return DISPLAYED_PORTRAIT_TAG;
				case 1: return DISPLAYED_SIGNATURE_OR_MARK_TAG;
			}
			throw new System.ArgumentException("Unknown type");
		}

		private static int GetTypeFromDisplayedImageTag(int tag)
		{
			switch (tag)
			{
				case 0x5F40: return 0;
				case 0x5F43: return 1;
			}
			throw new System.ArgumentException("Unknown tag");
		}
	}
}
