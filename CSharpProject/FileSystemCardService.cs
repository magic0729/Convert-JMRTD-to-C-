using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public abstract class FileSystemCardService : ICardService
	{
		// Deprecated overload kept for compatibility with original API
		public abstract CardFileInputStream GetInputStream(short fid);
		public abstract CardFileInputStream GetInputStream(short fid, int offset);

		// ICardService implementation - abstract methods to be implemented by concrete classes
		public abstract void Open();
		public abstract void Close();
		public abstract bool IsOpen();
		public abstract ResponseAPDU Transmit(CommandAPDU command);
		public abstract void AddAPDUListener(IAPDUListener listener);
		public abstract void RemoveAPDUListener(IAPDUListener listener);
		public abstract ICollection<IAPDUListener> GetAPDUListeners();
		public abstract byte[] GetATR();
		public abstract bool IsConnectionLost();
		public abstract bool IsExtendedAPDULengthSupported();
	}
}


