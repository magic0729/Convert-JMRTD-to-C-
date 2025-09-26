using System;
using System.Collections.Generic;

namespace org.jmrtd.lds.iso39794
{
    public class FaceImageRepresentation2DBlock : Block
    {
        private int imageWidth;
        private int imageHeight;
        private string? colorSpace;
        private string? sourceType;
        private string? imageType;

        public FaceImageRepresentation2DBlock(int imageWidth, int imageHeight, string? colorSpace, string? sourceType, string? imageType)
        {
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.colorSpace = colorSpace;
            this.sourceType = sourceType;
            this.imageType = imageType;
        }

        internal FaceImageRepresentation2DBlock(object asn1Encodable)
        {
            // TODO: Implement ASN1Util when ASN1 support is added
        }

        public int GetImageWidth() => imageWidth;
        public int GetImageHeight() => imageHeight;
        public string? GetColorSpace() => colorSpace;
        public string? GetSourceType() => sourceType;
        public string? GetImageType() => imageType;

        public override int GetHashCode()
        {
            return HashCode.Combine(imageWidth, imageHeight, colorSpace, sourceType, imageType);
        }

        public override bool Equals(object? obj)
        {
            if (obj == this) return true;
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;

            var other = (FaceImageRepresentation2DBlock)obj;
            return imageWidth == other.imageWidth && imageHeight == other.imageHeight && 
                   Equals(colorSpace, other.colorSpace) && Equals(sourceType, other.sourceType) && 
                   Equals(imageType, other.imageType);
        }

        public override string ToString()
        {
            return $"FaceImageRepresentation2DBlock [imageWidth: {imageWidth}, imageHeight: {imageHeight}, colorSpace: {colorSpace}, sourceType: {sourceType}, imageType: {imageType}]";
        }

        public override byte[] GetEncoded()
        {
            // TODO: Implement when ASN1 support is added
            return Array.Empty<byte>();
        }

        internal override object GetASN1Object()
        {
            // TODO: Implement ASN1Util when ASN1 support is added
            return new object();
        }
    }
}
