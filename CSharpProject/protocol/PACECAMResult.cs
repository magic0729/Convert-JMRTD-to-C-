using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.protocol
{
	public sealed class PACECAMResult : PACEResult
	{
		public PACECAMResult(IAccessKeySpec paceKey, org.jmrtd.lds.PACEInfo.MappingType mappingType, string agreementAlg, string cipherAlg, string digestAlg, int keyLength, PACEMappingResult mappingResult, System.Security.Cryptography.AsymmetricAlgorithm pcdKeyPair, System.Security.Cryptography.AsymmetricAlgorithm piccPublicKey, SecureMessagingWrapper wrapper)
			: base(paceKey, mappingType, agreementAlg, cipherAlg, digestAlg, keyLength, mappingResult, pcdKeyPair, piccPublicKey, wrapper) { }
	}
}


