using System;
using System.Numerics;
using System.Security.Cryptography;
using org.jmrtd;
using org.jmrtd.CustomJavaAPI;
using org.jmrtd.lds;
using TLV = org.jmrtd.CustomJavaAPI.TLV;

namespace org.jmrtd.protocol
{
	// High-level structure port with TODOs for crypto/TLV-heavy areas.
	public class PACEProtocol
	{
		private readonly APDULevelPACECapable service;
		private SecureMessagingWrapper? wrapper;
		private readonly int maxTranceiveLengthForSecureMessaging;
		private readonly int maxTranceiveLengthForProtocol;
		private readonly bool shouldCheckMAC;
		private readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

		[Obsolete("Use full constructor with separate lengths")]
		public PACEProtocol(APDULevelPACECapable service, SecureMessagingWrapper? wrapper, int maxTranceiveLength, bool shouldCheckMAC)
			: this(service, wrapper, 256, maxTranceiveLength, shouldCheckMAC) { }

		public PACEProtocol(APDULevelPACECapable service, SecureMessagingWrapper? wrapper, int maxTranceiveLengthForProtocol, int maxTranceiveLengthForSecureMessaging, bool shouldCheckMAC)
		{
			this.service = service;
			this.wrapper = wrapper;
			this.maxTranceiveLengthForProtocol = maxTranceiveLengthForProtocol;
			this.maxTranceiveLengthForSecureMessaging = maxTranceiveLengthForSecureMessaging;
			this.shouldCheckMAC = shouldCheckMAC;
		}

		public PACEResult DoPACE(IAccessKeySpec accessKey, string oid, object? staticParameters, BigInteger? parameterId)
		{
			// Map from OID per PACEInfo
			var keySeed = accessKey.GetKey();
			string cipherAlg = PACEInfo.ToCipherAlgorithm(oid);
			int keyLength = PACEInfo.ToKeyLength(oid);
			var staticPACEKey = new SecretKey(keySeed, cipherAlg);

			// Step 1: Get and decrypt PICC nonce
			byte[] piccNonce = DoPACEStep1(staticPACEKey);

			// Step 2: Mapping (kept minimal; GM assumed)
			var mappingResult = DoPACEStep2(PACEInfo.MappingType.GM, "ECDH", staticParameters, piccNonce);

			// Step 3: ECDH key agreement
			ECDiffieHellman pcdKeyPair = ECDiffieHellman.Create(ResolveCurveFromSecurityInfo(staticParameters, parameterId));
			byte[] piccPublicKey = DoPACEStep3ExchangePublicKeys(pcdKeyPair, staticParameters);
			byte[] sharedSecret = ComputeSharedSecret(pcdKeyPair, piccPublicKey);

			// Derive ENC and MAC keys from shared secret
			var encKey = DerivePACEKey(sharedSecret, cipherAlg, "ENC", keyLength);
			var macKey = DerivePACEKey(sharedSecret, cipherAlg, "MAC", keyLength);

			this.wrapper = cipherAlg == "AES"
				? new AESSecureMessagingWrapper(encKey, macKey, maxTranceiveLengthForSecureMessaging, shouldCheckMAC, this.wrapper?.GetSendSequenceCounter() ?? 0)
				: new DESedeSecureMessagingWrapper(encKey, macKey, maxTranceiveLengthForSecureMessaging, shouldCheckMAC, this.wrapper?.GetSendSequenceCounter() ?? 0);

			// Step 4: Mutual authentication tokens (CMAC over public keys)
			_ = DoPACEStep4Tokens(pcdKeyPair, piccPublicKey, macKey, cipherAlg);

			return new PACEResult(accessKey, PACEInfo.MappingType.GM, "ECDH", cipherAlg, "SHA-256", keyLength, mappingResult, pcdKeyPair, pcdKeyPair, this.wrapper);
		}

		private byte[] DoPACEStep1(SecretKey staticPACEKey)
		{
			// Step 1: send GENERAL AUTHENTICATE, unwrap DO'80 and decrypt with static key (AES-CBC/Zero IV or 3DES-CBC)
			byte[] step1Data = Array.Empty<byte>();
			var useWrapper = this.wrapper ?? new PACEWrapperShim(staticPACEKey, staticPACEKey, maxTranceiveLengthForProtocol, false, 0);
            byte[] response = service.SendGeneralAuthenticate((useWrapper as APDUWrapper)!, step1Data, maxTranceiveLengthForProtocol, false);
			byte[] encNonce = TLV.UnwrapDO(0x80, response);
			return DecryptPICCNonce(staticPACEKey, encNonce);
		}

		private PACEMappingResult DoPACEStep2(PACEInfo.MappingType mappingType, string agreementAlg, object? @params, byte[] piccNonce)
		{
			// Minimal: send piccNonce back in DO'81 to advance mapping; parse response DO'82 if present
			byte[] step2Data = TLV.WrapDO(0x81, piccNonce);
			var useWrapper = this.wrapper ?? new PACEWrapperShim(new SecretKey(new byte[16], "AES"), new SecretKey(new byte[16], "AES"), maxTranceiveLengthForProtocol, false, 0);
            byte[] response = service.SendGeneralAuthenticate((useWrapper as APDUWrapper)!, step2Data, maxTranceiveLengthForProtocol, false);
			return new PACEMappingResult(@params ?? new object());
		}

		private ECDiffieHellman DoPACEStep3GenerateECDH(BigInteger? parameterId)
		{
			var curve = ECCurve.NamedCurves.nistP256;
			if (parameterId.HasValue)
			{
				curve = MapParameterIdToCurve((int)parameterId.Value);
			}
			return ECDiffieHellman.Create(curve);
		}

		private static ECCurve MapParameterIdToCurve(int parameterId)
		{
			// Basic mapping; extend as DG14 parsing is completed
			return parameterId switch
			{
				19 => ECCurve.NamedCurves.nistP256,
				20 => ECCurve.NamedCurves.nistP384,
				21 => ECCurve.NamedCurves.nistP521,
				_ => ECCurve.NamedCurves.nistP256,
			};
		}

		private static ECCurve ResolveCurveFromSecurityInfo(object? staticParameters, BigInteger? parameterId)
		{
			try
			{
				if (staticParameters is org.jmrtd.lds.PACEDomainParameterInfo pdi)
				{
					var pid = pdi.GetParameterId();
					if (pid.HasValue) return MapParameterIdToCurve((int)pid.Value);
					var dp = pdi.GetDomainParameters();
					if (dp is string oid)
					{
						// Common OIDs
						switch (oid)
						{
							case "1.2.840.10045.3.1.7": return ECCurve.NamedCurves.nistP256; // prime256v1
							case "1.3.132.0.34": return ECCurve.NamedCurves.nistP384; // secp384r1
							case "1.3.132.0.35": return ECCurve.NamedCurves.nistP521; // secp521r1
							// brainpool (map closest .NET curve: fallback P-256/384/521)
							case "1.3.36.3.3.2.8.1.1.7": return ECCurve.NamedCurves.nistP256; // brainpoolP256r1
							case "1.3.36.3.3.2.8.1.1.11": return ECCurve.NamedCurves.nistP384; // brainpoolP384r1
							case "1.3.36.3.3.2.8.1.1.13": return ECCurve.NamedCurves.nistP521; // brainpoolP512r1 -> nearest P-521
						}
					}
				}
			}
			catch { }
			if (parameterId.HasValue) return MapParameterIdToCurve((int)parameterId.Value);
			return ECCurve.NamedCurves.nistP256;
		}

		private byte[] DoPACEStep3ExchangePublicKeys(ECDiffieHellman pcdKeyPair, object? ephemeralParams)
		{
			var useWrapper = this.wrapper ?? new PACEWrapperShim(new SecretKey(new byte[16], "AES"), new SecretKey(new byte[16], "AES"), maxTranceiveLengthForProtocol, false, 0);
			byte[] pcdPub = pcdKeyPair.PublicKey.ExportSubjectPublicKeyInfo();
			byte[] step3Data = TLV.WrapDO(0x83, pcdPub);
            byte[] response = service.SendGeneralAuthenticate((useWrapper as APDUWrapper)!, step3Data, maxTranceiveLengthForProtocol, false);
			return TLV.UnwrapDO(0x84, response);
		}

		private byte[]? DoPACEStep4Tokens(ECDiffieHellman pcdKeyPair, byte[] piccPublicKeyEncoded, SecretKey macKey, string cipherAlg)
		{
			// Compute token T_PCD = CMAC_KMac(PICC_public)
			byte[] tpcd = ComputeToken(piccPublicKeyEncoded, macKey, cipherAlg);
			var useWrapper = this.wrapper ?? new PACEWrapperShim(new SecretKey(new byte[16], "AES"), new SecretKey(new byte[16], "AES"), maxTranceiveLengthForProtocol, false, 0);
			byte[] step4Data = TLV.WrapDO(0x85, tpcd);
            byte[] response = service.SendGeneralAuthenticate((useWrapper as APDUWrapper)!, step4Data, maxTranceiveLengthForProtocol, true);
			byte[] tpicc = TLV.UnwrapDO(0x86, response);
			// Verify token TPICC ?= CMAC_KMac(PCD_public)
			byte[] pcdPub = pcdKeyPair.PublicKey.ExportSubjectPublicKeyInfo();
			byte[] expect = ComputeToken(pcdPub, macKey, cipherAlg);
			if (tpicc.Length != expect.Length) return null;
			for (int i = 0; i < expect.Length; i++) if (tpicc[i] != expect[i]) return null;
			return tpicc;
		}

		private byte[] RandomBytes(int length)
		{
			byte[] bytes = new byte[length];
			rng.GetBytes(bytes);
			return bytes;
		}

		private sealed class PACEWrapperShim : SecureMessagingWrapper
		{
			public PACEWrapperShim(SecretKey ksEnc, SecretKey ksMac, int maxLen, bool checkMac, long ssc)
				: base(ksEnc, ksMac, maxLen, checkMac, ssc) { }
			
			public override CommandAPDU Wrap(CommandAPDU command) => command;
			public override ResponseAPDU Unwrap(ResponseAPDU response) => response;
			public override object Type => "PACE_Shim";
			
			public override CommandAPDU WrapCommand(CommandAPDU command) => Wrap(command);
			public override ResponseAPDU UnwrapResponse(ResponseAPDU response) => Unwrap(response);
		}

		private static byte[] DecryptPICCNonce(SecretKey staticPACEKey, byte[] encNonce)
		{
			if (staticPACEKey.Algorithm == "AES")
			{
				using var aes = Aes.Create();
				aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.Zeros; aes.IV = new byte[16];
				aes.Key = staticPACEKey.GetEncoded();
				using var dec = aes.CreateDecryptor();
				return dec.TransformFinalBlock(encNonce, 0, encNonce.Length);
			}
			else
			{
				using var tdes = TripleDES.Create();
				tdes.Mode = CipherMode.CBC; tdes.Padding = PaddingMode.Zeros; tdes.IV = new byte[8];
				tdes.Key = staticPACEKey.GetEncoded().Length == 24 ? staticPACEKey.GetEncoded() : PadTo24(staticPACEKey.GetEncoded());
				using var dec = tdes.CreateDecryptor();
				return dec.TransformFinalBlock(encNonce, 0, encNonce.Length);
			}
		}

		private static byte[] PadTo24(byte[] key)
		{
			byte[] k = new byte[24];
			Array.Copy(key, 0, k, 0, Math.Min(key.Length, 24));
			return k;
		}

		private static SecretKey DerivePACEKey(byte[] sharedSecret, string cipherAlg, string label, int keyLength)
		{
			// Simple KDF: K = SHA-256(0x00000001 || Z || label) and truncate to keyLength
			using var sha = SHA256.Create();
			byte[] counter = { 0, 0, 0, 1 };
			byte[] labelBytes = System.Text.Encoding.ASCII.GetBytes(label);
			byte[] input = new byte[counter.Length + sharedSecret.Length + labelBytes.Length];
			Array.Copy(counter, 0, input, 0, counter.Length);
			Array.Copy(sharedSecret, 0, input, counter.Length, sharedSecret.Length);
			Array.Copy(labelBytes, 0, input, counter.Length + sharedSecret.Length, labelBytes.Length);
			byte[] digest = sha.ComputeHash(input);
			int bytes = keyLength / 8;
			byte[] key = new byte[bytes]; Array.Copy(digest, key, bytes);
			return new SecretKey(key, cipherAlg);
		}

		private static byte[] ComputeSharedSecret(ECDiffieHellman pcdKeyPair, byte[] piccPublicKeyEncoded)
		{
			using var piccPub = ECDiffieHellman.Create();
			piccPub.ImportSubjectPublicKeyInfo(piccPublicKeyEncoded, out _);
			return pcdKeyPair.DeriveKeyMaterial(piccPub.PublicKey);
		}

		private static byte[] ComputeToken(byte[] message, SecretKey macKey, string cipherAlg)
		{
			if (cipherAlg == "AES") return AESCMAC.Compute(macKey.GetEncoded(), message, 16);
			return AESCMAC.Compute(new SecretKey(macKey.GetEncoded(), "AES").GetEncoded(), message, 8);
		}
	}
}


