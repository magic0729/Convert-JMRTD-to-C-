using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Formats.Asn1;

namespace org.jmrtd.lds
{
    public class CardSecurityFile
    {
        private const string CONTENT_TYPE_OID = "0.4.0.127.0.7.3.2.1";
        private string? digestAlgorithm;
        private string? digestEncryptionAlgorithm;
        private HashSet<SecurityInfo>? securityInfos;
        private byte[]? encryptedDigest;
        private X509Certificate2? certificate;

        public CardSecurityFile(string digestAlgorithm, string digestEncryptionAlgorithm, ICollection<SecurityInfo> securityInfos, RSA privateKey, X509Certificate2 certificate)
            : this(digestAlgorithm, digestEncryptionAlgorithm, securityInfos, privateKey, certificate, null)
        {
        }

        public CardSecurityFile(string digestAlgorithm, string digestEncryptionAlgorithm, ICollection<SecurityInfo> securityInfos, RSA privateKey, X509Certificate2 certificate, string? provider)
            : this(digestAlgorithm, digestEncryptionAlgorithm, securityInfos, (byte[]?)null, certificate)
        {
            // TODO: Implement SignedDataUtil.signData when crypto support is added
            // var contentInfo = ToContentInfo(CONTENT_TYPE_OID, securityInfos);
            // this.encryptedDigest = SignedDataUtil.signData(digestAlgorithm, digestEncryptionAlgorithm, CONTENT_TYPE_OID, contentInfo, privateKey, provider);
        }

        public CardSecurityFile(string digestAlgorithm, string digestEncryptionAlgorithm, ICollection<SecurityInfo> securityInfos, byte[]? encryptedDigest, X509Certificate2 certificate)
        {
            if (securityInfos == null)
            {
                throw new ArgumentNullException(nameof(securityInfos));
            }
            if (certificate == null)
            {
                throw new ArgumentNullException(nameof(certificate));
            }
            this.digestAlgorithm = digestAlgorithm;
            this.digestEncryptionAlgorithm = digestEncryptionAlgorithm;
            this.securityInfos = new HashSet<SecurityInfo>(securityInfos);
            this.encryptedDigest = encryptedDigest;
            this.certificate = certificate;
        }

        public CardSecurityFile(Stream inputStream)
        {
            ReadContent(inputStream);
        }

        public string? GetDigestAlgorithm() => digestAlgorithm;
        public string? GetDigestEncryptionAlgorithm() => digestEncryptionAlgorithm;

        public byte[]? GetEncryptedDigest()
        {
            return encryptedDigest?.ToArray();
        }

        protected void ReadContent(Stream inputStream)
        {
            using var ms = new MemoryStream();
            inputStream.CopyTo(ms);
            var der = ms.ToArray();

            // Parse ContentInfo { contentType, [0] EXPLICIT content }
            var reader = new AsnReader(der, AsnEncodingRules.DER);
            var contentInfo = reader.ReadSequence();
            string contentTypeOid = contentInfo.ReadObjectIdentifier();

            // [0] EXPLICIT SignedData
            var ctx0 = contentInfo.ReadEncodedValue();
            var ctxReader = new AsnReader(ctx0, AsnEncodingRules.DER);
            var signedDataExplicit = ctxReader.ReadEncodedValue();
            var explicitReader = new AsnReader(signedDataExplicit, AsnEncodingRules.DER);
            var signedData = explicitReader.ReadSequence();

            // SignedData ::= SEQUENCE { version, digestAlgorithms, encapContentInfo, certificates [0] IMPLICIT CertificateSet OPTIONAL, ... }
            signedData.ReadInteger(); // version
            signedData.ReadSetOf(); // digestAlgorithms (skip)

            // encapContentInfo ::= SEQUENCE { eContentType, [0] EXPLICIT OCTET STRING OPTIONAL }
            var encap = signedData.ReadSequence();
            string eContentType = encap.ReadObjectIdentifier();
            byte[]? eContent = null;
            if (encap.HasData)
            {
                var eCtx = encap.ReadEncodedValue();
                var eReader = new AsnReader(eCtx, AsnEncodingRules.DER);
                // [0] EXPLICIT OCTET STRING
                var eExplicit = eReader.ReadEncodedValue();
                var eExplicitReader = new AsnReader(eExplicit, AsnEncodingRules.DER);
                eContent = eExplicitReader.ReadOctetString();
            }

            // certificates [0] IMPLICIT CertificateSet OPTIONAL
            X509Certificate2? cert = null;
            if (signedData.HasData)
            {
                var pk = signedData.PeekTag();
                if (pk.TagClass == TagClass.ContextSpecific && pk.TagValue == 0)
                {
                    var certSet = signedData.ReadEncodedValue();
                    // Treat as SET OF Certificate (DER-encoded X.509)
                    var setReader = new AsnReader(certSet, AsnEncodingRules.DER);
                    var set = setReader.ReadSetOf();
                    if (set.HasData)
                    {
                        var certDer = set.ReadEncodedValue();
                        try { cert = System.Security.Cryptography.X509Certificates.X509CertificateLoader.LoadCertificate(certDer.ToArray()); } catch { }
                    }
                }
            }

            // signatures (ignore)

            certificate = cert;
            // Content type should be CONTENT_TYPE_OID; eContent holds DER of SecurityInfos container (SET/SEQUENCE)
            securityInfos = new HashSet<SecurityInfo>();
            if (eContent != null)
            {
                try
                {
                    var siReader = new AsnReader(eContent, AsnEncodingRules.DER);
                    // It may be OCTET STRING wrapping again
                    if (siReader.PeekTag().TagClass == TagClass.Universal && siReader.PeekTag().TagValue == (int)UniversalTagNumber.OctetString)
                    {
                        eContent = siReader.ReadOctetString();
                        siReader = new AsnReader(eContent, AsnEncodingRules.DER);
                    }

                    var container = siReader.PeekTag();
                    var seq = container.TagValue == (int)UniversalTagNumber.Set ? siReader.ReadSetOf() : siReader.ReadSequence();
                    while (seq.HasData)
                    {
                        var encoded = seq.ReadEncodedValue();
                        var si = SecurityInfo.GetInstance(encoded);
                        if (si != null) securityInfos.Add(si);
                    }
                }
                catch { }
            }
        }

        protected void WriteContent(Stream outputStream)
        {
            try
            {
                // TODO: Implement SignedDataUtil when crypto support is added
                // var contentInfo = ToContentInfo(CONTENT_TYPE_OID, securityInfos);
                // var signedData = SignedDataUtil.createSignedData(digestAlgorithm, digestEncryptionAlgorithm, CONTENT_TYPE_OID, contentInfo, encryptedDigest, certificate);
                // SignedDataUtil.writeData(signedData, outputStream);
            }
            catch (CryptographicException ce)
            {
                throw new IOException("Certificate exception during SignedData creation", ce);
            }
            catch (NotSupportedException nsae)
            {
                throw new IOException("Unsupported algorithm", nsae);
            }
            catch (Exception gse)
            {
                throw new IOException("General security exception", gse);
            }
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

        [Obsolete("This method is deprecated.")]
        public ICollection<object> GetPACEInfos()
        {
            var paceInfos = new List<object>(securityInfos?.Count ?? 0);
            if (securityInfos != null)
            {
                foreach (var securityInfo in securityInfos)
                {
                    // TODO: Check for PACEInfo when it's converted to a class
                    // if (securityInfo is PACEInfo paceInfo)
                    // {
                    //     paceInfos.Add(paceInfo);
                    // }
                }
            }
            return paceInfos;
        }

        [Obsolete("This method is deprecated.")]
        public ICollection<ChipAuthenticationInfo> GetChipAuthenticationInfos()
        {
            var chipAuthenticationInfos = new List<ChipAuthenticationInfo>(securityInfos?.Count ?? 0);
            if (securityInfos != null)
            {
                foreach (var securityInfo in securityInfos)
                {
                    if (securityInfo is ChipAuthenticationInfo chipAuthInfo)
                    {
                        chipAuthenticationInfos.Add(chipAuthInfo);
                    }
                }
            }
            return chipAuthenticationInfos;
        }

        [Obsolete("This method is deprecated.")]
        public ICollection<ChipAuthenticationPublicKeyInfo> GetChipAuthenticationPublicKeyInfos()
        {
            var chipAuthenticationPublicKeyInfos = new List<ChipAuthenticationPublicKeyInfo>(securityInfos?.Count ?? 0);
            if (securityInfos != null)
            {
                foreach (var securityInfo in securityInfos)
                {
                    if (securityInfo is ChipAuthenticationPublicKeyInfo chipAuthPubKeyInfo)
                    {
                        chipAuthenticationPublicKeyInfos.Add(chipAuthPubKeyInfo);
                    }
                }
            }
            return chipAuthenticationPublicKeyInfos;
        }

        public override string ToString()
        {
            return $"CardSecurityFile [{string.Join(", ", securityInfos ?? new HashSet<SecurityInfo>())}]";
        }

        public override bool Equals(object? otherObj)
        {
            if (otherObj == null) return false;
            if (otherObj.GetType() != GetType()) return false;

            var other = (CardSecurityFile)otherObj;
            if (securityInfos == null) return other.securityInfos == null;
            if (other.securityInfos == null) return securityInfos == null;
            return securityInfos.SetEquals(other.securityInfos);
        }

        public override int GetHashCode()
        {
            return 3 * (securityInfos?.GetHashCode() ?? 0) + 63;
        }

        private static object ToContentInfo(string contentTypeOID, ICollection<SecurityInfo> securityInfos)
        {
            try
            {
                // TODO: Implement ASN1 encoding when ASN1 support is added
                // This would create an ASN1EncodableVector, add SecurityInfo DER objects, create DLSet, and return ContentInfo
                throw new NotImplementedException("ASN1 encoding not yet implemented");
            }
            catch (IOException ioe)
            {
                // Log warning: "Error creating signedData"
                throw new ArgumentException("Error DER encoding the security infos", ioe);
            }
        }

        private static HashSet<SecurityInfo> GetSecurityInfos(object signedData)
        {
            // TODO: Implement SignedDataUtil.getContent and ASN1 parsing when crypto support is added
            throw new NotImplementedException("SignedData parsing not yet implemented");
        }
    }
}
