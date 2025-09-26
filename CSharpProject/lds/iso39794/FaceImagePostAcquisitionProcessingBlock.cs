using System;
using System.Collections.Generic;

namespace org.jmrtd.lds.iso39794
{
    public class FaceImagePostAcquisitionProcessingBlock : Block
    {
        private string? processingAlgorithm;
        private string? processingAlgorithmVendor;
        private string? processingAlgorithmVersion;

        public FaceImagePostAcquisitionProcessingBlock(string? processingAlgorithm, string? processingAlgorithmVendor, string? processingAlgorithmVersion)
        {
            this.processingAlgorithm = processingAlgorithm;
            this.processingAlgorithmVendor = processingAlgorithmVendor;
            this.processingAlgorithmVersion = processingAlgorithmVersion;
        }

        internal FaceImagePostAcquisitionProcessingBlock(object asn1Encodable)
        {
            // TODO: Implement ASN1Util when ASN1 support is added
        }

        public string? GetProcessingAlgorithm() => processingAlgorithm;
        public string? GetProcessingAlgorithmVendor() => processingAlgorithmVendor;
        public string? GetProcessingAlgorithmVersion() => processingAlgorithmVersion;

        public override int GetHashCode()
        {
            return HashCode.Combine(processingAlgorithm, processingAlgorithmVendor, processingAlgorithmVersion);
        }

        public override bool Equals(object? obj)
        {
            if (obj == this) return true;
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;

            var other = (FaceImagePostAcquisitionProcessingBlock)obj;
            return Equals(processingAlgorithm, other.processingAlgorithm) && 
                   Equals(processingAlgorithmVendor, other.processingAlgorithmVendor) && 
                   Equals(processingAlgorithmVersion, other.processingAlgorithmVersion);
        }

        public override string ToString()
        {
            return $"FaceImagePostAcquisitionProcessingBlock [processingAlgorithm: {processingAlgorithm}, processingAlgorithmVendor: {processingAlgorithmVendor}, processingAlgorithmVersion: {processingAlgorithmVersion}]";
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
