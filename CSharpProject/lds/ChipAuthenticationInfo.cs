using System;
using System.Numerics;
using System.Security.Cryptography;

namespace org.jmrtd.lds
{
    public class ChipAuthenticationInfo : SecurityInfo
    {
        private readonly string protocolOID;
        private readonly int version;
        private readonly BigInteger? keyId;

        public ChipAuthenticationInfo(string oid, int version)
            : this(oid, version, null)
        {
        }

        public ChipAuthenticationInfo(string oid, int version, BigInteger? keyId)
        {
            if (!CheckRequiredIdentifier(oid))
            {
                throw new ArgumentException("Invalid OID");
            }
            this.protocolOID = oid;
            this.version = version;
            this.keyId = keyId;
        }

        public static bool CheckRequiredIdentifier(string oid)
        {
            return oid switch
            {
                SecurityInfo.ID_CA_DH_3DES_CBC_CBC or
                SecurityInfo.ID_CA_ECDH_3DES_CBC_CBC or
                SecurityInfo.ID_CA_DH_AES_CBC_CMAC_128 or
                SecurityInfo.ID_CA_DH_AES_CBC_CMAC_192 or
                SecurityInfo.ID_CA_DH_AES_CBC_CMAC_256 or
                SecurityInfo.ID_CA_ECDH_AES_CBC_CMAC_128 or
                SecurityInfo.ID_CA_ECDH_AES_CBC_CMAC_192 or
                SecurityInfo.ID_CA_ECDH_AES_CBC_CMAC_256 => true,
                _ => false
            };
        }

        public override string GetObjectIdentifier() => protocolOID;

        public override string GetProtocolOIDString() => ToProtocolOIDString(protocolOID);

        public int GetVersion() => version;

        public BigInteger? GetKeyId() => keyId;

        [Obsolete("This method is deprecated.")]
        public override object GetDERObject()
        {
            // TODO: Implement ASN1 encoding when ASN1 support is added
            throw new NotImplementedException("ASN1 encoding not yet implemented");
        }

        public override string ToString()
        {
            return $"ChipAuthenticationInfo [protocol: {ToProtocolOIDString(protocolOID)}, version: {version}, keyId: {keyId}]";
        }

        public override int GetHashCode()
        {
            return 1234567891 + 7 * protocolOID.GetHashCode() + 5 * version + 3 * (keyId?.GetHashCode() ?? 1991);
        }

        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;

            var otherCAInfo = (ChipAuthenticationInfo)other;
            return protocolOID.Equals(otherCAInfo.protocolOID) &&
                   version == otherCAInfo.version &&
                   (keyId?.Equals(otherCAInfo.keyId) ?? otherCAInfo.keyId == null);
        }

        private string ToProtocolOIDString(string oid)
        {
            return oid switch
            {
                SecurityInfo.ID_CA_DH_3DES_CBC_CBC => "id-CA-DH-3DES-CBC-CBC",
                SecurityInfo.ID_CA_ECDH_3DES_CBC_CBC => "id-CA-ECDH-3DES-CBC-CBC",
                SecurityInfo.ID_CA_DH_AES_CBC_CMAC_128 => "id-CA-DH-AES-CBC-CMAC-128",
                SecurityInfo.ID_CA_DH_AES_CBC_CMAC_192 => "id-CA-DH-AES-CBC-CMAC-192",
                SecurityInfo.ID_CA_DH_AES_CBC_CMAC_256 => "id-CA-DH-AES-CBC-CMAC-256",
                SecurityInfo.ID_CA_ECDH_AES_CBC_CMAC_128 => "id-CA-ECDH-AES-CBC-CMAC-128",
                SecurityInfo.ID_CA_ECDH_AES_CBC_CMAC_192 => "id-CA-ECDH-AES-CBC-CMAC-192",
                SecurityInfo.ID_CA_ECDH_AES_CBC_CMAC_256 => "id-CA-ECDH-AES-CBC-CMAC-256",
                _ => oid
            };
        }
    }
}
