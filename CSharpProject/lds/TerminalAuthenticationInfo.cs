using System;

namespace org.jmrtd.lds
{
    public class TerminalAuthenticationInfo : SecurityInfo
    {
        private readonly string protocolOID;
        private readonly int version;
        private readonly object? efCVCA; // ASN1Sequence placeholder

        public TerminalAuthenticationInfo(string oid, int version)
            : this(oid, version, null)
        {
        }

        public TerminalAuthenticationInfo(string oid, int version, object? efCVCA)
        {
            if (!CheckRequiredIdentifier(oid))
            {
                throw new ArgumentException("Invalid OID");
            }
            this.protocolOID = oid;
            this.version = version;
            this.efCVCA = efCVCA;
        }

        public static bool CheckRequiredIdentifier(string oid)
        {
            return oid switch
            {
                SecurityInfo.ID_TA or
                SecurityInfo.ID_TA_RSA or
                SecurityInfo.ID_TA_RSA_V1_5_SHA_1 or
                SecurityInfo.ID_TA_RSA_V1_5_SHA_256 or
                SecurityInfo.ID_TA_RSA_PSS_SHA_1 or
                SecurityInfo.ID_TA_RSA_PSS_SHA_256 or
                SecurityInfo.ID_TA_ECDSA or
                SecurityInfo.ID_TA_ECDSA_SHA_1 or
                SecurityInfo.ID_TA_ECDSA_SHA_224 or
                SecurityInfo.ID_TA_ECDSA_SHA_256 => true,
                _ => false
            };
        }

        public override string GetObjectIdentifier() => protocolOID;

        public override string GetProtocolOIDString() => ToProtocolOIDString(protocolOID);

        public int GetVersion() => version;

        public object? GetEfCVCA() => efCVCA;

        [Obsolete("This method is deprecated.")]
        public override object GetDERObject()
        {
            // TODO: Implement ASN1 encoding when ASN1 support is added
            throw new NotImplementedException("ASN1 encoding not yet implemented");
        }

        public override string ToString()
        {
            return $"TerminalAuthenticationInfo [protocol: {ToProtocolOIDString(protocolOID)}, version: {version}, efCVCA: {efCVCA}]";
        }

        public override int GetHashCode()
        {
            return 1234567891 + 7 * protocolOID.GetHashCode() + 5 * version + 3 * (efCVCA?.GetHashCode() ?? 1991);
        }

        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;

            var otherTAInfo = (TerminalAuthenticationInfo)other;
            return protocolOID.Equals(otherTAInfo.protocolOID) &&
                   version == otherTAInfo.version &&
                   (efCVCA?.Equals(otherTAInfo.efCVCA) ?? otherTAInfo.efCVCA == null);
        }

        private string ToProtocolOIDString(string oid)
        {
            return oid switch
            {
                SecurityInfo.ID_TA => "id-TA",
                SecurityInfo.ID_TA_RSA => "id-TA-RSA",
                SecurityInfo.ID_TA_RSA_V1_5_SHA_1 => "id-TA-RSA-v1.5-SHA-1",
                SecurityInfo.ID_TA_RSA_V1_5_SHA_256 => "id-TA-RSA-v1.5-SHA-256",
                SecurityInfo.ID_TA_RSA_PSS_SHA_1 => "id-TA-RSA-PSS-SHA-1",
                SecurityInfo.ID_TA_RSA_PSS_SHA_256 => "id-TA-RSA-PSS-SHA-256",
                SecurityInfo.ID_TA_ECDSA => "id-TA-ECDSA",
                SecurityInfo.ID_TA_ECDSA_SHA_1 => "id-TA-ECDSA-SHA-1",
                SecurityInfo.ID_TA_ECDSA_SHA_224 => "id-TA-ECDSA-SHA-224",
                SecurityInfo.ID_TA_ECDSA_SHA_256 => "id-TA-ECDSA-SHA-256",
                _ => oid
            };
        }
    }
}
