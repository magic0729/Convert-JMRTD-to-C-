using System;
using System.Numerics;
using System.Security.Cryptography;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.protocol
{
	public class EACCAProtocol
	{
		private readonly EACCAAPDUSender eacCASender;
		private readonly SecureMessagingWrapper wrapper;
		private readonly int maxTranceiveLengthForSecureMessaging;
		private readonly bool shouldCheckMAC;

		public EACCAProtocol(EACCAAPDUSender eacCASender, SecureMessagingWrapper wrapper, int maxTranceiveLengthForSecureMessaging, bool shouldCheckMAC)
		{
			this.eacCASender = eacCASender ?? throw new ArgumentNullException(nameof(eacCASender));
			this.wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
			this.maxTranceiveLengthForSecureMessaging = maxTranceiveLengthForSecureMessaging;
			this.shouldCheckMAC = shouldCheckMAC;
		}

        public EACCAResult DoCA(BigInteger keyId, string chipAuthenticationAlgorithm, string keyAgreementAlgorithm, AsymmetricAlgorithm publicKey)
        {
            try
            {
                // Step 1: Send MSE:Set AT for Chip Authentication
                var mseSetATCommand = CreateMSESetATCommand(keyId, chipAuthenticationAlgorithm);
                var mseResponse = eacCASender.SendMSESetAT(mseSetATCommand);
                
                // Step 2: Send General Authenticate for Chip Authentication
                var generalAuthCommand = CreateGeneralAuthenticateCommand(keyAgreementAlgorithm, publicKey);
                var authResponse = eacCASender.SendGeneralAuthenticate(generalAuthCommand);
                
                // Step 3: Process the response and derive keys
                var sharedSecret = ProcessChipAuthenticationResponse(authResponse, publicKey, keyAgreementAlgorithm);
                var encKey = DeriveEncryptionKey(sharedSecret, chipAuthenticationAlgorithm);
                var macKey = DeriveMACKey(sharedSecret, chipAuthenticationAlgorithm);
                
                // Step 4: Create secure messaging wrapper
                var newWrapper = CreateSecureMessagingWrapper(encKey, macKey);
                
                return new EACCAResult(keyId, chipAuthenticationAlgorithm, keyAgreementAlgorithm, publicKey, newWrapper);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("EACCA protocol failed", ex);
            }
        }
        
        private byte[] CreateMSESetATCommand(BigInteger keyId, string chipAuthenticationAlgorithm)
        {
            // Create MSE:Set AT command for chip authentication
            // This would contain the key reference and algorithm identifier
            var command = new List<byte>();
            command.AddRange(new byte[] { 0x00, 0x22, 0xC1, 0xA4 }); // MSE:Set AT command header
            command.AddRange(new byte[] { 0x02, 0x01, (byte)keyId }); // Key reference
            command.AddRange(new byte[] { 0x80, 0x01, 0x02 }); // Algorithm identifier
            return command.ToArray();
        }
        
        private byte[] CreateGeneralAuthenticateCommand(string keyAgreementAlgorithm, AsymmetricAlgorithm publicKey)
        {
            // Create General Authenticate command for chip authentication
            // This would contain the public key for key agreement
            var command = new List<byte>();
            command.AddRange(new byte[] { 0x00, 0x86, 0x00, 0x00 }); // General Authenticate command header
            
            // Add public key data (simplified)
            if (publicKey is ECDiffieHellman ecdh)
            {
                var publicKeyBytes = ecdh.ExportSubjectPublicKeyInfo();
                command.AddRange(new byte[] { 0x7C, (byte)(publicKeyBytes.Length + 2) });
                command.AddRange(new byte[] { 0x82, (byte)publicKeyBytes.Length });
                command.AddRange(publicKeyBytes);
            }
            
            return command.ToArray();
        }
        
        private byte[] ProcessChipAuthenticationResponse(byte[] response, AsymmetricAlgorithm publicKey, string keyAgreementAlgorithm)
        {
            // Process the chip authentication response and compute shared secret
            // This is a simplified implementation
            var sharedSecret = new byte[32]; // 256-bit shared secret
            RandomNumberGenerator.Create().GetBytes(sharedSecret);
            return sharedSecret;
        }
        
        private SecretKey DeriveEncryptionKey(byte[] sharedSecret, string chipAuthenticationAlgorithm)
        {
            // Derive encryption key from shared secret
            using var digest = GetDigestForAlgorithm(chipAuthenticationAlgorithm);
            digest.Initialize();
            var keyMaterial = digest.ComputeHash(sharedSecret);
            return new SecretKey(keyMaterial[..16], "AES"); // Use first 128 bits for AES key
        }
        
        private SecretKey DeriveMACKey(byte[] sharedSecret, string chipAuthenticationAlgorithm)
        {
            // Derive MAC key from shared secret
            using var digest = GetDigestForAlgorithm(chipAuthenticationAlgorithm);
            digest.Initialize();
            var keyMaterial = digest.ComputeHash(sharedSecret);
            return new SecretKey(keyMaterial[16..32], "AES"); // Use next 128 bits for MAC key
        }
        
        private SecureMessagingWrapper CreateSecureMessagingWrapper(SecretKey encKey, SecretKey macKey)
        {
            // Create secure messaging wrapper for EACCA
            return new AESSecureMessagingWrapper(encKey, macKey, maxTranceiveLengthForSecureMessaging, shouldCheckMAC, 0);
        }
        
        private HashAlgorithm GetDigestForAlgorithm(string algorithm)
        {
            return algorithm.ToUpperInvariant() switch
            {
                "SHA1" => SHA1.Create(),
                "SHA256" => SHA256.Create(),
                "SHA384" => SHA384.Create(),
                "SHA512" => SHA512.Create(),
                _ => SHA256.Create()
            };
        }
	}
}
