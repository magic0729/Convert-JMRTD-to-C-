using System;
using System.Security.Cryptography;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.protocol
{
	public class AAProtocol
	{
		private readonly AAAPDUSender aaSender;
		private readonly SecureMessagingWrapper wrapper;

		public AAProtocol(AAAPDUSender aaSender, SecureMessagingWrapper wrapper)
		{
			this.aaSender = aaSender ?? throw new ArgumentNullException(nameof(aaSender));
			this.wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
		}

        public AAResult DoAA(AsymmetricAlgorithm publicKey, string digestAlgorithm, string signatureAlgorithm, byte[] challenge)
        {
            try
            {
                // Step 1: Send Internal Authenticate command with challenge
                var internalAuthCommand = CreateInternalAuthenticateCommand(challenge);
                var authResponse = aaSender.SendInternalAuthenticate(internalAuthCommand);
                
                // Step 2: Process the response and verify signature
                var signature = ExtractSignatureFromResponse(authResponse);
                var isValid = VerifySignature(publicKey, digestAlgorithm, signatureAlgorithm, challenge, signature);
                
                if (!isValid)
                {
                    throw new InvalidOperationException("Active Authentication signature verification failed");
                }
                
                return new AAResult(publicKey, digestAlgorithm, signatureAlgorithm, challenge, signature, isValid);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("AA protocol failed", ex);
            }
        }
        
        private byte[] CreateInternalAuthenticateCommand(byte[] challenge)
        {
            // Create Internal Authenticate command with challenge
            var command = new List<byte>();
            command.AddRange(new byte[] { 0x00, 0x88, 0x00, 0x00 }); // Internal Authenticate command header
            command.AddRange(new byte[] { 0x7C, (byte)(challenge.Length + 2) }); // Dynamic authentication template
            command.AddRange(new byte[] { 0x80, (byte)challenge.Length }); // Challenge
            command.AddRange(challenge);
            return command.ToArray();
        }
        
        private byte[] ExtractSignatureFromResponse(byte[] response)
        {
            // Extract signature from Internal Authenticate response
            // This is a simplified implementation
            if (response.Length < 2)
            {
                throw new InvalidOperationException("Invalid response length");
            }
            
            // Skip response template and extract signature
            var signatureLength = response.Length - 2;
            var signature = new byte[signatureLength];
            Array.Copy(response, 2, signature, 0, signatureLength);
            return signature;
        }
        
        private bool VerifySignature(AsymmetricAlgorithm publicKey, string digestAlgorithm, string signatureAlgorithm, byte[] challenge, byte[] signature)
        {
            try
            {
                // Create hash of challenge
                var hash = ComputeHash(challenge, digestAlgorithm);
                
                // Verify signature
                if (publicKey is RSA rsa)
                {
                    var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                    rsaDeformatter.SetHashAlgorithm(digestAlgorithm);
                    return rsaDeformatter.VerifySignature(hash, signature);
                }
                else if (publicKey is ECDsa ecdsa)
                {
                    return ecdsa.VerifyHash(hash, signature);
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private byte[] ComputeHash(byte[] data, string digestAlgorithm)
        {
            using var digest = GetDigestForAlgorithm(digestAlgorithm);
            digest.Initialize();
            return digest.ComputeHash(data);
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
