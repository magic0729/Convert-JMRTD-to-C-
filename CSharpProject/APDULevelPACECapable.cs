using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public interface APDULevelPACECapable
	{
		void SendMSESetATMutualAuth(APDUWrapper wrapper, string oid, int paceKeyReference, byte[]? referencePrivateKeyOrForComputingSessionKey);
		byte[] SendGeneralAuthenticate(APDUWrapper wrapper, byte[] data, int ne, bool isLast);
	}
}


