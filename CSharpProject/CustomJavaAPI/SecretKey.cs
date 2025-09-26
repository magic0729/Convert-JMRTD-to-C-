using System.Security.Cryptography;

namespace org.jmrtd.CustomJavaAPI
{
	// Minimal SecretKey shim to represent a symmetric key as in Javax Crypto.
	public sealed class SecretKey
	{
		public byte[] KeyBytes { get; }
		public string Algorithm { get; }

		public SecretKey(byte[] keyBytes, string algorithm)
		{
			KeyBytes = keyBytes;
			Algorithm = algorithm;
		}

		public SymmetricAlgorithm ToAlgorithm()
		{
			// Best-effort mapping based on algorithm name.
			return Algorithm.ToUpperInvariant() switch
			{
				"AES" => Aes.Create(),
				"DESEDE" or "TRIPLEDES" => TripleDES.Create(),
				_ => Aes.Create(),
			};
		}

		public byte[] GetEncoded() => KeyBytes;
	}
}


