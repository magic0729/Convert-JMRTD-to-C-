using System;
using System.Collections.Generic;

namespace org.jmrtd.lds.iso39794
{
	public class FingerImageRepresentationBlock : Block
	{
		public VersionBlock Version { get; }
		public RegistryIdBlock RegistryId { get; }
		public QualityBlock? Quality { get; }
		public FingerImageCaptureDeviceBlock? CaptureDevice { get; }
		public FingerImageSegmentationBlock? Segmentation { get; }

		public FingerImageRepresentationBlock(VersionBlock version, RegistryIdBlock registryId, QualityBlock? quality = null,
			FingerImageCaptureDeviceBlock? captureDevice = null, FingerImageSegmentationBlock? segmentation = null)
		{
			Version = version ?? throw new System.ArgumentNullException(nameof(version));
			RegistryId = registryId ?? throw new System.ArgumentNullException(nameof(registryId));
			Quality = quality;
			CaptureDevice = captureDevice;
			Segmentation = segmentation;
			Length = 0;
		}

	public override byte[] GetEncoded()
	{
		// TODO: Implement full ISO 39794 encoding for FingerImageRepresentationBlock
		return System.Array.Empty<byte>();
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
