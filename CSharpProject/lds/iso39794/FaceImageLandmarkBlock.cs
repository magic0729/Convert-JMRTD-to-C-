using System;
using System.Collections.Generic;
using System.Linq;

namespace org.jmrtd.lds.iso39794
{
    public class FaceImageLandmarkBlock : Block
    {
		private FaceImageLandmarkKind landmarkKind = null!;
        private FaceImageLandmarkCoordinates? landmarkCoordinates;

        public FaceImageLandmarkBlock(FaceImageLandmarkKind landmarkKind, FaceImageLandmarkCoordinates? landmarkCoordinates)
        {
            this.landmarkKind = landmarkKind;
            this.landmarkCoordinates = landmarkCoordinates;
        }

        internal FaceImageLandmarkBlock(object asn1Encodable)
        {
            // TODO: Implement ASN1Util when ASN1 support is added
            // var taggedObjects = ASN1Util.DecodeTaggedObjects(asn1Encodable);
            // this.landmarkKind = FaceImageLandmarkKind.DecodeLandmarkKind(taggedObjects[0]);
            // if (taggedObjects.ContainsKey(1))
            // {
            //     this.landmarkCoordinates = FaceImageLandmarkCoordinates.DecodeLandmarkCoordinates(taggedObjects[1]);
            // }
        }

        public FaceImageLandmarkKind GetLandmarkKind() => landmarkKind;
        public FaceImageLandmarkCoordinates? GetLandmarkCoordinates() => landmarkCoordinates;

        public override int GetHashCode()
        {
            return HashCode.Combine(landmarkCoordinates, landmarkKind);
        }

        public override bool Equals(object? obj)
        {
            if (obj == this) return true;
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;

            var other = (FaceImageLandmarkBlock)obj;
            return Equals(landmarkCoordinates, other.landmarkCoordinates) && Equals(landmarkKind, other.landmarkKind);
        }

        public override string ToString()
        {
            return $"FaceImageLandmarkBlock [landmarkKind: {landmarkKind}, landmarkCoordinates: {landmarkCoordinates}]";
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
            // taggedObjects[0] = FaceImageLandmarkKind.EncodeLandmarkKind(landmarkKind);
            // if (landmarkCoordinates != null)
            // {
            //     taggedObjects[1] = FaceImageLandmarkCoordinates.EncodeLandmarkCoordinates(landmarkCoordinates);
            // }
            // return ASN1Util.EncodeTaggedObjects(taggedObjects);
            return new object();
        }

        internal static List<FaceImageLandmarkBlock> DecodeLandmarkBlocks(object asn1Encodable)
        {
            // TODO: Implement ASN1Util when ASN1 support is added
            // if (ASN1Util.IsSequenceOfSequences(asn1Encodable))
            // {
            //     var blockASN1Objects = ASN1Util.List(asn1Encodable);
            //     var blocks = new List<FaceImageLandmarkBlock>(blockASN1Objects.Count);
            //     foreach (var blockASN1Object in blockASN1Objects)
            //     {
            //         blocks.Add(new FaceImageLandmarkBlock(blockASN1Object));
            //     }
            //     return blocks;
            // }
            // return new List<FaceImageLandmarkBlock> { new FaceImageLandmarkBlock(asn1Encodable) };
            return new List<FaceImageLandmarkBlock>();
        }
    }
}
