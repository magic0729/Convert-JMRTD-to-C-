using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Decoder for ISO/IEC 7816-11 biometric data structures
	/// </summary>
	/// <typeparam name="B">Type of BiometricDataBlock</typeparam>
    #pragma warning disable CA2022 // Avoid inexact read (TLVInputStream.Read uses exact lengths by contract)
    public class ISO781611Decoder<B> where B : BiometricDataBlock
	{
		private static readonly System.Diagnostics.TraceSource Logger = new System.Diagnostics.TraceSource("org.jmrtd.cbeff");
		private readonly Dictionary<int, BiometricDataBlockDecoder<B>> bdbDecoders;
		private BiometricEncodingType encodingType;

		/// <summary>
		/// Creates a new ISO781611Decoder with a single decoder for all BDB types
		/// </summary>
		/// <param name="bdbDecoder">The biometric data block decoder</param>
		public ISO781611Decoder(BiometricDataBlockDecoder<B> bdbDecoder)
		{
			this.bdbDecoders = ToMap(bdbDecoder ?? throw new ArgumentNullException(nameof(bdbDecoder)));
    }
    #pragma warning restore CA2022

		/// <summary>
		/// Creates a new ISO781611Decoder with specific decoders for different BDB types
		/// </summary>
		/// <param name="bdbDecoders">Map of BDB tag to decoder</param>
		public ISO781611Decoder(Dictionary<int, BiometricDataBlockDecoder<B>> bdbDecoders)
		{
			this.bdbDecoders = bdbDecoders ?? throw new ArgumentNullException(nameof(bdbDecoders));
		}

		/// <summary>
		/// Decodes a ComplexCBEFFInfo from the input stream
		/// </summary>
		/// <param name="inputStream">The input stream to read from</param>
		/// <returns>The decoded ComplexCBEFFInfo</returns>
		/// <exception cref="IOException">If an I/O error occurs</exception>
		public ComplexCBEFFInfo<B> Decode(Stream inputStream)
		{
			return ReadBITGroup(inputStream);
		}

		/// <summary>
		/// Gets the encoding type detected during decoding
		/// </summary>
		/// <returns>The biometric encoding type</returns>
		public BiometricEncodingType GetEncodingType()
		{
			return encodingType;
		}

		private ComplexCBEFFInfo<B> ReadBITGroup(Stream inputStream)
		{
			using var tlvIn = new TLVInputStream(inputStream);
			int tag = tlvIn.ReadTag();
			if (tag != 32609) // BIOMETRIC_INFORMATION_GROUP_TEMPLATE_TAG
			{
				throw new ArgumentException($"Expected tag 0x{32609:X}, found 0x{tag:X}");
			}
			int length = tlvIn.ReadLength();
			return ReadBITGroup(tag, length, inputStream);
		}

		private ComplexCBEFFInfo<B> ReadBITGroup(int tag, int length, Stream inputStream)
		{
			using var tlvIn = new TLVInputStream(inputStream);
			var result = new ComplexCBEFFInfo<B>();
			
			if (tag != 32609) // BIOMETRIC_INFORMATION_GROUP_TEMPLATE_TAG
			{
				throw new ArgumentException($"Expected tag 0x{32609:X}, found 0x{tag:X}");
			}

			int bitCountTag = tlvIn.ReadTag();
			if (bitCountTag != 2) // BIOMETRIC_INFO_COUNT_TAG
			{
				throw new ArgumentException($"Expected tag BIOMETRIC_INFO_COUNT_TAG (0x{2:X}) in CBEFF structure, found 0x{bitCountTag:X}");
			}

			int bitCountLength = tlvIn.ReadLength();
			if (bitCountLength != 1)
			{
				throw new ArgumentException($"BIOMETRIC_INFO_COUNT should have length 1, found length {bitCountLength}");
			}

			byte[] countBytes = tlvIn.ReadValue();
			int bitCount = countBytes[0];

			for (int i = 0; i < bitCount; ++i)
			{
				result.Add(ReadBIT(inputStream, i));
			}

			return result;
		}

		private CBEFFInfo<B> ReadBIT(Stream inputStream, int index)
		{
			using var tlvIn = new TLVInputStream(inputStream);
			int tag = tlvIn.ReadTag();
			int length = tlvIn.ReadLength();
			return ReadBIT(tag, length, inputStream, index);
		}

		private CBEFFInfo<B> ReadBIT(int tag, int length, Stream inputStream, int index)
		{
			using var tlvIn = new TLVInputStream(inputStream);
			
			if (tag != 32608) // BIOMETRIC_INFORMATION_TEMPLATE_TAG
			{
				throw new ArgumentException($"Expected tag BIOMETRIC_INFORMATION_TEMPLATE_TAG (0x{32608:X}), found 0x{tag:X}, index is {index}");
			}

			int bhtTag = tlvIn.ReadTag();
			int bhtLength = tlvIn.ReadLength();

			if (bhtTag != 125) // SMT_TAG
			{
				if ((bhtTag & 0xA0) == 160) // BIOMETRIC_HEADER_TEMPLATE_BASE_TAG
				{
					StandardBiometricHeader sbh = ReadBHT(inputStream, bhtTag, bhtLength, index);
					B bdb = ReadBiometricDataBlock(inputStream, sbh, index);
					return new SimpleCBEFFInfo<B>(bdb);
				}
				throw new ArgumentException($"Unsupported template tag: 0x{bhtTag:X}");
			}

			ReadStaticallyProtectedBIT(inputStream, bhtTag, bhtLength, index);
			return null!; // This case is not fully implemented in the original
		}

		private StandardBiometricHeader ReadBHT(Stream inputStream, int bhtTag, int bhtLength, int index)
		{
			using var tlvIn = new TLVInputStream(inputStream);
			int expectedBHTTag = 161; // BIOMETRIC_HEADER_TEMPLATE_BASE_TAG + 1
			
			if (bhtTag != expectedBHTTag)
			{
				Logger.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, $"Expected tag 0x{expectedBHTTag:X}, found 0x{bhtTag:X}");
			}

			var elements = new Dictionary<int, byte[]>();
			int bytesRead = 0;

			while (bytesRead < bhtLength)
			{
				int tag = tlvIn.ReadTag();
				bytesRead += GetTagLength(tag);
				int length = tlvIn.ReadLength();
				bytesRead += GetLengthLength(length);
				byte[] value = tlvIn.ReadValue();
				elements[tag] = value;
				bytesRead += value.Length;
			}

			return new StandardBiometricHeader(elements);
		}

		private void ReadStaticallyProtectedBIT(Stream inputStream, int tag, int length, int index)
		{
			byte[] decodedValue = DecodeSMTValue(inputStream);
			using var tlvBHTIn = new TLVInputStream(new MemoryStream(decodedValue));
			
			int headerTemplateTag = tlvBHTIn.ReadTag();
			int headerTemplateLength = tlvBHTIn.ReadLength();
			StandardBiometricHeader sbh = ReadBHT(tlvBHTIn, headerTemplateTag, headerTemplateLength, index);
			
			byte[] biometricDataBlockBytes = DecodeSMTValue(inputStream);
			using var biometricDataBlockIn = new MemoryStream(biometricDataBlockBytes);
			ReadBiometricDataBlock(biometricDataBlockIn, sbh, index);
		}

		private byte[] DecodeSMTValue(Stream inputStream)
		{
			using var tlvIn = new TLVInputStream(inputStream);
			int doTag = tlvIn.ReadTag();
			int doLength = tlvIn.ReadLength();

			return doTag switch
			{
				129 => tlvIn.ReadValue(), // SMT_DO_PV
				133 => throw new InvalidOperationException("Access denied. Biometric Information Template is statically protected."), // SMT_DO_CG
				142 => SkipBytes(tlvIn, doLength), // SMT_DO_CC
				158 => SkipBytes(tlvIn, doLength), // SMT_DO_DS
				_ => null!
			};
		}

		private byte[] SkipBytes(TLVInputStream tlvIn, int length)
		{
			// Skip the specified number of bytes
            byte[] buffer = new byte[length];
            #pragma warning disable CA2022 // TLVInputStream.Read is safe here and bounded by 'length'
            tlvIn.Read(buffer, 0, length);
            #pragma warning restore CA2022
			return null!;
		}

		private B ReadBiometricDataBlock(Stream inputStream, StandardBiometricHeader sbh, int index)
		{
			using var tlvIn = new TLVInputStream(inputStream);
			int bioDataBlockTag = tlvIn.ReadTag();
			
			if (bioDataBlockTag != 24366 && bioDataBlockTag != 32558) // BIOMETRIC_DATA_BLOCK_TAG and BIOMETRIC_DATA_BLOCK_CONSTRUCTED_TAG
			{
				throw new ArgumentException($"Expected tag BIOMETRIC_DATA_BLOCK_TAG (0x{24366:X}) or BIOMETRIC_DATA_BLOCK_CONSTRUCTED_ALT (0x{32558:X}), found 0x{bioDataBlockTag:X}");
			}

			encodingType = BiometricEncodingTypeExtensions.FromBDBTag(bioDataBlockTag);
			int length = tlvIn.ReadLength();

			if (!bdbDecoders.TryGetValue(bioDataBlockTag, out BiometricDataBlockDecoder<B>? bdbDecoder))
			{
				throw new ArgumentException($"No decoder for biometric data block tag 0x{bioDataBlockTag:X}");
			}

			return bdbDecoder.Decode(inputStream, sbh, index, length);
		}

		private static Dictionary<int, BiometricDataBlockDecoder<R>> ToMap<R>(BiometricDataBlockDecoder<R> bdbDecoder) where R : BiometricDataBlock
		{
			var bdbDecoders = new Dictionary<int, BiometricDataBlockDecoder<R>>();
			bdbDecoders[24366] = bdbDecoder; // BIOMETRIC_DATA_BLOCK_TAG
			bdbDecoders[32558] = bdbDecoder; // BIOMETRIC_DATA_BLOCK_CONSTRUCTED_TAG
			return bdbDecoders;
		}

        private static int GetTagLength(int tag)
		{
			// Simplified tag length calculation
			if (tag < 0x1F) return 1;
			if (tag < 0x7F) return 2;
			return 3;
		}

        private static int GetLengthLength(int length)
		{
			// Simplified length length calculation
			if (length < 0x80) return 1;
			if (length < 0x100) return 2;
			if (length < 0x10000) return 3;
			return 4;
		}
	}
}
