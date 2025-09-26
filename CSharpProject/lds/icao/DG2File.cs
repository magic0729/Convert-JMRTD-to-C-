using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.jmrtd.cbeff;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds.icao
{
    public class DG2File : CBEFFDataGroup
    {
        private static readonly ISO781611Decoder<BiometricDataBlock> DECODER = new ISO781611Decoder<BiometricDataBlock>(GetDecoderMap());
        private static readonly ISO781611Encoder<BiometricDataBlock> ISO_19794_ENCODER = new ISO781611Encoder<BiometricDataBlock>(new Face19794Encoder());
        private static readonly ISO781611Encoder<BiometricDataBlock> ISO_39794_ENCODER = new ISO781611Encoder<BiometricDataBlock>(new Face39794Encoder());

        [Obsolete("This method is deprecated.")]
        public DG2File(List<BiometricDataBlock> faceInfos) : this(BiometricEncodingType.ISO_19794, faceInfos)
        {
        }

        public DG2File(Stream inputStream) : base(117, inputStream, false)
        {
        }

        private DG2File(BiometricEncodingType encodingType, ICollection<BiometricDataBlock> biometricDataBlocks) : base(117, encodingType, biometricDataBlocks, false)
        {
        }

        public static DG2File CreateISO19794DG2File(List<BiometricDataBlock> faceInfos)
        {
            return new DG2File(BiometricEncodingType.ISO_19794, faceInfos);
        }

        public static DG2File CreateISO39794DG2File(List<BiometricDataBlock> faceImageDataBlocks)
        {
            return new DG2File(BiometricEncodingType.ISO_39794, faceImageDataBlocks);
        }

        [Obsolete("This method is deprecated.")]
        public List<BiometricDataBlock> GetFaceInfos()
        {
            return ToFaceInfos(GetSubRecords());
        }

        private static List<BiometricDataBlock> ToFaceInfos(List<BiometricDataBlock> records)
        {
            if (records == null) return new List<BiometricDataBlock>();
            return records.Where(record => record != null).ToList();
        }

        public override string ToString()
        {
            return $"DG2File [{base.ToString()}]";
        }

        private static System.Collections.Generic.Dictionary<int, BiometricDataBlockDecoder<BiometricDataBlock>> GetDecoderMap()
        {
            var decoders = new System.Collections.Generic.Dictionary<int, BiometricDataBlockDecoder<BiometricDataBlock>>();
            decoders[24366] = new Face19794Decoder();
            decoders[32558] = new Face39794Decoder();
            return decoders;
        }

        private sealed class Face19794Encoder : BiometricDataBlockEncoder<BiometricDataBlock>
        {
            public void Encode(BiometricDataBlock info, System.IO.Stream outputStream)
            {
                if (info is org.jmrtd.lds.iso19794.FaceInfo fi)
                {
                    fi.WriteObject(outputStream);
                }
            }
        }

        private sealed class Face39794Encoder : BiometricDataBlockEncoder<BiometricDataBlock>
        {
            public void Encode(BiometricDataBlock info, System.IO.Stream outputStream)
            {
                if (info is org.jmrtd.lds.iso39794.FaceImageDataBlock fib)
                {
                    var tlvOut = outputStream as TLVOutputStream ?? new TLVOutputStream(outputStream);
                    tlvOut.WriteTag(0xA1);
                    tlvOut.WriteValue(fib.GetEncoded());
                }
            }
        }

        private sealed class Face19794Decoder : BiometricDataBlockDecoder<BiometricDataBlock>
        {
            public BiometricDataBlock Decode(System.IO.Stream inputStream, StandardBiometricHeader sbh, int index, int length)
            {
                return new org.jmrtd.lds.iso19794.FaceInfo(sbh, inputStream);
            }
        }

        private sealed class Face39794Decoder : BiometricDataBlockDecoder<BiometricDataBlock>
        {
            public BiometricDataBlock Decode(System.IO.Stream inputStream, StandardBiometricHeader sbh, int index, int length)
            {
                var tlvIn = inputStream as TLVInputStream ?? new TLVInputStream(inputStream);
                int tag = tlvIn.ReadTag(); // expect A1
                int _ = tlvIn.ReadLength();
                return new org.jmrtd.lds.iso39794.FaceImageDataBlock(sbh, inputStream);
            }
        }

        public override ISO781611Decoder<BiometricDataBlock> GetDecoder() => DECODER;

        public override ISO781611Encoder<BiometricDataBlock> GetEncoder()
        {
            if (this.encodingType == null) return ISO_19794_ENCODER;
            switch (this.encodingType)
            {
                case BiometricEncodingType.ISO_19794: return ISO_19794_ENCODER;
                case BiometricEncodingType.ISO_39794: return ISO_39794_ENCODER;
                default: return ISO_19794_ENCODER;
            }
        }
    }
}
