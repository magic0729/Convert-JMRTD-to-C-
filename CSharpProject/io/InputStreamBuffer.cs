using System;
using System.IO;

namespace org.jmrtd.io
{
	public class InputStreamBuffer
	{
		private readonly PositionInputStream carrier;
		private readonly FragmentBuffer buffer;

		public InputStreamBuffer(Stream inputStream, int length)
		{
			carrier = new PositionInputStream(inputStream);
			buffer = new FragmentBuffer(length);
		}

		public void UpdateFrom(InputStreamBuffer other) => buffer.UpdateFrom(other.buffer);

		public SubInputStream GetInputStream()
		{
			lock (carrier) { return new SubInputStream(this, carrier); }
		}

		public int GetPosition() => buffer.GetPosition();
		public int GetBytesBuffered() => buffer.GetBytesBuffered();
		public int GetLength() => buffer.GetLength();

		public sealed class SubInputStream : Stream
		{
			private readonly InputStreamBuffer owner;
			private readonly object syncObject;
			private int position = 0;
			private int markedPosition = -1;

			internal SubInputStream(InputStreamBuffer owner, object syncObject)
			{
				this.owner = owner;
				this.syncObject = syncObject;
			}

			public FragmentBuffer GetBuffer() => owner.buffer;

			public override int Read(byte[] buffer, int offset, int count)
			{
				lock (syncObject)
				{
					if (buffer == null) throw new ArgumentNullException(nameof(buffer));
					if (offset < 0 || count < 0 || count > buffer.Length - offset) throw new ArgumentOutOfRangeException();
					if (count == 0) return 0;
					if (position >= owner.buffer.GetLength()) return -1;
					if (count > owner.buffer.GetLength() - position) count = owner.buffer.GetLength() - position;

					var fragment = owner.buffer.GetSmallestUnbufferedFragment(position, count);
					if (fragment.Length > 0)
					{
						int alreadyBufferedPrefixLength = fragment.Offset - position;
						int unbufferedPostfixLength = fragment.Length;
						Buffer.BlockCopy(owner.buffer.GetBuffer(), position, buffer, offset, alreadyBufferedPrefixLength);
						position += alreadyBufferedPrefixLength;

						// Move the underlying carrier to desired position using Seek if possible
						int bytesReadFromCarrier = 0;
						if (owner.carrier.CanSeek)
						{
							owner.carrier.Seek(position, SeekOrigin.Begin);
							bytesReadFromCarrier = owner.carrier.Read(buffer, offset + alreadyBufferedPrefixLength, unbufferedPostfixLength);
						}
						else
						{
							bytesReadFromCarrier = owner.carrier.Read(buffer, offset + alreadyBufferedPrefixLength, unbufferedPostfixLength);
						}
						owner.buffer.AddFragment(fragment.Offset, buffer, offset + alreadyBufferedPrefixLength, bytesReadFromCarrier);
						position += bytesReadFromCarrier;
						return alreadyBufferedPrefixLength + bytesReadFromCarrier;
					}

					int length = Math.Min(count, owner.buffer.GetLength() - position);
					Buffer.BlockCopy(owner.buffer.GetBuffer(), position, buffer, offset, length);
					position += length;
					return length;
				}
			}

			public override int ReadByte()
			{
				lock (syncObject)
				{
					if (position >= owner.buffer.GetLength()) return -1;
					if (owner.buffer.IsCoveredByFragment(position)) return owner.buffer.GetBuffer()[position++] & 0xFF;
					if (owner.carrier.CanSeek) owner.carrier.Seek(position, SeekOrigin.Begin);
					int result = owner.carrier.ReadByte();
					if (result < 0) return -1;
					owner.buffer.AddFragment(position++, (byte)result);
					return result;
				}
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				lock (syncObject)
				{
					if (origin == SeekOrigin.Begin) position = (int)offset;
					else if (origin == SeekOrigin.Current) position += (int)offset;
					else position = owner.buffer.GetLength() + (int)offset;
					return position;
				}
			}

			public long Skip(long n)
			{
				lock (syncObject)
				{
					int leftInBuffer = owner.buffer.GetBufferedLength(position);
					if (n <= leftInBuffer)
					{
						position += (int)n;
						return n;
					}
					int consumed = leftInBuffer;
					position += leftInBuffer;
					long skippedBytes = 0;
					if (owner.carrier.CanSeek)
					{
						owner.carrier.Seek(position, SeekOrigin.Begin);
						skippedBytes = owner.carrier.Seek(n - leftInBuffer, SeekOrigin.Current) - owner.carrier.Position;
						position += (int)skippedBytes;
					}
					else
					{
						// Fallback: read and discard
						Span<byte> tmp = stackalloc byte[4096];
						long remaining = n - leftInBuffer;
						while (remaining > 0)
						{
							int toRead = (int)Math.Min(tmp.Length, remaining);
							int r = Read(tmp.Slice(0, toRead).ToArray(), 0, toRead);
							if (r <= 0) break;
							remaining -= r;
							skippedBytes += r;
						}
					}
					return consumed + skippedBytes;
				}
			}

			public override void Flush() { }
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
			public override bool CanRead => true;
			public override bool CanSeek => true;
			public override bool CanWrite => false;
			public override long Length => owner.buffer.GetLength();
			public override long Position { get => position; set => position = (int)value; }

			public void Mark(int readLimit) { markedPosition = position; }
			public void Reset()
			{
				if (markedPosition < 0) throw new IOException("Invalid reset, was Mark() called?");
				position = markedPosition;
			}
		}

	}
}


