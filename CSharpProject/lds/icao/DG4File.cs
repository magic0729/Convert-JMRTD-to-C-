using System;
using System.Collections.Generic;
using System.IO;
using org.jmrtd.CustomJavaAPI;
using org.jmrtd.cbeff;

namespace org.jmrtd.lds.icao
{
	public class DG4File : org.jmrtd.lds.CBEFFDataGroup
	{
		private static readonly ISO781611Decoder<BiometricDataBlock> DECODER = new ISO781611Decoder<BiometricDataBlock>(GetDecoderMap());
		private static readonly ISO781611Encoder<BiometricDataBlock> ISO_19794_ENCODER = new ISO781611Encoder<BiometricDataBlock>(new Iris19794Encoder());
		private static readonly ISO781611Encoder<BiometricDataBlock> ISO_39794_ENCODER = new ISO781611Encoder<BiometricDataBlock>(new Iris39794Encoder());

		private static Dictionary<int, BiometricDataBlockDecoder<BiometricDataBlock>> GetDecoderMap()
		{
			var decoders = new Dictionary<int, BiometricDataBlockDecoder<BiometricDataBlock>>();
			decoders[24366] = new Iris19794Decoder();
			decoders[32558] = new Iris39794Decoder();
			return decoders;
		}

		private sealed class Iris19794Encoder : BiometricDataBlockEncoder<BiometricDataBlock>
		{
			public void Encode(BiometricDataBlock info, Stream outputStream)
			{
				if (info is org.jmrtd.lds.iso19794.IrisInfo ii)
				{
					ii.WriteObject(outputStream);
				}
			}
		}

		private sealed class Iris39794Encoder : BiometricDataBlockEncoder<BiometricDataBlock>
		{
			public void Encode(BiometricDataBlock info, Stream outputStream)
			{
				if (info is org.jmrtd.lds.iso39794.IrisImageDataBlock iib)
				{
					var tlvOut = outputStream as TLVOutputStream ?? new TLVOutputStream(outputStream);
					tlvOut.WriteTag(0xA1);
					tlvOut.WriteValue(iib.GetEncoded());
				}
			}
		}

		private sealed class Iris19794Decoder : BiometricDataBlockDecoder<BiometricDataBlock>
		{
			public BiometricDataBlock Decode(Stream inputStream, StandardBiometricHeader sbh, int index, int length)
			{
				return new org.jmrtd.lds.iso19794.IrisInfo(sbh, inputStream);
			}
		}

		private sealed class Iris39794Decoder : BiometricDataBlockDecoder<BiometricDataBlock>
		{
			public BiometricDataBlock Decode(Stream inputStream, StandardBiometricHeader sbh, int index, int length)
			{
				var tlvIn = inputStream as TLVInputStream ?? new TLVInputStream(inputStream);
				int tag = tlvIn.ReadTag(); // expect A1
				int _ = tlvIn.ReadLength();
				return new org.jmrtd.lds.iso39794.IrisImageDataBlock(sbh, inputStream);
			}
		}

		public DG4File(List<org.jmrtd.lds.iso19794.IrisInfo> irisInfos, bool shouldAddRandomDataIfEmpty)
			: base(118, BiometricEncodingType.ISO_19794, FromIrisInfos(irisInfos), shouldAddRandomDataIfEmpty)
		{
		}

		public DG4File(Stream inputStream) : base(118, inputStream, false) { }

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

		private static List<BiometricDataBlock> FromIrisInfos(List<org.jmrtd.lds.iso19794.IrisInfo> irisInfos)
		{
			if (irisInfos == null) return new List<BiometricDataBlock>();
			var records = new List<BiometricDataBlock>(irisInfos.Count);
			foreach (var ii in irisInfos) records.Add(ii);
			return records;
		}
	}
}
