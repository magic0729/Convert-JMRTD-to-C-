using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public interface APDULevelReadBinaryCapable
	{
		void SendSelectApplet(APDUWrapper wrapper, byte[] aid);
		void SendSelectMF();
		void SendSelectFile(APDUWrapper wrapper, short fid);
		byte[] SendReadBinary(APDUWrapper wrapper, int fid, int offset, int length, bool isResponseChained, bool isShortLe);
	}
}


