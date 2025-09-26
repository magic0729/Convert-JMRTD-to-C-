using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace org.jmrtd.lds
{
    public class CardAccessFile
    {
        private HashSet<SecurityInfo>? securityInfos;

        public CardAccessFile(ICollection<SecurityInfo> securityInfos)
        {
            if (securityInfos == null)
            {
                throw new ArgumentNullException(nameof(securityInfos));
            }
            this.securityInfos = new HashSet<SecurityInfo>(securityInfos);
        }

        public CardAccessFile(Stream inputStream)
        {
            ReadContent(inputStream);
        }

        protected void ReadContent(Stream inputStream)
        {
            using var ms = new MemoryStream();
            inputStream.CopyTo(ms);
            var data = ms.ToArray();

            securityInfos = new HashSet<SecurityInfo>();
            try
            {
                var reader = new System.Formats.Asn1.AsnReader(data, System.Formats.Asn1.AsnEncodingRules.DER);
                // CardAccess is SEQUENCE OF SecurityInfo (accept SET too)
                var container = reader.PeekTag();
                if (container.TagClass == System.Formats.Asn1.TagClass.Universal && (container.TagValue == (int)System.Formats.Asn1.UniversalTagNumber.Sequence || container.TagValue == (int)System.Formats.Asn1.UniversalTagNumber.Set))
                {
                    var seq = container.TagValue == (int)System.Formats.Asn1.UniversalTagNumber.Sequence ? reader.ReadSequence() : reader.ReadSetOf();
                    while (seq.HasData)
                    {
                        var encoded = seq.ReadEncodedValue();
                        var si = SecurityInfo.GetInstance(encoded);
                        if (si != null) securityInfos.Add(si);
                    }
                }
            }
            catch
            {
                // Leave empty if parsing fails
            }
        }

        protected void WriteContent(Stream outputStream)
        {
            // TODO: Implement ASN1 encoding when ASN1 support is added
            // This would create an ASN1Set from securityInfos and encode it
            throw new NotImplementedException("ASN1 encoding not yet implemented");
        }

        public byte[] GetEncoded()
        {
            using var byteArrayOutputStream = new MemoryStream();
            try
            {
                WriteContent(byteArrayOutputStream);
                byteArrayOutputStream.Flush();
                return byteArrayOutputStream.ToArray();
            }
            catch (IOException)
            {
                // Log warning: "Exception while encoding"
                return Array.Empty<byte>();
            }
        }

        public ICollection<SecurityInfo> GetSecurityInfos()
        {
            return securityInfos?.ToList().AsReadOnly() ?? new List<SecurityInfo>().AsReadOnly();
        }

        public override string ToString()
        {
            return $"CardAccessFile [{string.Join(", ", securityInfos ?? new HashSet<SecurityInfo>())}]";
        }

        public override bool Equals(object? otherObj)
        {
            if (otherObj == null) return false;
            if (otherObj.GetType() != GetType()) return false;

            var other = (CardAccessFile)otherObj;
            if (securityInfos == null) return other.securityInfos == null;
            if (other.securityInfos == null) return securityInfos == null;
            return securityInfos.SetEquals(other.securityInfos);
        }

        public override int GetHashCode()
        {
            return 7 * (securityInfos?.GetHashCode() ?? 0) + 61;
        }
    }
}
