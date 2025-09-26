using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Encoder for ISO/IEC 7816-11 biometric data structures
	/// </summary>
	/// <typeparam name="B">Type of BiometricDataBlock</typeparam>
	public class ISO781611Encoder<B> where B : BiometricDataBlock
	{
		private readonly BiometricDataBlockEncoder<B> bdbEncoder;

		/// <summary>
		/// Creates a new ISO781611Encoder with the specified biometric data block encoder
		/// </summary>
		/// <param name="bdbEncoder">The biometric data block encoder</param>
		public ISO781611Encoder(BiometricDataBlockEncoder<B> bdbEncoder)
		{
			this.bdbEncoder = bdbEncoder ?? throw new ArgumentNullException(nameof(bdbEncoder));
		}

		/// <summary>
		/// Encodes a CBEFFInfo to the output stream
		/// </summary>
		/// <param name="cbeffInfo">The CBEFF information to encode</param>
		/// <param name="outputStream">The output stream to write to</param>
		/// <exception cref="IOException">If an I/O error occurs</exception>
		public void Encode(CBEFFInfo<B> cbeffInfo, Stream outputStream)
		{
			if (cbeffInfo == null) throw new ArgumentNullException(nameof(cbeffInfo));
			if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));

			if (cbeffInfo is SimpleCBEFFInfo<B> simpleInfo)
			{
				WriteBITGroup(new List<CBEFFInfo<B>> { simpleInfo }, outputStream);
			}
			else if (cbeffInfo is ComplexCBEFFInfo<B> complexInfo)
			{
				WriteBITGroup(complexInfo.GetSubRecords(), outputStream);
			}
			else
			{
				throw new ArgumentException($"Unsupported CBEFFInfo type: {cbeffInfo.GetType()}");
			}
		}

		private void WriteBITGroup(IList<CBEFFInfo<B>> records, Stream outputStream)
		{
			using var tlvOut = new TLVOutputStream(outputStream);
			tlvOut.WriteTag(32609); // BIOMETRIC_INFORMATION_GROUP_TEMPLATE_TAG
			tlvOut.WriteTag(2); // BIOMETRIC_INFO_COUNT_TAG
			
			int count = records.Count;
			tlvOut.WriteValue(new byte[] { (byte)count });

			for (int index = 0; index < count; ++index)
			{
				if (records[index] is SimpleCBEFFInfo<B> simpleInfo)
				{
					WriteBIT(tlvOut, index, simpleInfo);
				}
				else
				{
					throw new ArgumentException($"Only SimpleCBEFFInfo is supported in BIT group, found: {records[index].GetType()}");
				}
			}
			tlvOut.WriteValueEnd();
		}

		private void WriteBIT(TLVOutputStream tlvOutputStream, int index, SimpleCBEFFInfo<B> cbeffInfo)
		{
			tlvOutputStream.WriteTag(32608); // BIOMETRIC_INFORMATION_TEMPLATE_TAG
			WriteBHT(tlvOutputStream, index, cbeffInfo);
			WriteBiometricDataBlock(tlvOutputStream, cbeffInfo.GetBiometricDataBlock());
			tlvOutputStream.WriteValueEnd();
		}

		private void WriteBHT(TLVOutputStream tlvOutputStream, int index, SimpleCBEFFInfo<B> cbeffInfo)
		{
			tlvOutputStream.WriteTag(161); // BIOMETRIC_HEADER_TEMPLATE_BASE_TAG + 1
			B bdb = cbeffInfo.GetBiometricDataBlock();
			StandardBiometricHeader sbh = bdb.GetStandardBiometricHeader();
			SortedDictionary<int, byte[]> elements = sbh.GetElements();

			foreach (var entry in elements)
			{
				tlvOutputStream.WriteTag(entry.Key);
				tlvOutputStream.WriteValue(entry.Value);
			}
			tlvOutputStream.WriteValueEnd();
		}

		private void WriteBiometricDataBlock(TLVOutputStream tlvOutputStream, B bdb)
		{
			tlvOutputStream.WriteTag(24366); // BIOMETRIC_DATA_BLOCK_TAG
			bdbEncoder.Encode(bdb, tlvOutputStream);
			tlvOutputStream.WriteValueEnd();
		}
	}
}
