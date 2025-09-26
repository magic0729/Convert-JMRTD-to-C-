using System;
using System.IO;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds.icao
{
    public class DG1File : DataGroup
    {
        private const short MRZ_INFO_TAG = 24351;
        private readonly MRZInfo mrzInfo;

        public DG1File(MRZInfo mrzInfo) : base(97)
        {
            this.mrzInfo = mrzInfo ?? throw new ArgumentNullException(nameof(mrzInfo));
        }

        public DG1File(Stream inputStream) : base(97, inputStream)
        {
            // TODO: Initialize mrzInfo from stream
            this.mrzInfo = new MRZInfo(""); // Placeholder
        }

        public MRZInfo GetMRZInfo() => mrzInfo;

        protected override void ReadContent(Stream inputStream)
        {
            // Read TLV structure
            var tlv = TLVUtil.ReadTLV(inputStream);
            
            if (tlv.GetTag() != MRZ_INFO_TAG)
            {
                throw new ArgumentException($"Expected MRZ info tag {MRZ_INFO_TAG:X4}, got {tlv.GetTag():X4}");
            }
            
            // Parse MRZ from TLV value
            using var mrzStream = new MemoryStream(tlv.GetValue());
            using var reader = new StreamReader(mrzStream);
            string mrzText = reader.ReadToEnd();
            
            // Update MRZ info
            var mrzInfoField = typeof(DG1File).GetField("mrzInfo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            mrzInfoField?.SetValue(this, new MRZInfo(mrzText));
        }

        protected override void WriteContent(Stream outputStream)
        {
            // Write MRZ info as TLV
            using var mrzStream = new MemoryStream();
            mrzInfo.WriteObject(mrzStream);
            byte[] mrzData = mrzStream.ToArray();
            
            var tlv = new TLVObject(MRZ_INFO_TAG, mrzData);
            TLVUtil.WriteTLV(outputStream, tlv);
        }

        public override string ToString()
        {
            return $"DG1File {mrzInfo.ToString().Replace("\n", "").Trim()}";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var other = (DG1File)obj;
            return mrzInfo.Equals(other.mrzInfo);
        }

        public override int GetHashCode()
        {
            return 3 * mrzInfo.GetHashCode() + 57;
        }
    }
}
