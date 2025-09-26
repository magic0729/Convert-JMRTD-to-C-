using System;
using System.IO;

namespace org.jmrtd.io
{
	public class PositionInputStream : Stream
	{
		private readonly Stream carrier;
		private long position;
		private long markedPosition = -1;

		public PositionInputStream(Stream carrier)
		{
			this.carrier = carrier;
			position = 0;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int read = carrier.Read(buffer, offset, count);
			if (read > 0) position += read;
			return read;
		}

		public override int ReadByte()
		{
			int b = carrier.ReadByte();
			if (b >= 0) position++;
			return b;
		}

		public override long Seek(long offset, SeekOrigin origin) => carrier.Seek(offset, origin);
		public override void SetLength(long value) => carrier.SetLength(value);
		public override void Flush() => carrier.Flush();
		public override void Write(byte[] buffer, int offset, int count) => carrier.Write(buffer, offset, count);
		public override bool CanRead => carrier.CanRead;
		public override bool CanSeek => carrier.CanSeek;
		public override bool CanWrite => carrier.CanWrite;
		public override long Length => carrier.Length;
		public override long Position { get => position; set => throw new NotSupportedException(); }

		public void Mark(int readLimit)
		{
			markedPosition = position;
		}

		public void Reset()
		{
			if (markedPosition < 0) throw new IOException("Invalid reset, was Mark() called?");
			if (!carrier.CanSeek) throw new IOException("Underlying stream not seekable");
			carrier.Seek(markedPosition, SeekOrigin.Begin);
			position = markedPosition;
		}
	}
}


