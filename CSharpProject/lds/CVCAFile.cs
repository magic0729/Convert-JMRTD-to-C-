using System;
using System.IO;
using org.jmrtd.cert;

namespace org.jmrtd.lds
{
    public class CVCAFile : AbstractLDSFile
    {
        public const byte CAR_TAG = 66;
        public const int LENGTH = 36;

        private short fid;
        private string? caReference;
        private string? altCAReference;

        public CVCAFile(Stream inputStream) : this(284, inputStream)
        {
        }

        public CVCAFile(short fid, Stream inputStream) : base(CAR_TAG, inputStream)
        {
            this.fid = fid;
        }

        public CVCAFile(string caReference, string? altCAReference) : this(284, caReference, altCAReference)
        {
        }

        public CVCAFile(short fid, string caReference, string? altCAReference) : base(CAR_TAG)
        {
            if (caReference == null || caReference.Length > 16 || (altCAReference != null && altCAReference.Length > 16))
            {
                throw new ArgumentException();
            }
            this.fid = fid;
            this.caReference = caReference;
            this.altCAReference = altCAReference;
        }

        public CVCAFile(short fid, string caReference) : this(fid, caReference, null)
        {
        }

        public short GetFID() => fid;

        protected override void ReadContent(Stream inputStream)
        {
            using var dataIn = new BinaryReader(inputStream);
            int tag = dataIn.ReadByte();
            if (tag != CAR_TAG)
            {
                throw new ArgumentException($"Wrong tag, expected {CAR_TAG:X}, found {tag:X}");
            }
            int length = dataIn.ReadByte();
            if (length > 16)
            {
                throw new ArgumentException("Wrong length");
            }
            byte[] data = dataIn.ReadBytes(length);
            caReference = System.Text.Encoding.UTF8.GetString(data);
            
            tag = dataIn.ReadByte();
            if (tag != 0 && tag != -1)
            {
                if (tag != CAR_TAG)
                {
                    throw new ArgumentException("Wrong tag");
                }
                length = dataIn.ReadByte();
                if (length > 16)
                {
                    throw new ArgumentException("Wrong length");
                }
                data = dataIn.ReadBytes(length);
                altCAReference = System.Text.Encoding.UTF8.GetString(data);
                tag = dataIn.ReadByte();
            }
            while (tag != -1)
            {
                if (tag != 0)
                {
                    throw new ArgumentException("Bad file padding");
                }
                tag = dataIn.ReadByte();
            }
        }

        protected override void WriteContent(Stream outputStream)
        {
            byte[] result = new byte[LENGTH];
            result[0] = CAR_TAG;
            result[1] = (byte)caReference!.Length;
            Array.Copy(System.Text.Encoding.UTF8.GetBytes(caReference), 0, result, 2, result[1]);
            if (altCAReference != null)
            {
                int index = result[1] + 2;
                result[index] = CAR_TAG;
                result[index + 1] = (byte)altCAReference.Length;
                Array.Copy(System.Text.Encoding.UTF8.GetBytes(altCAReference), 0, result, index + 2, result[index + 1]);
            }
            outputStream.Write(result, 0, result.Length);
        }

        public CVCPrincipal? GetCAReference()
        {
            return caReference == null ? null : new CVCPrincipal(caReference.Substring(0, 2), caReference.Substring(2), 0);
        }

        public CVCPrincipal? GetAltCAReference()
        {
            return altCAReference == null ? null : new CVCPrincipal(altCAReference.Substring(0, 2), altCAReference.Substring(2), 0);
        }

        public override string ToString()
        {
            return "CA reference: \"" + caReference + "\"" + (altCAReference != null ? ", Alternative CA reference: " + altCAReference : "");
        }

        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (other.GetType() != GetType()) return false;

            var otherCVCAFile = (CVCAFile)other;
            return caReference!.Equals(otherCVCAFile.caReference) && 
                   (altCAReference == null && otherCVCAFile.altCAReference == null || 
                    altCAReference != null && altCAReference.Equals(otherCVCAFile.altCAReference));
        }

        public override int GetHashCode()
        {
            return 11 * caReference!.GetHashCode() + (altCAReference != null ? 13 * altCAReference.GetHashCode() : 0) + 5;
        }

        public int GetLength() => LENGTH;
    }
}
