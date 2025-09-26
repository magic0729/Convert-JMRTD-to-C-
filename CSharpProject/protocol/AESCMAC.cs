using System;
using System.Security.Cryptography;

namespace org.jmrtd.protocol
{
	internal static class AESCMAC
	{
		// NIST SP 800-38B CMAC (AES). Minimal implementation for PACE tokens.
		public static byte[] Compute(byte[] key, byte[] message, int macSize)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (message == null) throw new ArgumentNullException(nameof(message));
			using var aes = Aes.Create();
			aes.Mode = CipherMode.ECB; aes.Padding = PaddingMode.None; aes.Key = key.Length >= 16 ? key.AsSpan(0, 16).ToArray() : key;
			byte[] k1, k2;
			GenerateSubkeys(aes, out k1, out k2);
			int blockSize = 16;
			int n = (message.Length + blockSize - 1) / blockSize;
			bool lastComplete = message.Length > 0 && (message.Length % blockSize == 0);
			byte[] lastBlock = new byte[blockSize];
			if (n == 0)
			{
				n = 1; lastComplete = false;
			}
			int lastLen = message.Length - (n - 1) * blockSize;
			if (lastComplete)
			{
				Array.Copy(message, (n - 1) * blockSize, lastBlock, 0, blockSize);
				Xor(lastBlock, k1);
			}
			else
			{
				int idx = (n - 1) * blockSize;
				if (lastLen > 0) Array.Copy(message, idx, lastBlock, 0, lastLen);
				lastBlock[lastLen] = 0x80;
				Xor(lastBlock, k2);
			}
			byte[] x = new byte[blockSize];
			using var enc = aes.CreateEncryptor();
			for (int i = 0; i < n - 1; i++)
			{
				byte[] m = new byte[blockSize];
				Array.Copy(message, i * blockSize, m, 0, blockSize);
				XorInPlace(m, x);
				x = enc.TransformFinalBlock(m, 0, blockSize);
			}
			XorInPlace(lastBlock, x);
			byte[] t = enc.TransformFinalBlock(lastBlock, 0, blockSize);
			if (macSize == blockSize) return t;
			byte[] mac = new byte[macSize];
			Array.Copy(t, 0, mac, 0, macSize);
			return mac;
		}

		private static void GenerateSubkeys(SymmetricAlgorithm aes, out byte[] k1, out byte[] k2)
		{
			byte[] zero = new byte[16];
			using var enc = aes.CreateEncryptor();
			byte[] l = enc.TransformFinalBlock(zero, 0, zero.Length);
			k1 = DoubleLu(l);
			k2 = DoubleLu(k1);
		}

		private static byte[] DoubleLu(byte[] input)
		{
			byte[] r = new byte[input.Length];
			int carry = 0;
			for (int i = input.Length - 1; i >= 0; i--)
			{
				int v = (input[i] << 1) & 0xFF; v |= carry; carry = (input[i] & 0x80) != 0 ? 1 : 0; r[i] = (byte)v;
			}
			if (carry != 0)
			{
				r[^1] ^= 0x87;
			}
			return r;
		}

		private static void Xor(byte[] a, byte[] b)
		{
			for (int i = 0; i < a.Length; i++) a[i] ^= b[i];
		}

		private static void XorInPlace(byte[] a, byte[] x)
		{
			for (int i = 0; i < a.Length; i++) a[i] ^= x[i];
		}
	}
}


