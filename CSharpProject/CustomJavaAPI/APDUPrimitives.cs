using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace org.jmrtd.CustomJavaAPI
{
	public interface ISerializable
	{
		// Marker interface for serializable objects
	}

	public interface IAccessKeySpec : ISerializable
	{
		string Algorithm { get; }
		byte[] Key { get; }
		byte[] GetKey() => Key; // Compatibility method
	}

	public interface IBACKeySpec : IAccessKeySpec
	{
		string DocumentNumber { get; }
		string DateOfBirth { get; }
		string DateOfExpiry { get; }
	}
	// Minimal shims for net.sf.scuba.smartcards primitives.
	public class CommandAPDU
	{
		public int CLA { get; }
		public int INS { get; }
		public int P1 { get; }
		public int P2 { get; }
		public int Nc => Data?.Length ?? 0;
		public int Ne { get; }
		public byte[]? Data { get; }
		public byte[] Bytes { get; }

		public CommandAPDU(byte[] bytes)
		{
			Bytes = bytes;
			// Very loose parsing just for logging
			if (bytes.Length >= 4)
			{
				CLA = bytes[0]; INS = bytes[1]; P1 = bytes[2]; P2 = bytes[3];
			}
		}

		public CommandAPDU(int cla, int ins, int p1, int p2, int ne = 0)
			: this(cla, ins, p1, p2, Array.Empty<byte>(), ne) { }

		public CommandAPDU(int cla, int ins, int p1, int p2, byte[] data, int ne = 0)
		{
			CLA = cla; INS = ins; P1 = p1; P2 = p2; Data = data; Ne = ne;
			var list = new List<byte> { (byte)cla, (byte)ins, (byte)p1, (byte)p2 };
			if (data != null && data.Length > 0)
			{
				list.Add((byte)data.Length);
				list.AddRange(data);
			}
			if (ne > 0) list.Add((byte)(ne == 65536 ? 0 : Math.Min(ne, 256)));
			Bytes = list.ToArray();
		}
	}

	public class ResponseAPDU
	{
		public byte[] Bytes { get; }
		public int StatusWord { get; }
		public byte[]? Data { get; }

		public ResponseAPDU(byte[] bytes)
		{
			Bytes = bytes;
			if (bytes.Length >= 2)
			{
				StatusWord = (bytes[^2] << 8) | bytes[^1];
				if (bytes.Length > 2)
				{
					Data = new byte[bytes.Length - 2];
					Array.Copy(bytes, 0, Data, 0, Data.Length);
				}
			}
		}

		public ResponseAPDU(byte[] data, int statusWord)
		{
			Data = data;
			StatusWord = statusWord;
			Bytes = BuildBytes(data, statusWord);
		}

		private static byte[] BuildBytes(byte[]? data, int sw)
		{
			var list = new List<byte>();
			if (data != null && data.Length > 0) list.AddRange(data);
			list.Add((byte)((sw >> 8) & 0xFF));
			list.Add((byte)(sw & 0xFF));
			return list.ToArray();
		}
	}

	public class APDUEvent : EventArgs
	{
		public object Source { get; }
		public object? Type { get; }
		public int SequenceNumber { get; }
		public CommandAPDU Command { get; }
		public ResponseAPDU Response { get; }

		public APDUEvent(object source, object? type, int sequenceNumber, CommandAPDU command, ResponseAPDU response)
		{
			Source = source;
			Type = type;
			SequenceNumber = sequenceNumber;
			Command = command;
			Response = response;
		}
	}

	public interface APDUWrapper : IAPDUWrapper
	{
	}

	public interface APDUListener
	{
		void ExchangedAPDU(APDUEvent e);
	}

	public interface IAPDUWrapper
	{
		CommandAPDU Wrap(CommandAPDU command);
		ResponseAPDU Unwrap(ResponseAPDU response);
		object Type { get; }
		long GetSendSequenceCounter();
	}


	public interface IAPDUListener
	{
		void ExchangedAPDU(APDUEvent apduEvent);
	}

	public interface IAPDULevelReadBinaryCapable
	{
		void SendSelectApplet(IAPDUWrapper? wrapper, byte[] aid);
		void SendSelectMF();
		void SendSelectFile(IAPDUWrapper? wrapper, short fid);
		byte[] SendReadBinary(IAPDUWrapper? wrapper, int sfi, int offset, int length, bool isSFI, bool isTLVEncodedOffsetNeeded);
	}

	public interface ICardService
	{
		void Open();
		void Close();
		bool IsOpen();
		ResponseAPDU Transmit(CommandAPDU command);
		void AddAPDUListener(IAPDUListener listener);
		void RemoveAPDUListener(IAPDUListener listener);
		ICollection<IAPDUListener> GetAPDUListeners();
		byte[] GetATR();
		bool IsConnectionLost();
		bool IsExtendedAPDULengthSupported();
	}

	public abstract class CardService : ICardService
	{
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

	public class CardFileInputStream : Stream
	{
		private readonly Stream underlyingStream;

		public CardFileInputStream(Stream underlyingStream)
		{
			this.underlyingStream = underlyingStream ?? throw new ArgumentNullException(nameof(underlyingStream));
		}

		public CardFileInputStream(Stream underlyingStream, int length)
		{
			this.underlyingStream = underlyingStream ?? throw new ArgumentNullException(nameof(underlyingStream));
			// Length parameter is ignored in this simple implementation
		}

		public override int Read(byte[] buffer, int offset, int count) => underlyingStream.Read(buffer, offset, count);
		public override int ReadByte() => underlyingStream.ReadByte();
		public override long Seek(long offset, SeekOrigin origin) => underlyingStream.Seek(offset, origin);
		public override void SetLength(long value) => underlyingStream.SetLength(value);
		public override void Write(byte[] buffer, int offset, int count) => underlyingStream.Write(buffer, offset, count);
		public override void Flush() => underlyingStream.Flush();

		public override bool CanRead => underlyingStream.CanRead;
		public override bool CanSeek => underlyingStream.CanSeek;
		public override bool CanWrite => underlyingStream.CanWrite;
		public override long Length => underlyingStream.Length;
		public override long Position { get => underlyingStream.Position; set => underlyingStream.Position = value; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				underlyingStream?.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	public abstract class SecureMessagingWrapper : IAPDUWrapper
	{
		protected readonly SecretKey ksEnc;
		protected readonly SecretKey ksMac;
		protected readonly int maxTranceiveLength;
		protected readonly bool shouldCheckMAC;
		protected readonly long sendSequenceCounter;
		protected long currentSSC;

		protected SecureMessagingWrapper(SecretKey ksEnc, SecretKey ksMac, int maxTranceiveLength, bool shouldCheckMAC, long sendSequenceCounter)
		{
			this.ksEnc = ksEnc ?? throw new ArgumentNullException(nameof(ksEnc));
			this.ksMac = ksMac ?? throw new ArgumentNullException(nameof(ksMac));
			this.maxTranceiveLength = maxTranceiveLength;
			this.shouldCheckMAC = shouldCheckMAC;
			this.sendSequenceCounter = sendSequenceCounter;
			this.currentSSC = sendSequenceCounter;
		}

		public abstract CommandAPDU Wrap(CommandAPDU command);
		public abstract ResponseAPDU Unwrap(ResponseAPDU response);
		public abstract object Type { get; }
		
		// Legacy method names for compatibility
		public virtual CommandAPDU WrapCommand(CommandAPDU command) => Wrap(command);
		public virtual ResponseAPDU UnwrapResponse(ResponseAPDU response) => Unwrap(response);

		// Accessor methods
		public SecretKey GetEncryptionKey() => ksEnc;
		public SecretKey GetMACKey() => ksMac;
		public int GetMaxTranceiveLength() => maxTranceiveLength;
		public bool ShouldCheckMAC() => shouldCheckMAC;
		public long GetSendSequenceCounter() => sendSequenceCounter;

		protected byte[] GetSscBlockAndIncrement(int blockSize)
		{
			// SSC is incremented per APDU; represented as big-endian block
			checked { currentSSC = currentSSC + 1; }
			byte[] block = new byte[blockSize];
			long val = currentSSC;
			for (int i = 0; i < blockSize; i++)
			{
				block[blockSize - 1 - i] = (byte)(val & 0xFF);
				val >>= 8;
			}
			return block;
		}

		protected static byte[] BuildProtectedHeader(int cla, int ins, int p1, int p2)
		{
			// Set SM bits in CLA (bit 4 and 5)
			byte protectedCla = (byte)(cla | 0x0C);
			return new byte[] { protectedCla, (byte)ins, (byte)p1, (byte)p2 };
		}

		protected static byte[] Concat(byte[] a, byte[] b)
		{
			var r = new byte[a.Length + b.Length];
			Array.Copy(a, 0, r, 0, a.Length);
			Array.Copy(b, 0, r, a.Length, b.Length);
			return r;
		}

		protected static byte[] BuildMacInput(CommandAPDU command, byte[] enc)
		{
			return Concat(new byte[] { (byte)command.CLA, (byte)command.INS, (byte)command.P1, (byte)command.P2 }, enc);
		}

		protected static byte[] EncryptCBC(byte[] key, byte[] plaintext)
		{
			using var aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.IV = new byte[16];
			aes.Key = key.Length >= 16 ? key.AsSpan(0, 16).ToArray() : key;
			using var enc = aes.CreateEncryptor();
			return enc.TransformFinalBlock(plaintext, 0, plaintext.Length);
		}

		protected static byte[] DecryptCBC(byte[] key, byte[] ciphertext)
		{
			using var aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.IV = new byte[16];
			aes.Key = key.Length >= 16 ? key.AsSpan(0, 16).ToArray() : key;
			using var dec = aes.CreateDecryptor();
			return dec.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
		}

		// Static factory method for compatibility
		public static SecureMessagingWrapper GetInstance(SecretKey ksEnc, SecretKey ksMac, int maxTranceiveLength, bool shouldCheckMAC, long sendSequenceCounter)
		{
			// Return a default implementation - this should be overridden by concrete classes
			return new DefaultSecureMessagingWrapper(ksEnc, ksMac, maxTranceiveLength, shouldCheckMAC, sendSequenceCounter);
		}
	}

	// Default implementation for GetInstance
	public class DefaultSecureMessagingWrapper : SecureMessagingWrapper
	{
		public DefaultSecureMessagingWrapper(SecretKey ksEnc, SecretKey ksMac, int maxTranceiveLength, bool shouldCheckMAC, long sendSequenceCounter)
			: base(ksEnc, ksMac, maxTranceiveLength, shouldCheckMAC, sendSequenceCounter)
		{
		}

		public override CommandAPDU Wrap(CommandAPDU command) => command;
		public override ResponseAPDU Unwrap(ResponseAPDU response) => response;
		public override object Type => "Default";
	}

	public class TLVInputStream : Stream
	{
		private readonly Stream underlyingStream;

		public TLVInputStream(Stream underlyingStream)
		{
			this.underlyingStream = underlyingStream ?? throw new ArgumentNullException(nameof(underlyingStream));
		}

		/// <summary>
		/// Reads a TLV tag from the stream
		/// </summary>
		/// <returns>The tag value</returns>
		public int ReadTag()
		{
			int firstByte = underlyingStream.ReadByte();
			if (firstByte == -1) throw new EndOfStreamException("Unexpected end of stream while reading tag");

			// Simple tag parsing - handles most common cases
			if ((firstByte & 0x1F) != 0x1F)
			{
				// Single byte tag
				return firstByte;
			}
			else
			{
				// Multi-byte tag
				int tag = firstByte;
				int nextByte;
				do
				{
					nextByte = underlyingStream.ReadByte();
					if (nextByte == -1) throw new EndOfStreamException("Unexpected end of stream while reading tag");
					tag = (tag << 8) | nextByte;
				} while ((nextByte & 0x80) != 0);
				return tag;
			}
		}

		/// <summary>
		/// Reads a TLV length from the stream
		/// </summary>
		/// <returns>The length value</returns>
		public int ReadLength()
		{
			int firstByte = underlyingStream.ReadByte();
			if (firstByte == -1) throw new EndOfStreamException("Unexpected end of stream while reading length");

			if ((firstByte & 0x80) == 0)
			{
				// Short form - single byte length
				return firstByte;
			}
			else
			{
				// Long form - multiple bytes
				int numBytes = firstByte & 0x7F;
				if (numBytes == 0) throw new ArgumentException("Invalid length encoding");
				if (numBytes > 4) throw new ArgumentException("Length too large");

				int length = 0;
				for (int i = 0; i < numBytes; i++)
				{
					int nextByte = underlyingStream.ReadByte();
					if (nextByte == -1) throw new EndOfStreamException("Unexpected end of stream while reading length");
					length = (length << 8) | nextByte;
				}
				return length;
			}
		}

		/// <summary>
		/// Reads TLV value bytes from the stream
		/// </summary>
		/// <returns>The value bytes</returns>
		public byte[] ReadValue()
		{
			int length = ReadLength();
			if (length == 0) return Array.Empty<byte>();

			byte[] value = new byte[length];
			int totalRead = 0;
			while (totalRead < length)
			{
				int bytesRead = underlyingStream.Read(value, totalRead, length - totalRead);
				if (bytesRead == 0) throw new EndOfStreamException("Unexpected end of stream while reading value");
				totalRead += bytesRead;
			}
			return value;
		}

		/// <summary>
		/// Skips to the next occurrence of the specified tag
		/// </summary>
		/// <param name="tag">The tag to skip to</param>
		public void SkipToTag(int tag)
		{
			while (true)
			{
				int currentTag = ReadTag();
				if (currentTag == tag) return;
				
				int length = ReadLength();
				underlyingStream.Seek(length, SeekOrigin.Current);
			}
		}

		public override int Read(byte[] buffer, int offset, int count) => underlyingStream.Read(buffer, offset, count);
		public override int ReadByte() => underlyingStream.ReadByte();
		public override long Seek(long offset, SeekOrigin origin) => underlyingStream.Seek(offset, origin);
		public override void SetLength(long value) => underlyingStream.SetLength(value);
		public override void Write(byte[] buffer, int offset, int count) => underlyingStream.Write(buffer, offset, count);
		public override void Flush() => underlyingStream.Flush();

		public override bool CanRead => underlyingStream.CanRead;
		public override bool CanSeek => underlyingStream.CanSeek;
		public override bool CanWrite => underlyingStream.CanWrite;
		public override long Length => underlyingStream.Length;
		public override long Position { get => underlyingStream.Position; set => underlyingStream.Position = value; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				underlyingStream?.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	public class TLVOutputStream : Stream
	{
		private readonly Stream underlyingStream;

		public TLVOutputStream(Stream underlyingStream)
		{
			this.underlyingStream = underlyingStream ?? throw new ArgumentNullException(nameof(underlyingStream));
		}

		/// <summary>
		/// Writes a TLV tag to the stream
		/// </summary>
		/// <param name="tag">The tag to write</param>
		public void WriteTag(int tag)
		{
			if (tag < 0x1F)
			{
				// Single byte tag
				underlyingStream.WriteByte((byte)tag);
			}
			else
			{
				// Multi-byte tag
				var tagBytes = new List<byte>();
				while (tag > 0)
				{
					byte b = (byte)(tag & 0x7F);
					tag >>= 7;
					if (tag > 0) b |= 0x80;
					tagBytes.Insert(0, b);
				}
				underlyingStream.Write(tagBytes.ToArray(), 0, tagBytes.Count);
			}
		}

		/// <summary>
		/// Writes a TLV length to the stream
		/// </summary>
		/// <param name="length">The length to write</param>
		public void WriteLength(int length)
		{
			if (length < 0x80)
			{
				// Short form - single byte length
				underlyingStream.WriteByte((byte)length);
			}
			else
			{
				// Long form - multiple bytes
				var lengthBytes = new List<byte>();
				while (length > 0)
				{
					lengthBytes.Insert(0, (byte)(length & 0xFF));
					length >>= 8;
				}
				underlyingStream.WriteByte((byte)(0x80 | lengthBytes.Count));
				underlyingStream.Write(lengthBytes.ToArray(), 0, lengthBytes.Count);
			}
		}

		/// <summary>
		/// Writes TLV value bytes to the stream
		/// </summary>
		/// <param name="value">The value bytes to write</param>
		public void WriteValue(byte[] value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			WriteLength(value.Length);
			if (value.Length > 0)
			{
				underlyingStream.Write(value, 0, value.Length);
			}
		}

		/// <summary>
		/// Writes the end of a TLV value (used for constructed types)
		/// </summary>
		public void WriteValueEnd()
		{
			// For constructed types, this would write the end marker
			// In this simplified implementation, we don't need to do anything
		}

		public override int Read(byte[] buffer, int offset, int count) => underlyingStream.Read(buffer, offset, count);
		public override int ReadByte() => underlyingStream.ReadByte();
		public override long Seek(long offset, SeekOrigin origin) => underlyingStream.Seek(offset, origin);
		public override void SetLength(long value) => underlyingStream.SetLength(value);
		public override void Write(byte[] buffer, int offset, int count) => underlyingStream.Write(buffer, offset, count);
		public override void Flush() => underlyingStream.Flush();

		public override bool CanRead => underlyingStream.CanRead;
		public override bool CanSeek => underlyingStream.CanSeek;
		public override bool CanWrite => underlyingStream.CanWrite;
		public override long Length => underlyingStream.Length;
		public override long Position { get => underlyingStream.Position; set => underlyingStream.Position = value; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				underlyingStream?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}


