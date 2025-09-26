using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using org.jmrtd.CustomJavaAPI;
using org.jmrtd.protocol;

namespace org.jmrtd
{
	public abstract class AbstractMRTDCardService : FileSystemCardService
	{
		public abstract BACResult DoBAC(IAccessKeySpec keySpec);
		public abstract BACResult DoBAC(SecretKey keyEnc, SecretKey keyMac);

		[Obsolete("Use DoPACE with BigInteger parameter")]
		public PACEResult DoPACE(IAccessKeySpec keySpec, string oid, object? paramsSpec)
		{
			return DoPACE(keySpec, oid, paramsSpec, null);
		}

		public abstract PACEResult DoPACE(IAccessKeySpec keySpec, string oid, object? parameters, BigInteger? nonce);

		public abstract void SendSelectApplet(bool isMasterFile);
		public abstract void SendSelectMF();
		public abstract AAResult DoAA(AsymmetricAlgorithm publicKey, string digestAlg, string sigAlg, byte[] challenge);
		public abstract EACCAResult DoEACCA(BigInteger keyId, string keyAgreementAlg, string cipherAlg, AsymmetricAlgorithm publicKey);
		public abstract EACTAResult DoEACTA(object cvcPrincipal, IList<object> certs, AsymmetricAlgorithm privateKey, string terminalName, EACCAResult eaccaResult, string protocol);
		public abstract EACTAResult DoEACTA(object cvcPrincipal, IList<object> certs, AsymmetricAlgorithm privateKey, string terminalName, EACCAResult eaccaResult, PACEResult paceResult);
		public abstract SecureMessagingWrapper GetWrapper();
		public abstract int GetMaxReadBinaryLength();
	}
}


