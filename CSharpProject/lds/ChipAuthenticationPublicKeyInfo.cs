using System;
using System.Numerics;
using System.Security.Cryptography;

namespace org.jmrtd.lds
{
    public class ChipAuthenticationPublicKeyInfo : SecurityInfo
    {
        private readonly string protocolOID;
        private readonly System.Security.Cryptography.RSA publicKey;
        private readonly BigInteger? keyId;

        public ChipAuthenticationPublicKeyInfo(string oid, System.Security.Cryptography.RSA publicKey)
            : this(oid, publicKey, null)
        {
        }

        public ChipAuthenticationPublicKeyInfo(string oid, System.Security.Cryptography.RSA publicKey, BigInteger? keyId)
        {
            if (!CheckRequiredIdentifier(oid))
            {
                throw new ArgumentException("Invalid OID");
            }
            this.protocolOID = oid;
            this.publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
            this.keyId = keyId;
        }

        public static bool CheckRequiredIdentifier(string oid)
        {
            return oid switch
            {
                SecurityInfo.ID_PK_DH or
                SecurityInfo.ID_PK_ECDH => true,
                _ => false
            };
        }

        public override string GetObjectIdentifier() => protocolOID;

        public override string GetProtocolOIDString() => ToProtocolOIDString(protocolOID);

        public System.Security.Cryptography.RSA GetPublicKey() => publicKey;

        public BigInteger? GetKeyId() => keyId;

        [Obsolete("This method is deprecated.")]
        public override object GetDERObject()
        {
            // TODO: Implement ASN1 encoding when ASN1 support is added
            throw new NotImplementedException("ASN1 encoding not yet implemented");
        }

        public override string ToString()
        {
            return $"ChipAuthenticationPublicKeyInfo [protocol: {ToProtocolOIDString(protocolOID)}, keyId: {keyId}, publicKey: {publicKey}]";
        }

        public override int GetHashCode()
        {
            return 1234567891 + 7 * protocolOID.GetHashCode() + 5 * publicKey.GetHashCode() + 3 * (keyId?.GetHashCode() ?? 1991);
        }

        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;

            var otherCAPubKeyInfo = (ChipAuthenticationPublicKeyInfo)other;
            return protocolOID.Equals(otherCAPubKeyInfo.protocolOID) &&
                   publicKey.Equals(otherCAPubKeyInfo.publicKey) &&
                   (keyId?.Equals(otherCAPubKeyInfo.keyId) ?? otherCAPubKeyInfo.keyId == null);
        }

        private string ToProtocolOIDString(string oid)
        {
            return oid switch
            {
                SecurityInfo.ID_PK_DH => "id-PK-DH",
                SecurityInfo.ID_PK_ECDH => "id-PK-ECDH",
                _ => oid
            };
        }
    }
}
