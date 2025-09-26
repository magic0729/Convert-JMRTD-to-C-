using System;
using System.IO;

namespace org.jmrtd.CustomJavaAPI
{
	public static class TLV
	{
		public static byte[] WrapDO(int tag, byte[] value)
		{
			using var ms = new MemoryStream();
			WriteTag(ms, tag);
			WriteLength(ms, value?.Length ?? 0);
			if (value != null && value.Length > 0) ms.Write(value, 0, value.Length);
			return ms.ToArray();
		}

		public static byte[] UnwrapDO(int expectedTag, byte[] tlv)
		{
			using var ms = new MemoryStream(tlv);
			int tag = ReadTag(ms);
			if (tag != expectedTag) throw new InvalidDataException($"Expected tag 0x{expectedTag:X}, found 0x{tag:X}");
			int length = ReadLength(ms);
			byte[] value = new byte[length];
			int read = ms.Read(value, 0, length);
			if (read != length) throw new EndOfStreamException();
			return value;
		}

		private static void WriteTag(Stream s, int tag)
		{
			if (tag <= 0xFF)
			{
				s.WriteByte((byte)tag);
			}
			else if (tag <= 0xFFFF)
			{
				s.WriteByte((byte)((tag >> 8) & 0xFF));
				s.WriteByte((byte)(tag & 0xFF));
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(tag));
			}
		}

		private static void WriteLength(Stream s, int length)
		{
			if (length < 0x80)
			{
				s.WriteByte((byte)length);
				return;
			}
			// Long form
			if (length <= 0xFF)
			{
				s.WriteByte(0x81);
				s.WriteByte((byte)length);
			}
			else if (length <= 0xFFFF)
			{
				s.WriteByte(0x82);
				s.WriteByte((byte)((length >> 8) & 0xFF));
				s.WriteByte((byte)(length & 0xFF));
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}
		}

		private static int ReadTag(Stream s)
		{
			int b1 = s.ReadByte();
			if (b1 < 0) throw new EndOfStreamException();
			if ((b1 & 0x1F) == 0x1F)
			{
				int b2 = s.ReadByte();
				if (b2 < 0) throw new EndOfStreamException();
				return (b1 << 8) | b2;
			}
			return b1;
		}

		private static int ReadLength(Stream s)
		{
			int first = s.ReadByte();
			if (first < 0) throw new EndOfStreamException();
			if ((first & 0x80) == 0)
			{
				return first;
			}
			int numBytes = first & 0x7F;
			if (numBytes == 1)
			{
				int b = s.ReadByte();
				if (b < 0) throw new EndOfStreamException();
				return b;
			}
			if (numBytes == 2)
			{
				int b1 = s.ReadByte();
				int b2 = s.ReadByte();
				if (b1 < 0 || b2 < 0) throw new EndOfStreamException();
				return (b1 << 8) | b2;
			}
			throw new NotSupportedException("Lengths > 2 bytes not supported");
		}
	}
}


