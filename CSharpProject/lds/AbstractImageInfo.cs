using System;
using System.IO;

namespace org.jmrtd.lds
{
	public abstract class AbstractImageInfo : AbstractLDSInfo
	{
		private int type;
		private string mimeType = string.Empty;
		private byte[] imageBytes = Array.Empty<byte>();

		protected AbstractImageInfo(int type, string mimeType)
		{
			this.type = type;
			this.mimeType = mimeType;
		}

		protected AbstractImageInfo() { }

		protected void SetType(int type) => this.type = type;
		public int GetTypeCode() => type;

		protected void SetMimeType(string mimeType) => this.mimeType = mimeType;
		public string GetMimeType() => mimeType;

		protected void SetImageBytes(byte[] bytes) => imageBytes = bytes ?? Array.Empty<byte>();
		public byte[] GetImageBytes() => imageBytes;
		public int GetImageLength() => imageBytes?.Length ?? 0;

		protected void ReadImage(Stream inputStream, long length)
		{
			int len = (int)length;
			imageBytes = new byte[len];
			int total = 0;
			while (total < len)
			{
				int r = inputStream.Read(imageBytes, total, len - total);
				if (r <= 0) throw new EndOfStreamException();
				total += r;
			}
		}

		protected void WriteImage(Stream outputStream)
		{
			if (imageBytes == null) { outputStream.Write(new byte[0], 0, 0); return; }
			if (outputStream is org.jmrtd.CustomJavaAPI.TLVOutputStream tlvOut)
			{
				tlvOut.WriteValue(imageBytes);
			}
			else
			{
				outputStream.Write(imageBytes, 0, imageBytes.Length);
			}
		}

		protected abstract void ReadObjectCore(Stream inputStream);
		protected abstract void WriteObjectCore(Stream outputStream);

		public override void WriteObject(Stream outputStream)
		{
			WriteObjectCore(outputStream);
		}
	}
}
