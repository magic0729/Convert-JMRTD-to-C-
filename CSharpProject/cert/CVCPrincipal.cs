using System;

namespace org.jmrtd.cert
{
    public class CVCPrincipal
    {
        private readonly string country;
        private readonly string mnemonic;
        private readonly int seqNumber;

        public CVCPrincipal(string country, string mnemonic, int seqNumber)
        {
            this.country = country ?? throw new ArgumentNullException(nameof(country));
            this.mnemonic = mnemonic ?? throw new ArgumentNullException(nameof(mnemonic));
            this.seqNumber = seqNumber;
        }

        public string GetCountry() => country;
        public string GetMnemonic() => mnemonic;
        public int GetSeqNumber() => seqNumber;

        public override string ToString()
        {
            return $"CVCPrincipal[{country}:{mnemonic}:{seqNumber}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            var other = (CVCPrincipal)obj;
            return country.Equals(other.country) && mnemonic.Equals(other.mnemonic) && seqNumber == other.seqNumber;
        }

        public override int GetHashCode()
        {
            return country.GetHashCode() ^ mnemonic.GetHashCode() ^ seqNumber.GetHashCode();
        }
    }
}
