using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Formats.Asn1;

namespace org.jmrtd.lds
{
    public class SODFile : AbstractTaggedLDSFile
    {
        private const string ICAO_LDS_SOD_OID = "2.23.136.1.1.1";
        private const string ICAO_LDS_SOD_ALT_OID = "1.3.27.1.1.1";
        private const string SDU_LDS_SOD_OID = "1.2.528.1.1006.1.20.1";

        private SignedCms? signedCms;
        private Dictionary<int, byte[]>? dataGroupHashes;

        public SODFile(string digestAlgorithm, string digestEncryptionAlgorithm, Dictionary<int, byte[]> dataGroupHashes, AsymmetricAlgorithm privateKey, X509Certificate2 docSigningCertificate)
            : this(digestAlgorithm, digestEncryptionAlgorithm, dataGroupHashes, privateKey, docSigningCertificate, null)
        {
        }

        public SODFile(string digestAlgorithm, string digestEncryptionAlgorithm, object digestEncryptionParameters, Dictionary<int, byte[]> dataGroupHashes, AsymmetricAlgorithm privateKey, X509Certificate2 docSigningCertificate)
            : this(digestAlgorithm, digestEncryptionAlgorithm, digestEncryptionParameters, dataGroupHashes, privateKey, docSigningCertificate, null)
        {
        }

        public SODFile(string digestAlgorithm, string digestEncryptionAlgorithm, Dictionary<int, byte[]> dataGroupHashes, AsymmetricAlgorithm privateKey, X509Certificate2 docSigningCertificate, string? provider)
            : this(digestAlgorithm, digestEncryptionAlgorithm, dataGroupHashes, privateKey, docSigningCertificate, provider, null, null)
        {
        }

        public SODFile(string digestAlgorithm, string digestEncryptionAlgorithm, object digestEncryptionParameters, Dictionary<int, byte[]> dataGroupHashes, AsymmetricAlgorithm privateKey, X509Certificate2 docSigningCertificate, string? provider)
            : this(digestAlgorithm, digestEncryptionAlgorithm, digestEncryptionParameters, dataGroupHashes, privateKey, docSigningCertificate, provider, null, null)
        {
        }

        public SODFile(string digestAlgorithm, string digestEncryptionAlgorithm, Dictionary<int, byte[]> dataGroupHashes, AsymmetricAlgorithm privateKey, X509Certificate2 docSigningCertificate, string? provider, string? ldsVersion, string? unicodeVersion)
            : base(119)
        {
            try
            {
                this.dataGroupHashes = new Dictionary<int, byte[]>(dataGroupHashes);
                CreateSignedCms(digestAlgorithm, digestEncryptionAlgorithm, dataGroupHashes, privateKey, docSigningCertificate, ldsVersion, unicodeVersion);
            }
            catch (Exception ioe)
            {
                throw new ArgumentException("Error creating signedData", ioe);
            }
        }

        public SODFile(string digestAlgorithm, string digestEncryptionAlgorithm, object digestEncryptionParameters, Dictionary<int, byte[]> dataGroupHashes, AsymmetricAlgorithm privateKey, X509Certificate2 docSigningCertificate, string? provider, string? ldsVersion, string? unicodeVersion)
            : base(119)
        {
            try
            {
                this.dataGroupHashes = new Dictionary<int, byte[]>(dataGroupHashes);
                CreateSignedCms(digestAlgorithm, digestEncryptionAlgorithm, dataGroupHashes, privateKey, docSigningCertificate, ldsVersion, unicodeVersion);
            }
            catch (Exception ioe)
            {
                throw new ArgumentException("Error creating signedData", ioe);
            }
        }

        public SODFile(string digestAlgorithm, string digestEncryptionAlgorithm, Dictionary<int, byte[]> dataGroupHashes, byte[] encryptedDigest, X509Certificate2 docSigningCertificate)
            : base(119)
        {
            if (dataGroupHashes == null)
            {
                throw new ArgumentException("Cannot construct security object from null datagroup hashes");
            }
            try
            {
                this.dataGroupHashes = new Dictionary<int, byte[]>(dataGroupHashes);
                // For pre-signed data, we would need to reconstruct the SignedCms from the encrypted digest
                // This is a complex operation that would require the full ASN.1 structure
                throw new NotImplementedException("Creating SOD from pre-encrypted digest is not yet implemented");
            }
            catch (Exception ioe)
            {
                throw new ArgumentException("Error creating signedData", ioe);
            }
        }

        public SODFile(string digestAlgorithm, string digestEncryptionAlgorithm, object digestEncryptionParameters, Dictionary<int, byte[]> dataGroupHashes, byte[] encryptedDigest, X509Certificate2 docSigningCertificate)
            : base(119)
        {
            if (dataGroupHashes == null)
            {
                throw new ArgumentException("Cannot construct security object from null datagroup hashes");
            }
            try
            {
                this.dataGroupHashes = new Dictionary<int, byte[]>(dataGroupHashes);
                // For pre-signed data, we would need to reconstruct the SignedCms from the encrypted digest
                // This is a complex operation that would require the full ASN.1 structure
                throw new NotImplementedException("Creating SOD from pre-encrypted digest is not yet implemented");
            }
            catch (Exception ioe)
            {
                throw new ArgumentException("Error creating signedData", ioe);
            }
        }

        public SODFile(Stream inputStream) : base(119, inputStream)
        {
            // ReadContent will be called by base constructor
        }

        protected override void ReadContent(Stream inputStream)
        {
            try
            {
                // Read all bytes from the stream
                using var memoryStream = new MemoryStream();
                inputStream.CopyTo(memoryStream);
                byte[] cmsData = memoryStream.ToArray();

                // Parse as SignedCms
                signedCms = new SignedCms();
                signedCms.Decode(cmsData);

                // Extract data group hashes from eContent
                ExtractDataGroupHashes();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read SOD content", ex);
            }
        }

        protected override void WriteContent(Stream outputStream)
        {
            if (signedCms == null)
            {
                throw new InvalidOperationException("No signed data to write");
            }

            try
            {
                byte[] encodedData = signedCms.Encode();
                outputStream.Write(encodedData, 0, encodedData.Length);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to write SOD content", ex);
            }
        }

        public Dictionary<int, byte[]> GetDataGroupHashes()
        {
            return dataGroupHashes != null ? new Dictionary<int, byte[]>(dataGroupHashes) : new Dictionary<int, byte[]>();
        }

        public byte[]? GetEncryptedDigest()
        {
            if (signedCms?.SignerInfos.Count > 0)
            {
                return signedCms.SignerInfos[0].GetSignature();
            }
            return null;
        }

        public object? GetDigestEncryptionAlgorithmParams()
        {
            // Most signature algorithms don't have additional parameters
            return null;
        }

        public byte[]? GetEContent()
        {
            return signedCms?.ContentInfo.Content;
        }

        public string? GetDigestAlgorithm()
        {
            if (signedCms?.SignerInfos.Count > 0)
            {
                var signerInfo = signedCms.SignerInfos[0];
                return signerInfo.DigestAlgorithm.FriendlyName ?? signerInfo.DigestAlgorithm.Value;
            }
            return null;
        }

        private static string? GetDigestAlgorithm(object ldsSecurityObject)
        {
            // TODO: Implement when SignedDataUtil is available
            // try
            // {
            //     return SignedDataUtil.LookupMnemonicByOID(ldsSecurityObject.GetDigestAlgorithmIdentifier().GetAlgorithm().GetId());
            // }
            // catch (NoSuchAlgorithmException nsae)
            // {
            //     // Log warning
            //     return null;
            // }
            return null;
        }

        public string? GetSignerInfoDigestAlgorithm()
        {
            return GetDigestAlgorithm();
        }

        public string? GetDigestEncryptionAlgorithm()
        {
            if (signedCms?.SignerInfos.Count > 0)
            {
                var signerInfo = signedCms.SignerInfos[0];
                return signerInfo.SignatureAlgorithm.FriendlyName ?? signerInfo.SignatureAlgorithm.Value;
            }
            return null;
        }

        public string? GetLDSVersion()
        {
            // LDS version would be extracted from the eContent ASN.1 structure
            // For now, return a default version
            return "1.7";
        }

        public string? GetUnicodeVersion()
        {
            // Unicode version would be extracted from the eContent ASN.1 structure
            // For now, return a default version
            return "6.0.0";
        }

        public List<X509Certificate2> GetDocSigningCertificates()
        {
            var certificates = new List<X509Certificate2>();
            if (signedCms?.Certificates != null)
            {
                foreach (X509Certificate2 cert in signedCms.Certificates)
                {
                    certificates.Add(cert);
                }
            }
            return certificates;
        }

        public X509Certificate2? GetDocSigningCertificate()
        {
            var certificates = GetDocSigningCertificates();
            if (certificates == null || certificates.Count == 0)
            {
                return null;
            }
            return certificates[certificates.Count - 1];
        }

        public X500DistinguishedName? GetIssuerX500Principal()
        {
            if (signedCms?.SignerInfos.Count > 0)
            {
                var signerInfo = signedCms.SignerInfos[0];
                return signerInfo.Certificate?.IssuerName;
            }
            return null;
        }

        public BigInteger? GetSerialNumber()
        {
            if (signedCms?.SignerInfos.Count > 0)
            {
                var signerInfo = signedCms.SignerInfos[0];
                if (signerInfo.Certificate != null)
                {
                    var serialBytes = signerInfo.Certificate.GetSerialNumber();
                    // Serial number is stored in little-endian format, reverse for BigInteger
                    Array.Reverse(serialBytes);
                    return new BigInteger(serialBytes, isUnsigned: true);
                }
            }
            return null;
        }

        public byte[]? GetSubjectKeyIdentifier()
        {
            if (signedCms?.SignerInfos.Count > 0)
            {
                var signerInfo = signedCms.SignerInfos[0];
                if (signerInfo.Certificate != null)
                {
                    var skiExtension = signerInfo.Certificate.Extensions["2.5.29.14"]; // Subject Key Identifier OID
                    if (skiExtension != null)
                    {
                        var ski = skiExtension as X509SubjectKeyIdentifierExtension;
                        return ski?.SubjectKeyIdentifierBytes.ToArray();
                    }
                }
            }
            return null;
        }

        public override string ToString()
        {
            try
            {
                var result = new System.Text.StringBuilder();
                result.Append("SODFile ");
                var certificates = GetDocSigningCertificates();
                foreach (var certificate in certificates)
                {
                    result.Append(certificate.Issuer);
                    result.Append(", ");
                }
                return result.ToString();
            }
            catch (Exception)
            {
                // Log warning
                return "SODFile";
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            if (obj.GetType() != GetType()) return false;

            var other = (SODFile)obj;
            return GetEncoded().SequenceEqual(other.GetEncoded());
        }

        public override int GetHashCode()
        {
            return 11 * GetEncoded().GetHashCode() + 111;
        }

        public byte[] GetEncoded()
        {
            using var outputStream = new MemoryStream();
            WriteContent(outputStream);
            return outputStream.ToArray();
        }

        private void CreateSignedCms(string digestAlgorithm, string digestEncryptionAlgorithm, Dictionary<int, byte[]> dataGroupHashes, AsymmetricAlgorithm privateKey, X509Certificate2 docSigningCertificate, string? ldsVersion, string? unicodeVersion)
        {
            try
            {
                // Create the LDS Security Object content
                byte[] ldsSecurityObjectContent = CreateLDSSecurityObjectContent(digestAlgorithm, dataGroupHashes, ldsVersion, unicodeVersion);
                // Create ContentInfo with ICAO LDS SOD OID
                var contentInfo = new ContentInfo(new Oid(ICAO_LDS_SOD_OID), ldsSecurityObjectContent);

                // Create SignedCms
                signedCms = new SignedCms(contentInfo, detached: false);

                // Create CmsSigner
                var cmsSigner = new CmsSigner(docSigningCertificate);

                // Set digest algorithm
                cmsSigner.DigestAlgorithm = new Oid(GetDigestAlgorithmOid(digestAlgorithm));

                // Sign the content
                signedCms.ComputeSignature(cmsSigner);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create SignedCms", ex);
            }
        }

        private void ExtractDataGroupHashes()
        {
            if (signedCms?.ContentInfo.Content == null)
            {
                dataGroupHashes = new Dictionary<int, byte[]>();
                return;
            }

            try
            {
                // Parse the eContent as ASN.1 to extract data group hashes
                dataGroupHashes = ParseLDSSecurityObject(signedCms.ContentInfo.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to extract data group hashes: {ex.Message}");
                dataGroupHashes = new Dictionary<int, byte[]>();
            }
        }

        private byte[] CreateLDSSecurityObjectContent(string digestAlgorithm, Dictionary<int, byte[]> dataGroupHashes, string? ldsVersion, string? unicodeVersion)
        {
            try
            {
                var writer = new AsnWriter(AsnEncodingRules.DER);

                // LDSSecurityObject ::= SEQUENCE {
                writer.PushSequence();

                // version INTEGER,
                writer.WriteInteger(0);

                // hashAlgorithm AlgorithmIdentifier,
                writer.PushSequence();
                writer.WriteObjectIdentifier(GetDigestAlgorithmOid(digestAlgorithm));
                writer.WriteNull(); // parameters
                writer.PopSequence();

                // dataGroupHashValues SEQUENCE OF DataGroupHash
                writer.PushSequence();
                foreach (var kvp in dataGroupHashes.OrderBy(x => x.Key))
                {
                    // DataGroupHash ::= SEQUENCE {
                    writer.PushSequence();
                    writer.WriteInteger(kvp.Key); // dataGroupNumber INTEGER,
                    writer.WriteOctetString(kvp.Value); // dataGroupHashValue OCTET STRING
                    writer.PopSequence();
                }
                writer.PopSequence();

                // Optional: LDSVersionInfo
                if (ldsVersion != null)
                {
                    writer.PushSequence();
                    writer.WriteCharacterString(UniversalTagNumber.UTF8String, ldsVersion);
                    if (unicodeVersion != null)
                    {
                        writer.WriteCharacterString(UniversalTagNumber.UTF8String, unicodeVersion);
                    }
                    writer.PopSequence();
                }

                writer.PopSequence();

                return writer.Encode();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create LDS Security Object content", ex);
            }
        }

        private Dictionary<int, byte[]> ParseLDSSecurityObject(byte[] content)
        {
            var hashes = new Dictionary<int, byte[]>();

            try
            {
                var reader = new AsnReader(content, AsnEncodingRules.DER);
                var sequence = reader.ReadSequence();

                // Skip version
                sequence.ReadInteger();

                // Skip hashAlgorithm
                sequence.ReadSequence();

                // Read dataGroupHashValues
                var hashSequence = sequence.ReadSequence();
                while (hashSequence.HasData)
                {
                    var hashEntry = hashSequence.ReadSequence();
                    int dgNumber = (int)hashEntry.ReadInteger();
                    byte[] hashValue = hashEntry.ReadOctetString();
                    hashes[dgNumber] = hashValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to parse LDS Security Object: {ex.Message}");
            }

            return hashes;
        }

        private static string GetDigestAlgorithmOid(string digestAlgorithm)
        {
            return digestAlgorithm.ToUpperInvariant() switch
            {
                "SHA1" or "SHA-1" => "1.3.14.3.2.26",
                "SHA256" or "SHA-256" => "2.16.840.1.101.3.4.2.1",
                "SHA384" or "SHA-384" => "2.16.840.1.101.3.4.2.2",
                "SHA512" or "SHA-512" => "2.16.840.1.101.3.4.2.3",
                _ => "2.16.840.1.101.3.4.2.1" // Default to SHA-256
            };
        }

        /// <summary>
        /// Verify the SOD signature and data group hashes
        /// </summary>
        /// <param name="dataGroups">Dictionary of data group number to data group content</param>
        /// <param name="trustAnchors">Collection of trusted CSCA certificates</param>
        /// <param name="details">Output parameter containing verification details</param>
        /// <returns>True if verification succeeds, false otherwise</returns>
        public bool Verify(IDictionary<int, byte[]> dataGroups, X509Certificate2Collection trustAnchors, out string details)
        {
            var detailsList = new List<string>();
            bool success = true;

            try
            {
                if (signedCms == null)
                {
                    details = "No signed data available";
                    return false;
                }

                // Step 1: Verify the CMS signature
                try
                {
                    signedCms.CheckSignature(verifySignatureOnly: true);
                    detailsList.Add("✓ CMS signature verification passed");
                }
                catch (Exception ex)
                {
                    detailsList.Add($"✗ CMS signature verification failed: {ex.Message}");
                    success = false;
                }

                // Step 2: Verify certificate chain to trust anchors
                if (signedCms.SignerInfos.Count > 0)
                {
                    var signerInfo = signedCms.SignerInfos[0];
                    if (signerInfo.Certificate != null)
                    {
                        try
                        {
                            var chain = new X509Chain();
                            chain.ChainPolicy.ExtraStore.AddRange(signedCms.Certificates);
                            chain.ChainPolicy.ExtraStore.AddRange(trustAnchors);
                            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Passport certificates don't use CRL/OCSP

                            bool chainValid = chain.Build(signerInfo.Certificate);
                            if (chainValid)
                            {
                                detailsList.Add("✓ Certificate chain validation passed");
                            }
                            else
                            {
                                detailsList.Add($"✗ Certificate chain validation failed");
                                foreach (var status in chain.ChainStatus)
                                {
                                    detailsList.Add($"  - {status.Status}: {status.StatusInformation}");
                                }
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            detailsList.Add($"✗ Certificate chain validation error: {ex.Message}");
                            success = false;
                        }
                    }
                    else
                    {
                        detailsList.Add("✗ No signer certificate found");
                        success = false;
                    }
                }

                // Step 3: Verify data group hashes
                var sodHashes = GetDataGroupHashes();
                detailsList.Add($"SOD contains hashes for {sodHashes.Count} data groups");

                foreach (var kvp in dataGroups)
                {
                    int dgNumber = kvp.Key;
                    byte[] dgContent = kvp.Value;

                    if (sodHashes.TryGetValue(dgNumber, out byte[]? expectedHash))
                    {
                        // Compute hash of the data group
                        string? hashAlgorithm = GetDigestAlgorithm();
                        byte[] computedHash = ComputeHash(dgContent, hashAlgorithm ?? "SHA256");

                        if (computedHash.SequenceEqual(expectedHash))
                        {
                            detailsList.Add($"✓ DG{dgNumber} hash verification passed");
                        }
                        else
                        {
                            detailsList.Add($"✗ DG{dgNumber} hash verification failed");
                            detailsList.Add($"  Expected: {BitConverter.ToString(expectedHash)}");
                            detailsList.Add($"  Computed: {BitConverter.ToString(computedHash)}");
                            success = false;
                        }
                    }
                    else
                    {
                        detailsList.Add($"⚠ DG{dgNumber} not found in SOD hashes");
                    }
                }

                details = string.Join(Environment.NewLine, detailsList);
                return success;
            }
            catch (Exception ex)
            {
                details = $"Verification error: {ex.Message}";
                return false;
            }
        }

        private static byte[] ComputeHash(byte[] data, string algorithm)
        {
            using HashAlgorithm hashAlg = algorithm.ToUpperInvariant() switch
            {
                "SHA1" or "SHA-1" => SHA1.Create(),
                "SHA256" or "SHA-256" => SHA256.Create(),
                "SHA384" or "SHA-384" => SHA384.Create(),
                "SHA512" or "SHA-512" => SHA512.Create(),
                _ => SHA256.Create()
            };

            return hashAlg.ComputeHash(data);
        }
    }
}

