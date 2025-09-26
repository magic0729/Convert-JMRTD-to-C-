using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using org.jmrtd.CustomJavaAPI;
using org.jmrtd.lds;

namespace org.jmrtd.protocol
{
    public class AAResult
    {
        public AsymmetricAlgorithm PublicKey { get; }
        public string DigestAlgorithm { get; }
        public string SignatureAlgorithm { get; }
        public byte[] Challenge { get; }
        public byte[] Signature { get; }
        public bool IsValid { get; }

        public AAResult(AsymmetricAlgorithm publicKey, string digestAlgorithm, string signatureAlgorithm, byte[] challenge, byte[] signature, bool isValid)
        {
            PublicKey = publicKey;
            DigestAlgorithm = digestAlgorithm;
            SignatureAlgorithm = signatureAlgorithm;
            Challenge = challenge;
            Signature = signature;
            IsValid = isValid;
        }
    }

    public class EACCAResult
    {
        public BigInteger KeyId { get; }
        public string ChipAuthenticationAlgorithm { get; }
        public string KeyAgreementAlgorithm { get; }
        public AsymmetricAlgorithm PublicKey { get; }
        public SecureMessagingWrapper Wrapper { get; }

        public EACCAResult(BigInteger keyId, string chipAuthenticationAlgorithm, string keyAgreementAlgorithm, AsymmetricAlgorithm publicKey, SecureMessagingWrapper wrapper)
        {
            KeyId = keyId;
            ChipAuthenticationAlgorithm = chipAuthenticationAlgorithm;
            KeyAgreementAlgorithm = keyAgreementAlgorithm;
            PublicKey = publicKey;
            Wrapper = wrapper;
        }
    }

    public class EACTAResult
    {
        public object CvcPrincipal { get; }
        public IList<object> CvcCertificates { get; }
        public AsymmetricAlgorithm TerminalAuthenticationPrivateKey { get; }
        public string SignatureAlgorithm { get; }
        public EACCAResult EaccaResult { get; }
        public bool AuthenticationResult { get; }

        public EACTAResult(object cvcPrincipal, IList<object> cvcCertificates, AsymmetricAlgorithm terminalAuthenticationPrivateKey, string signatureAlgorithm, EACCAResult eaccaResult, bool authenticationResult)
        {
            CvcPrincipal = cvcPrincipal;
            CvcCertificates = cvcCertificates;
            TerminalAuthenticationPrivateKey = terminalAuthenticationPrivateKey;
            SignatureAlgorithm = signatureAlgorithm;
            EaccaResult = eaccaResult;
            AuthenticationResult = authenticationResult;
        }
    }

    public class PACEResult
    {
        public IAccessKeySpec AccessKey { get; }
        public PACEInfo.MappingType MappingType { get; }
        public string KeyAgreementAlgorithm { get; }
        public string CipherAlgorithm { get; }
        public string DigestAlgorithm { get; }
        public int KeyLength { get; }
        public PACEMappingResult MappingResult { get; }
        public AsymmetricAlgorithm PcdKeyPair { get; }
        public AsymmetricAlgorithm PiccKeyPair { get; }
        public SecureMessagingWrapper Wrapper { get; }

        public PACEResult(IAccessKeySpec accessKey, PACEInfo.MappingType mappingType, string keyAgreementAlgorithm, string cipherAlgorithm, string digestAlgorithm, int keyLength, PACEMappingResult mappingResult, AsymmetricAlgorithm pcdKeyPair, AsymmetricAlgorithm piccKeyPair, SecureMessagingWrapper wrapper)
        {
            AccessKey = accessKey;
            MappingType = mappingType;
            KeyAgreementAlgorithm = keyAgreementAlgorithm;
            CipherAlgorithm = cipherAlgorithm;
            DigestAlgorithm = digestAlgorithm;
            KeyLength = keyLength;
            MappingResult = mappingResult;
            PcdKeyPair = pcdKeyPair;
            PiccKeyPair = piccKeyPair;
            Wrapper = wrapper;
        }
    }
}
