using System;

namespace org.jmrtd.CustomJavaAPI
{
    /// <summary>
    /// TLV (Tag-Length-Value) object for representing structured data
    /// </summary>
    public class TLVObject
    {
        private readonly int tag;
        private readonly byte[] value;

        public TLVObject(int tag, byte[] value)
        {
            this.tag = tag;
            this.value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public int GetTag() => tag;
        public int GetLength() => value.Length;
        public byte[] GetValue() => (byte[])value.Clone();

        public override string ToString()
        {
            return $"TLV[Tag=0x{tag:X4}, Length={value.Length}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            var other = (TLVObject)obj;
            return tag == other.tag && value.Length == other.value.Length && 
                   System.Linq.Enumerable.SequenceEqual(value, other.value);
        }

        public override int GetHashCode()
        {
            int hash = tag.GetHashCode();
            foreach (byte b in value)
            {
                hash = hash * 31 + b.GetHashCode();
            }
            return hash;
        }
    }
}
