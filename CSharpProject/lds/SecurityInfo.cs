using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Formats.Asn1;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds
{
    public abstract class SecurityInfo : AbstractLDSInfo
    {
        // Constants from Java SecurityInfo
        public const string ID_AA = "2.23.136.1.1.5";
        public const string ID_PK_DH = "0.4.0.127.0.7.2.2.1.1"; // EACObjectIdentifiers.id_PK_DH
        public const string ID_PK_ECDH = "0.4.0.127.0.7.2.2.1.2"; // EACObjectIdentifiers.id_PK_ECDH
        public const string ID_CA_DH_3DES_CBC_CBC = "0.4.0.127.0.7.2.2.3.1.1"; // EACObjectIdentifiers.id_CA_DH_3DES_CBC_CBC
        public const string ID_CA_ECDH_3DES_CBC_CBC = "0.4.0.127.0.7.2.2.3.2.1"; // EACObjectIdentifiers.id_CA_ECDH_3DES_CBC_CBC
        public const string ID_CA_DH_AES_CBC_CMAC_128 = "0.4.0.127.0.7.2.2.3.1.2";
        public const string ID_CA_DH_AES_CBC_CMAC_192 = "0.4.0.127.0.7.2.2.3.1.3";
        public const string ID_CA_DH_AES_CBC_CMAC_256 = "0.4.0.127.0.7.2.2.3.1.4";
        public const string ID_CA_ECDH_AES_CBC_CMAC_128 = "0.4.0.127.0.7.2.2.3.2.2";
        public const string ID_CA_ECDH_AES_CBC_CMAC_192 = "0.4.0.127.0.7.2.2.3.2.3";
        public const string ID_CA_ECDH_AES_CBC_CMAC_256 = "0.4.0.127.0.7.2.2.3.2.4";
        public const string ID_TA = "0.4.0.127.0.7.2.2.2.1"; // EACObjectIdentifiers.id_TA
        public const string ID_TA_RSA = "0.4.0.127.0.7.2.2.2.2"; // EACObjectIdentifiers.id_TA_RSA
        public const string ID_TA_RSA_V1_5_SHA_1 = "0.4.0.127.0.7.2.2.2.2.1"; // EACObjectIdentifiers.id_TA_RSA_v1_5_SHA_1
        public const string ID_TA_RSA_V1_5_SHA_256 = "0.4.0.127.0.7.2.2.2.2.2"; // EACObjectIdentifiers.id_TA_RSA_v1_5_SHA_256
        public const string ID_TA_RSA_PSS_SHA_1 = "0.4.0.127.0.7.2.2.2.2.3"; // EACObjectIdentifiers.id_TA_RSA_PSS_SHA_1
        public const string ID_TA_RSA_PSS_SHA_256 = "0.4.0.127.0.7.2.2.2.2.4"; // EACObjectIdentifiers.id_TA_RSA_PSS_SHA_256
        public const string ID_TA_ECDSA = "0.4.0.127.0.7.2.2.2.3"; // EACObjectIdentifiers.id_TA_ECDSA
        public const string ID_TA_ECDSA_SHA_1 = "0.4.0.127.0.7.2.2.2.3.1"; // EACObjectIdentifiers.id_TA_ECDSA_SHA_1
        public const string ID_TA_ECDSA_SHA_224 = "0.4.0.127.0.7.2.2.2.3.2"; // EACObjectIdentifiers.id_TA_ECDSA_SHA_224
        public const string ID_TA_ECDSA_SHA_256 = "0.4.0.127.0.7.2.2.2.3.3"; // EACObjectIdentifiers.id_TA_ECDSA_SHA_256
        public const string ID_EC_PUBLIC_KEY_TYPE = "1.2.840.10045.2.1"; // X9ObjectIdentifiers.id_publicKeyType
        public const string ID_EC_PUBLIC_KEY = "1.2.840.10045.2.1"; // X9ObjectIdentifiers.id_ecPublicKey
        public const string ID_PACE = "0.4.0.127.0.7.2.2.4";
        public const string ID_PACE_DH_GM = "0.4.0.127.0.7.2.2.4.1";
        public const string ID_PACE_DH_GM_3DES_CBC_CBC = "0.4.0.127.0.7.2.2.4.1.1";
        public const string ID_PACE_DH_GM_AES_CBC_CMAC_128 = "0.4.0.127.0.7.2.2.4.1.2";
        public const string ID_PACE_DH_GM_AES_CBC_CMAC_192 = "0.4.0.127.0.7.2.2.4.1.3";
        public const string ID_PACE_DH_GM_AES_CBC_CMAC_256 = "0.4.0.127.0.7.2.2.4.1.4";
        public const string ID_PACE_ECDH_GM = "0.4.0.127.0.7.2.2.4.2";
        public const string ID_PACE_ECDH_GM_3DES_CBC_CBC = "0.4.0.127.0.7.2.2.4.2.1";
        public const string ID_PACE_ECDH_GM_AES_CBC_CMAC_128 = "0.4.0.127.0.7.2.2.4.2.2";
        public const string ID_PACE_ECDH_GM_AES_CBC_CMAC_192 = "0.4.0.127.0.7.2.2.4.2.3";
        public const string ID_PACE_ECDH_GM_AES_CBC_CMAC_256 = "0.4.0.127.0.7.2.2.4.2.4";
        public const string ID_PACE_DH_IM = "0.4.0.127.0.7.2.2.4.3";
        public const string ID_PACE_DH_IM_3DES_CBC_CBC = "0.4.0.127.0.7.2.2.4.3.1";
        public const string ID_PACE_DH_IM_AES_CBC_CMAC_128 = "0.4.0.127.0.7.2.2.4.3.2";
        public const string ID_PACE_DH_IM_AES_CBC_CMAC_192 = "0.4.0.127.0.7.2.2.4.3.3";
        public const string ID_PACE_DH_IM_AES_CBC_CMAC_256 = "0.4.0.127.0.7.2.2.4.3.4";
        public const string ID_PACE_ECDH_IM = "0.4.0.127.0.7.2.2.4.4";
        public const string ID_PACE_ECDH_IM_3DES_CBC_CBC = "0.4.0.127.0.7.2.2.4.4.1";
        public const string ID_PACE_ECDH_IM_AES_CBC_CMAC_128 = "0.4.0.127.0.7.2.2.4.4.2";
        public const string ID_PACE_ECDH_IM_AES_CBC_CMAC_192 = "0.4.0.127.0.7.2.2.4.4.3";
        public const string ID_PACE_ECDH_IM_AES_CBC_CMAC_256 = "0.4.0.127.0.7.2.2.4.4.4";
        public const string ID_PACE_ECDH_CAM = "0.4.0.127.0.7.2.2.4.6";
        public const string ID_PACE_ECDH_CAM_AES_CBC_CMAC_128 = "0.4.0.127.0.7.2.2.4.6.2";
        public const string ID_PACE_ECDH_CAM_AES_CBC_CMAC_192 = "0.4.0.127.0.7.2.2.4.6.3";
        public const string ID_PACE_ECDH_CAM_AES_CBC_CMAC_256 = "0.4.0.127.0.7.2.2.4.6.4";

        [Obsolete("This method is deprecated.")]
        public abstract object GetDERObject();

        public override void WriteObject(Stream outputStream)
        {
            // Deprecated in this port; intentionally left empty to avoid using obsolete DER APIs
            // TODO: Implement proper DER encoding when ASN1 support is added
            // byte[] derEncodedBytes = derEncoded.getEncoded("DER");
            // if (derEncodedBytes == null)
            // {
            //     throw new IOException("Could not decode from DER.");
            // }
            // outputStream.Write(derEncodedBytes, 0, derEncodedBytes.Length);
        }

        public abstract string GetObjectIdentifier();
        public abstract string GetProtocolOIDString();

        public static SecurityInfo GetInstance(object obj)
        {
            try
            {
                if (obj is byte[] bytes)
                {
                    return ParseSecurityInfo(bytes);
                }
                if (obj is ReadOnlyMemory<byte> rom)
                {
                    return ParseSecurityInfo(rom.ToArray());
                }
                if (obj is Stream s)
                {
                    using var ms = new MemoryStream();
                    s.CopyTo(ms);
                    return ParseSecurityInfo(ms.ToArray());
                }
                throw new ArgumentException("Unsupported input type for SecurityInfo.GetInstance");
            }
            catch (Exception e)
            {
                throw new ArgumentException("Malformed input stream.", e);
            }
        }

        private static SecurityInfo ParseSecurityInfo(byte[] der)
        {
            var reader = new AsnReader(der, AsnEncodingRules.DER);
            var seq = reader.ReadSequence();
            string protocolOid = seq.ReadObjectIdentifier();

            // PACE Domain Parameters: AlgorithmIdentifier with parameters indicating curve or parameterId
            if (protocolOid.StartsWith(ID_PACE, StringComparison.Ordinal))
            {
                // Expect AlgorithmIdentifier (SEQUENCE)
                if (seq.HasData)
                {
                    var algId = seq.ReadSequence();
                    string algOid = algId.ReadObjectIdentifier();
                    BigInteger? paramId = null;
                    object domainParams = algOid;

                    if (algId.HasData)
                    {
                        // Parameters may be OBJECT IDENTIFIER (named curve) or INTEGER (parameterId)
                        var pk = algId.PeekTag();
                        if (pk.TagClass == TagClass.Universal && pk.TagValue == (int)UniversalTagNumber.ObjectIdentifier)
                        {
                            string namedCurveOid = algId.ReadObjectIdentifier();
                            domainParams = namedCurveOid;
                        }
                        else if (pk.TagClass == TagClass.Universal && pk.TagValue == (int)UniversalTagNumber.Integer)
                        {
                            paramId = algId.ReadInteger();
                        }
                        else
                        {
                            // Ignore unsupported parameter encodings for now
                            algId.ReadEncodedValue();
                        }
                    }

                    // Optional parameterId after AlgorithmIdentifier (as INTEGER) in some encodings
                    if (seq.HasData)
                    {
                        var tk = seq.PeekTag();
                        if (tk.TagClass == TagClass.Universal && tk.TagValue == (int)UniversalTagNumber.Integer)
                        {
                            paramId = seq.ReadInteger();
                        }
                    }

                    return new PACEDomainParameterInfo(protocolOid, domainParams, paramId);
                }
                return new PACEDomainParameterInfo(protocolOid, "1.2.840.10045.3.1.7", null);
            }

            // For now, return a generic stub for other SecurityInfos until fully ported
            throw new NotImplementedException($"SecurityInfo parsing for OID {protocolOid} not yet implemented");
        }
    }
}
