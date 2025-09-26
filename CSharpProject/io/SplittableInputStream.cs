using System;
using System.IO;

namespace org.jmrtd.io
{
	public class SplittableInputStream : Stream
	{
		private readonly InputStreamBuffer inputStreamBuffer;
		private readonly InputStreamBuffer.SubInputStream carrier;

		public SplittableInputStream(Stream inputStream, int length)
		{
			inputStreamBuffer = new InputStreamBuffer(inputStream, length);
			carrier = inputStreamBuffer.GetInputStream();
		}

		public void UpdateFrom(SplittableInputStream other) => inputStreamBuffer.UpdateFrom(other.inputStreamBuffer);

		public Stream GetInputStream(int position)
		{
			var s = inputStreamBuffer.GetInputStream();
			long skipped = 0;
			Span<byte> tmp = stackalloc byte[4096];
			while (skipped < position)
			{
				int toRead = (int)Math.Min(tmp.Length, position - skipped);
				int r = s.Read(tmp.Slice(0, toRead).ToArray(), 0, toRead);
				if (r <= 0) break;
				skipped += r;
			}
			return s;
		}

		public int GetPosition() => (int)carrier.Position;

		public override int Read(byte[] buffer, int offset, int count) => carrier.Read(buffer, offset, count);
		public override int ReadByte() => carrier.ReadByte();
		public override long Seek(long offset, SeekOrigin origin) => carrier.Seek(offset, origin);
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Flush() { }
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => inputStreamBuffer.GetLength();
		public override long Position { get => carrier.Position; set => carrier.Seek(value, SeekOrigin.Begin); }



		public override int Read(Span<byte> buffer)
		{
			byte[] tmp = System.Buffers.ArrayPool<byte>.Shared.Rent(buffer.Length);
			try
			{
				int r = Read(tmp, 0, buffer.Length);
				tmp.AsSpan(0, r).CopyTo(buffer);
				return r;
			}
			finally
			{
				System.Buffers.ArrayPool<byte>.Shared.Return(tmp);
			}
		}

		public int GetLength() => inputStreamBuffer.GetLength();
		public int GetBytesBuffered() => inputStreamBuffer.GetBytesBuffered();
	}
}


