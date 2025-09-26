using System;
using System.Collections.Generic;
using System.Numerics;

namespace org.jmrtd.lds.iso39794
{
    public class FaceImageCoordinateTextureImageBlock : Block
    {
        private BigInteger uInPixel;
        private BigInteger vInPixel;

        public FaceImageCoordinateTextureImageBlock(BigInteger uInPixel, BigInteger vInPixel)
        {
            this.uInPixel = uInPixel;
            this.vInPixel = vInPixel;
        }

        internal FaceImageCoordinateTextureImageBlock(object asn1Encodable)
        {
            // TODO: Implement ASN1Util when ASN1 support is added
            // var taggedObjects = ASN1Util.DecodeTaggedObjects(asn1Encodable);
            // this.uInPixel = ASN1Util.DecodeBigInteger(taggedObjects[0]);
            // this.vInPixel = ASN1Util.DecodeBigInteger(taggedObjects[1]);
        }

        public BigInteger GetUInPixel() => uInPixel;
        public BigInteger GetVInPixel() => vInPixel;

        public override int GetHashCode()
        {
            return HashCode.Combine(uInPixel, vInPixel);
        }

        public override bool Equals(object? obj)
        {
            if (obj == this) return true;
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;

            var other = (FaceImageCoordinateTextureImageBlock)obj;
            return uInPixel.Equals(other.uInPixel) && vInPixel.Equals(other.vInPixel);
        }

        public override string ToString()
        {
            return $"CoordinateTextureImageBlock [uInPixel: {uInPixel}, vInPixel: {vInPixel}]";
        }

        public override byte[] GetEncoded()
        {
            // TODO: Implement when ASN1 support is added
            return Array.Empty<byte>();
        }

        internal override object GetASN1Object()
        {
            // TODO: Implement ASN1Util when ASN1 support is added
            // var taggedObjects = new Dictionary<int, object>();
            // taggedObjects[0] = ASN1Util.EncodeBigInteger(uInPixel);
            // taggedObjects[1] = ASN1Util.EncodeBigInteger(vInPixel);
            // return ASN1Util.EncodeTaggedObjects(taggedObjects);
            return new object();
        }
    }
}
