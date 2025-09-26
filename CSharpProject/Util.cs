using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using org.jmrtd.CustomJavaAPI;
using org.jmrtd.lds.icao;

namespace org.jmrtd
{
	public static class Util
	{
		public const int ENC_MODE = 1;
		public const int MAC_MODE = 2;
		public const int PACE_MODE = 3;

		public static SecretKey DeriveKey(byte[] keySeed, int mode)
		{
			return DeriveKey(keySeed, "DESede", 128, null, mode, 0);
		}

		public static SecretKey DeriveKey(byte[] keySeed, string cipherAlg, int keyLength, byte[]? nonce, int mode, byte paceKeyReference)
		{
			string digestAlg = InferDigestAlgorithmFromCipherAlgorithmForKeyDerivation(cipherAlg, keyLength);
			using var digest = GetMessageDigest(digestAlg);
			digest.Initialize();
			var data = new MemoryStream();
			data.Write(keySeed);
			if (nonce != null) data.Write(nonce);
			data.Write(new byte[] { 0, 0, 0, (byte)mode });
			var hashResult = digest.ComputeHash(data.ToArray());

			byte[] keyBytes = cipherAlg.ToUpperInvariant().StartsWith("AES")
				? keyLength switch
				{
					128 => hashResult[..16],
					192 => hashResult[..24],
					256 => hashResult[..32],
					_ => throw new ArgumentException("AES key length must be 128/192/256")
				}
				: cipherAlg.Equals("DESede", StringComparison.OrdinalIgnoreCase) || cipherAlg.Equals("3DES", StringComparison.OrdinalIgnoreCase)
					? keyLength switch
					{
						112 or 128 => Combine(hashResult[..8], hashResult[8..16], hashResult[..8]),
						_ => throw new ArgumentException("KDF can only use DESede with 128-bit length")
					}
					: throw new ArgumentException("Unsupported cipher algorithm");

			var algo = cipherAlg.Equals("3DES", StringComparison.OrdinalIgnoreCase) ? "DESede" : cipherAlg;
			return new SecretKey(keyBytes, algo);
		}

		public static byte[] ComputeKeySeed(string documentNumber, string dateOfBirth, string dateOfExpiry, string digestAlg, bool doTruncate)
		{
			string text = documentNumber + MRZInfo.CheckDigit(documentNumber) + dateOfBirth + MRZInfo.CheckDigit(dateOfBirth) + dateOfExpiry + MRZInfo.CheckDigit(dateOfExpiry);
			return ComputeKeySeed(text, digestAlg, doTruncate);
		}

		public static byte[] ComputeKeySeed(string cardAccessNumber, string digestAlg, bool doTruncate)
		{
			using var md = GetMessageDigest(digestAlg);
			var hash = md.ComputeHash(Encoding.UTF8.GetBytes(cardAccessNumber));
			if (!doTruncate) return hash;
			var keySeed = new byte[16];
			Array.Copy(hash, 0, keySeed, 0, 16);
			return keySeed;
		}

		public static byte[] Pad(byte[] input, int blockSize)
		{
			return Pad(input, 0, input.Length, blockSize);
		}

		public static byte[] Pad(byte[] bytes, int offset, int length, int blockSize)
		{
			using var ms = new MemoryStream();
			ms.Write(bytes, offset, length);
			ms.WriteByte(0x80);
			while (ms.Length % blockSize != 0) ms.WriteByte(0x00);
			return ms.ToArray();
		}

		public static byte[] Unpad(byte[] bytes)
		{
			int i = bytes.Length - 1;
			while (i >= 0 && bytes[i] == 0) i--;
			if (i < 0 || (bytes[i] & 0xFF) != 0x80) throw new CryptographicException($"Expected 0x80, found 0x{(bytes[Math.Max(i,0)] & 0xFF):X2}");
			var result = new byte[i];
			Array.Copy(bytes, 0, result, 0, i);
			return result;
		}

		public static byte[] I2Os(BigInteger value, int length)
		{
			var result = new byte[length];
			BigInteger baseVal = new BigInteger(256);
			for (int i = 0; i < length; i++)
			{
				BigInteger remainder = value % baseVal;
				value /= baseVal;
				result[length - 1 - i] = (byte)remainder;
			}
			return result;
		}

		public static byte[] I2Os(BigInteger value)
		{
			int nibbles = value.ToString("x").Length;
			if (nibbles % 2 != 0) nibbles++;
			return I2Os(value, nibbles / 2);
		}

		public static BigInteger Os2I(byte[] bytes) => Os2I(bytes, 0, bytes?.Length ?? 0);

		public static BigInteger Os2I(byte[] bytes, int offset, int length)
		{
			if (bytes == null) throw new ArgumentException();
			BigInteger result = BigInteger.Zero;
			BigInteger baseVal = new BigInteger(256);
			for (int i = offset; i < offset + length; i++)
			{
				result = result * baseVal + (bytes[i] & 0xFF);
			}
			return result;
		}

        public static byte[] StripLeadingZeroes(byte[] bytes)
		{
            if (bytes == null || bytes.Length <= 1) return bytes ?? Array.Empty<byte>();
			int start = 0;
			while (start < bytes.Length && bytes[start] == 0) start++;
			if (start == 0) return bytes;
			var result = new byte[bytes.Length - start];
			Array.Copy(bytes, start, result, 0, result.Length);
			return result;
		}

		public static HashAlgorithm GetMessageDigest(string algorithm)
		{
			return algorithm.ToUpperInvariant() switch
			{
				"SHA-1" or "SHA1" => SHA1.Create(),
				"SHA-224" => SHA256.Create(), // TODO: provide SHA-224 if strictly required
				"SHA-256" or "SHA256" => SHA256.Create(),
				"SHA-384" or "SHA384" => SHA384.Create(),
				"SHA-512" or "SHA512" => SHA512.Create(),
				_ => SHA256.Create(),
			};
		}

		public static string InferDigestAlgorithmFromCipherAlgorithmForKeyDerivation(string cipherAlg, int keyLength)
		{
			if (cipherAlg == null) throw new ArgumentException();
			if (cipherAlg.Equals("DESede", StringComparison.OrdinalIgnoreCase) || cipherAlg.Equals("AES-128", StringComparison.OrdinalIgnoreCase)) return "SHA-1";
			if (cipherAlg.Equals("AES", StringComparison.OrdinalIgnoreCase) && keyLength == 128) return "SHA-1";
			if (cipherAlg.Equals("AES-192", StringComparison.OrdinalIgnoreCase) || cipherAlg.Equals("AES-256", StringComparison.OrdinalIgnoreCase)) return "SHA-256";
			if (cipherAlg.Equals("AES", StringComparison.OrdinalIgnoreCase) && (keyLength == 192 || keyLength == 256)) return "SHA-256";
			throw new ArgumentException($"Unsupported cipher algorithm or key length \"{cipherAlg}\", {keyLength}");
		}

		private static byte[] Combine(byte[] a, byte[] b, byte[] c)
		{
			var result = new byte[a.Length + b.Length + c.Length];
			Buffer.BlockCopy(a, 0, result, 0, a.Length);
			Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
			Buffer.BlockCopy(c, 0, result, a.Length + b.Length, c.Length);
			return result;
		}

		// TODO: Port remaining BouncyCastle and TLV dependent utilities as C# equivalents when needed by callers.
	}
}


