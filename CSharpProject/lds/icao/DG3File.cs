using System;
using System.Collections.Generic;
using System.IO;
using org.jmrtd.CustomJavaAPI;
using org.jmrtd.cbeff;

namespace org.jmrtd.lds.icao
{
	public class DG3File : org.jmrtd.lds.CBEFFDataGroup
	{
		private static readonly ISO781611Decoder<BiometricDataBlock> DECODER = new ISO781611Decoder<BiometricDataBlock>(GetDecoderMap());
		private static readonly ISO781611Encoder<BiometricDataBlock> ISO_19794_ENCODER = new ISO781611Encoder<BiometricDataBlock>(new Finger19794Encoder());
		private static readonly ISO781611Encoder<BiometricDataBlock> ISO_39794_ENCODER = new ISO781611Encoder<BiometricDataBlock>(new Finger39794Encoder());

		private static Dictionary<int, BiometricDataBlockDecoder<BiometricDataBlock>> GetDecoderMap()
		{
			var decoders = new Dictionary<int, BiometricDataBlockDecoder<BiometricDataBlock>>();
			decoders[24366] = new Finger19794Decoder();
			decoders[32558] = new Finger39794Decoder();
			return decoders;
		}

		private sealed class Finger19794Encoder : BiometricDataBlockEncoder<BiometricDataBlock>
		{
			public void Encode(BiometricDataBlock info, Stream outputStream)
			{
				if (info is org.jmrtd.lds.iso19794.FingerInfo fi)
				{
					fi.WriteObject(outputStream);
				}
			}
		}

		private sealed class Finger39794Encoder : BiometricDataBlockEncoder<BiometricDataBlock>
		{
			public void Encode(BiometricDataBlock info, Stream outputStream)
			{
				if (info is org.jmrtd.lds.iso39794.FingerImageDataBlock fib)
				{
					var tlvOut = outputStream as TLVOutputStream ?? new TLVOutputStream(outputStream);
					tlvOut.WriteTag(0xA1);
					tlvOut.WriteValue(fib.GetEncoded());
				}
			}
		}

		private sealed class Finger19794Decoder : BiometricDataBlockDecoder<BiometricDataBlock>
		{
			public BiometricDataBlock Decode(Stream inputStream, StandardBiometricHeader sbh, int index, int length)
			{
				return new org.jmrtd.lds.iso19794.FingerInfo(sbh, inputStream);
			}
		}

		private sealed class Finger39794Decoder : BiometricDataBlockDecoder<BiometricDataBlock>
		{
			public BiometricDataBlock Decode(Stream inputStream, StandardBiometricHeader sbh, int index, int length)
			{
				var tlvIn = inputStream as TLVInputStream ?? new TLVInputStream(inputStream);
				int tag = tlvIn.ReadTag();
				// Expect A1
				int _ = tlvIn.ReadLength();
				return new org.jmrtd.lds.iso39794.FingerImageDataBlock(sbh, inputStream);
			}
		}

		public DG3File(List<org.jmrtd.lds.iso19794.FingerInfo> fingerInfos, bool shouldAddRandomDataIfEmpty)
			: base(99, BiometricEncodingType.ISO_19794, FromFingerInfos(fingerInfos), shouldAddRandomDataIfEmpty)
		{
		}

		public DG3File(Stream inputStream) : base(99, inputStream, false) { }

		public override ISO781611Decoder<BiometricDataBlock> GetDecoder() => DECODER;

		public override ISO781611Encoder<BiometricDataBlock> GetEncoder()
		{
			if (this.encodingType == null) return ISO_19794_ENCODER;
			switch (this.encodingType)
			{
				case BiometricEncodingType.ISO_19794: return ISO_19794_ENCODER;
				case BiometricEncodingType.ISO_39794: return ISO_39794_ENCODER;
				default: return ISO_19794_ENCODER;
			}
		}

		private static List<BiometricDataBlock> FromFingerInfos(List<org.jmrtd.lds.iso19794.FingerInfo> fingerInfos)
		{
			if (fingerInfos == null) return new List<BiometricDataBlock>();
			var records = new List<BiometricDataBlock>(fingerInfos.Count);
			foreach (var fi in fingerInfos) records.Add(fi);
			return records;
		}
	}
}
