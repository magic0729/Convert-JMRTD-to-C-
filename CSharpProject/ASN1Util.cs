using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Formats.Asn1;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace org.jmrtd
{
    public static class ASN1Util
    {
        public static object ReadASN1Object(Stream inputStream)
        {
            try
            {
                var asn1Stream = new Asn1InputStream(inputStream);
                return asn1Stream.ReadObject();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read ASN1 object", ex);
            }
        }

        public static object CheckTag(object asn1Encodable, int tagClass, int tagNo)
        {
            if (asn1Encodable == null)
            {
                throw new ArgumentNullException(nameof(asn1Encodable), "Expected a tagged object. Found null.");
            }
            
            if (asn1Encodable is Asn1TaggedObject taggedObject)
            {
                if (taggedObject.TagNo == tagNo && (int)taggedObject.TagClass == tagClass)
                {
                    return taggedObject.GetExplicitBaseObject();
                }
                throw new ArgumentException($"Expected tag {TagClassToString(tagClass)}[{tagNo}], found {TagClassToString((int)taggedObject.TagClass)}[{taggedObject.TagNo}]");
            }
            
            throw new ArgumentException("Expected a tagged object");
        }

        public static bool IsSequenceOfSequences(object asn1Encodable)
        {
            if (asn1Encodable is Asn1Sequence sequence)
            {
                foreach (var obj in sequence)
                {
                    if (!(obj is Asn1Sequence))
                    {
                        return false;
                    }
                }
                return sequence.Count > 0;
            }
            return false;
        }

        public static Dictionary<int, object> DecodeTaggedObjects(object asn1Encodable)
        {
            var taggedObjects = new Dictionary<int, object>();
            if (asn1Encodable == null) return taggedObjects;
            
            if (asn1Encodable is Asn1Set set)
            {
                foreach (var obj in set)
                {
                    if (obj is Asn1TaggedObject tagged)
                    {
                        taggedObjects[tagged.TagNo] = tagged.GetExplicitBaseObject();
                    }
                }
            }
            else if (asn1Encodable is Asn1Sequence sequence)
            {
                foreach (var obj in sequence)
                {
                    if (obj is Asn1TaggedObject tagged)
                    {
                        taggedObjects[tagged.TagNo] = tagged.GetExplicitBaseObject();
                    }
                }
            }
            
            return taggedObjects;
        }

        public static List<object> List(object asn1Encodable)
        {
            if (asn1Encodable == null) return new List<object>();
            
            if (asn1Encodable is Asn1Sequence sequence)
            {
                var list = new List<object>();
                foreach (var obj in sequence)
                {
                    list.Add(obj);
                }
                return list;
            }
            
            return new List<object> { asn1Encodable };
        }

        public static int DecodeInt(object asn1Encodable)
        {
            var bigInteger = DecodeBigInteger(asn1Encodable);
            if (bigInteger == null)
            {
                throw new FormatException("Could not parse integer");
            }
            return (int)bigInteger.Value;
        }

        public static BigInteger? DecodeBigInteger(object asn1Encodable)
        {
            if (asn1Encodable is DerInteger derInt)
            {
                return new BigInteger(derInt.Value.ToByteArray());
            }
            return null;
        }

        public static bool DecodeBoolean(object asn1Encodable)
        {
            if (asn1Encodable is DerBoolean derBool)
            {
                return derBool.IsTrue;
            }
            return false;
        }

        public static object EncodeBoolean(bool b)
        {
            return new DerBoolean(b ? new byte[] { 0xFF } : new byte[] { 0x00 });
        }

        public static object EncodeInt(int n)
        {
            return EncodeBigInteger(new BigInteger(n));
        }

        public static object EncodeBigInteger(BigInteger n)
        {
            return new DerInteger(new Org.BouncyCastle.Math.BigInteger(n.ToByteArray()));
        }

        public static object EncodeTaggedObjects(Dictionary<int, object> taggedObjects)
        {
            var vector = new Asn1EncodableVector();
            foreach (var kvp in taggedObjects)
            {
                var tagged = new DerTaggedObject(kvp.Key, (Asn1Encodable)kvp.Value);
                vector.Add(tagged);
            }
            return new DerSet(vector);
        }

        private static string TagClassToString(int tagClass)
        {
            return tagClass switch
            {
                64 => "APPLICATION",
                0 => "UNIVERSAL",
                128 => "CONTEXT_SPECIFIC",
                192 => "PRIVATE",
                _ => tagClass.ToString()
            };
        }
    }
}
