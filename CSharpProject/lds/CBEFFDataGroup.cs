using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.jmrtd.cbeff;

namespace org.jmrtd.lds
{
    public abstract class CBEFFDataGroup : DataGroup
    {
        protected BiometricEncodingType? encodingType;
        protected readonly List<BiometricDataBlock> subRecords;

        protected CBEFFDataGroup(short dataGroupNumber, Stream inputStream, bool doRead)
            : base(dataGroupNumber, inputStream)
        {
            if (doRead)
            {
                // The base constructor already invoked ReadContent(inputStream)
            }
            subRecords = new List<BiometricDataBlock>();
        }

        protected CBEFFDataGroup(short dataGroupNumber, BiometricEncodingType encodingType, ICollection<BiometricDataBlock> biometricDataBlocks, bool doRead)
            : base(dataGroupNumber)
        {
            this.encodingType = encodingType;
            this.subRecords = biometricDataBlocks?.ToList() ?? new List<BiometricDataBlock>();
        }

        public List<BiometricDataBlock> GetSubRecords() => subRecords.ToList();

        protected override void ReadContent(Stream inputStream)
        {
            var decoder = GetDecoder();
            if (decoder == null) throw new InvalidOperationException("CBEFF decoder not provided");

            var complex = decoder.Decode(inputStream);
            // Capture encoding type detected by decoder
            encodingType = decoder.GetEncodingType();

            subRecords.Clear();
            if (complex != null)
            {
                foreach (var info in complex.GetSubRecords())
                {
                    if (info is SimpleCBEFFInfo<BiometricDataBlock> simple)
                    {
                        var bdb = simple.GetBiometricDataBlock();
                        if (bdb != null) subRecords.Add(bdb);
                    }
                }
            }
        }

        protected override void WriteContent(Stream outputStream)
        {
            var encoder = GetEncoder();
            if (encoder == null) throw new InvalidOperationException("CBEFF encoder not provided");

            // Wrap subRecords into a ComplexCBEFFInfo for encoding
            var complex = new ComplexCBEFFInfo<BiometricDataBlock>();
            foreach (var bdb in subRecords)
            {
                complex.Add(new SimpleCBEFFInfo<BiometricDataBlock>(bdb));
            }
            encoder.Encode(complex, outputStream);
        }

        public abstract ISO781611Decoder<BiometricDataBlock> GetDecoder();
        public abstract ISO781611Encoder<BiometricDataBlock> GetEncoder();

        public override string ToString()
        {
            return $"{GetType().Name} [{string.Join(", ", subRecords)}]";
        }
    }
}
