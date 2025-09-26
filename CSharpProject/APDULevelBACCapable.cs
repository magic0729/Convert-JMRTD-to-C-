using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public interface APDULevelBACCapable
	{
		byte[] SendGetChallenge();
		byte[] SendMutualAuth(byte[] rndIFD, byte[] rndICC, byte[] kIFD, SecretKey kEnc, SecretKey kMac);
	}
}


