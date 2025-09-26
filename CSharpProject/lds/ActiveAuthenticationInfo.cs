using System;
using System.Numerics;

namespace org.jmrtd.lds
{
    public class ActiveAuthenticationInfo : SecurityInfo
    {
        private readonly string protocolOID;
        private readonly int version;
        private readonly string? signatureAlgorithmOID;

        public ActiveAuthenticationInfo(string oid, int version, string? signatureAlgorithmOID)
        {
            if (!CheckRequiredIdentifier(oid))
            {
                throw new ArgumentException("Invalid OID");
            }
            this.protocolOID = oid;
            this.version = version;
            this.signatureAlgorithmOID = signatureAlgorithmOID;
        }

        public static bool CheckRequiredIdentifier(string oid)
        {
            return SecurityInfo.ID_AA.Equals(oid);
        }

        public override string GetObjectIdentifier() => protocolOID;

        public override string GetProtocolOIDString() => ToProtocolOIDString(protocolOID);

        public int GetVersion() => version;

        public string? GetSignatureAlgorithmOID() => signatureAlgorithmOID;

        [Obsolete("This method is deprecated.")]
        public override object GetDERObject()
        {
            // TODO: Implement ASN1 encoding when ASN1 support is added
            throw new NotImplementedException("ASN1 encoding not yet implemented");
        }

        public override string ToString()
        {
            return $"ActiveAuthenticationInfo [protocol: {ToProtocolOIDString(protocolOID)}, version: {version}, signatureAlgorithm: {signatureAlgorithmOID}]";
        }

        public override int GetHashCode()
        {
            return 1234567891 + 7 * protocolOID.GetHashCode() + 5 * version + 3 * (signatureAlgorithmOID?.GetHashCode() ?? 1991);
        }

        public override bool Equals(object? other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;

            var otherAAInfo = (ActiveAuthenticationInfo)other;
            return protocolOID.Equals(otherAAInfo.protocolOID) &&
                   version == otherAAInfo.version &&
                   (signatureAlgorithmOID?.Equals(otherAAInfo.signatureAlgorithmOID) ?? otherAAInfo.signatureAlgorithmOID == null);
        }

        private string ToProtocolOIDString(string oid)
        {
            return oid switch
            {
                SecurityInfo.ID_AA => "id-AA",
                _ => oid
            };
        }
    }
}
