using System;
using System.IO;
using System.Security.Cryptography;
using org.jmrtd.lds;

namespace org.jmrtd.lds.icao
{
	public class DG15File : DataGroup
	{
		private AsymmetricAlgorithm publicKey = RSA.Create();

		public DG15File(AsymmetricAlgorithm publicKey) : base(111)
		{
			this.publicKey = publicKey ?? throw new System.ArgumentNullException(nameof(publicKey));
		}

		public DG15File(Stream inputStream) : base(111, inputStream)
		{
		}

		protected override void ReadContent(Stream inputStream)
		{
			// Reads remaining bytes and tries to parse as X509SubjectPublicKeyInfo
			using var ms = new MemoryStream();
			inputStream.CopyTo(ms);
			byte[] value = ms.ToArray();
			publicKey = TryParsePublicKey(value);
		}

		private static AsymmetricAlgorithm TryParsePublicKey(byte[] keyBytes)
		{
			// Try RSA first
			try
			{
				var rsa = RSA.Create();
				rsa.ImportSubjectPublicKeyInfo(new ReadOnlySpan<byte>(keyBytes), out _);
				return rsa;
			}
			catch { }
			// Try ECDsa
			try
			{
				var ecdsa = ECDsa.Create();
				ecdsa.ImportSubjectPublicKeyInfo(new ReadOnlySpan<byte>(keyBytes), out _);
				return ecdsa;
			}
			catch { }
			throw new CryptographicException("Unsupported public key format in DG15");
		}

		protected override void WriteContent(Stream outStream)
		{
			byte[] spki;
			if (publicKey is RSA rsa)
			{
				spki = rsa.ExportSubjectPublicKeyInfo();
			}
			else if (publicKey is ECDsa ecdsa)
			{
				spki = ecdsa.ExportSubjectPublicKeyInfo();
			}
			else
			{
				throw new CryptographicException("Unsupported key type");
			}
			outStream.Write(spki, 0, spki.Length);
		}

		public AsymmetricAlgorithm GetPublicKey() => publicKey;

		public override string ToString()
		{
			string alg = publicKey switch
			{
				RSA _ => "RSA",
				ECDsa _ => "EC",
				_ => publicKey.GetType().Name
			};
			return $"DG15File [{alg}]";
		}
	}
}
