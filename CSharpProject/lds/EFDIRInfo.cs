using System;
using System.Linq;

namespace org.jmrtd.lds
{
    public class EFDIRInfo : SecurityInfo
    {
        private const string EF_DIR_PROTOCOL_OID = "2.23.136.1.1.13";
        private readonly byte[] efDIR;

        public EFDIRInfo(byte[] efDIR)
        {
            if (efDIR == null)
            {
                throw new ArgumentException("Cannot create EFDIRInfo for null");
            }
            this.efDIR = new byte[efDIR.Length];
            Array.Copy(efDIR, this.efDIR, efDIR.Length);
        }

        public byte[] GetEFDIR()
        {
            var result = new byte[efDIR.Length];
            Array.Copy(efDIR, result, efDIR.Length);
            return result;
        }

        [Obsolete("This method is deprecated.")]
        public override object GetDERObject()
        {
            // TODO: Implement ASN1 encoding when ASN1 support is added
            throw new NotImplementedException("ASN1 encoding not yet implemented");
        }

        public override string GetObjectIdentifier() => EF_DIR_PROTOCOL_OID;

        public override string GetProtocolOIDString() => "id-EFDIR";
    }
}

