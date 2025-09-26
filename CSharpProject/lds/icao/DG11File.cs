using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds.icao
{
	public class DG11File : org.jmrtd.lds.DataGroup
	{
		public const int TAG_LIST_TAG = 0x5C;
		public const int FULL_NAME_TAG = 0x5F1E;
		public const int OTHER_NAME_TAG = 0x5F1F;
		public const int PERSONAL_NUMBER_TAG = 0x5F20;
		public const int FULL_DATE_OF_BIRTH_TAG = 0x5F3B;
		public const int PLACE_OF_BIRTH_TAG = 0x5F21;
		public const int PERMANENT_ADDRESS_TAG = 0x5F5A;
		public const int TELEPHONE_TAG = 0x5F22;
		public const int PROFESSION_TAG = 0x5F23;
		public const int TITLE_TAG = 0x5F24;
		public const int PERSONAL_SUMMARY_TAG = 0x5F25;
		public const int PROOF_OF_CITIZENSHIP_TAG = 0x5F26;
		public const int OTHER_VALID_TD_NUMBERS_TAG = 0x5F27;
		public const int CUSTODY_INFORMATION_TAG = 0x5F28;
		public const int CONTENT_SPECIFIC_CONSTRUCTED_TAG = 0xA0;
		public const int COUNT_TAG = 0x02;

		private const string SDF = "yyyyMMdd";

		private string? nameOfHolder;
		private List<string>? otherNames;
		private string? personalNumber;
		private string? fullDateOfBirth;
		private List<string>? placeOfBirth;
		private List<string>? permanentAddress;
		private string? telephone;
		private string? profession;
		private string? title;
		private string? personalSummary;
		private byte[]? proofOfCitizenship;
		private List<string>? otherValidTDNumbers;
		private string? custodyInformation;
		private List<int>? tagPresenceList;

		public DG11File(Stream inputStream) : base(107, inputStream) { }

		public DG11File(string? nameOfHolder, List<string>? otherNames, string? personalNumber, DateTime? fullDateOfBirth, List<string>? placeOfBirth, List<string>? permanentAddress, string? telephone, string? profession, string? title, string? personalSummary, byte[]? proofOfCitizenship, List<string>? otherValidTDNumbers, string? custodyInformation)
			: this(nameOfHolder, otherNames, personalNumber, fullDateOfBirth?.ToString(SDF), placeOfBirth, permanentAddress, telephone, profession, title, personalSummary, proofOfCitizenship, otherValidTDNumbers, custodyInformation) { }

		public DG11File(string? nameOfHolder, List<string>? otherNames, string? personalNumber, string? fullDateOfBirth, List<string>? placeOfBirth, List<string>? permanentAddress, string? telephone, string? profession, string? title, string? personalSummary, byte[]? proofOfCitizenship, List<string>? otherValidTDNumbers, string? custodyInformation)
			: base(107)
		{
			this.nameOfHolder = nameOfHolder;
			this.otherNames = otherNames?.ToList() ?? new List<string>();
			this.personalNumber = personalNumber;
			this.fullDateOfBirth = fullDateOfBirth;
			this.placeOfBirth = placeOfBirth?.ToList() ?? new List<string>();
			this.permanentAddress = permanentAddress?.ToList();
			this.telephone = telephone;
			this.profession = profession;
			this.title = title;
			this.personalSummary = personalSummary;
			this.proofOfCitizenship = proofOfCitizenship;
			this.otherValidTDNumbers = otherValidTDNumbers?.ToList() ?? new List<string>();
			this.custodyInformation = custodyInformation;
		}

		public List<int> GetTagPresenceList()
		{
			if (tagPresenceList != null) return tagPresenceList;
			tagPresenceList = new List<int>(12);
			if (nameOfHolder != null) tagPresenceList.Add(FULL_NAME_TAG);
			if (otherNames != null && otherNames.Count > 0) tagPresenceList.Add(OTHER_NAME_TAG);
			if (personalNumber != null) tagPresenceList.Add(PERSONAL_NUMBER_TAG);
			if (fullDateOfBirth != null) tagPresenceList.Add(FULL_DATE_OF_BIRTH_TAG);
			if (placeOfBirth != null && placeOfBirth.Count > 0) tagPresenceList.Add(PLACE_OF_BIRTH_TAG);
			if (permanentAddress != null && permanentAddress.Count > 0) tagPresenceList.Add(PERMANENT_ADDRESS_TAG);
			if (telephone != null) tagPresenceList.Add(TELEPHONE_TAG);
			if (profession != null) tagPresenceList.Add(PROFESSION_TAG);
			if (title != null) tagPresenceList.Add(TITLE_TAG);
			if (personalSummary != null) tagPresenceList.Add(PERSONAL_SUMMARY_TAG);
			if (proofOfCitizenship != null) tagPresenceList.Add(PROOF_OF_CITIZENSHIP_TAG);
			if (otherValidTDNumbers != null && otherValidTDNumbers.Count > 0) tagPresenceList.Add(OTHER_VALID_TD_NUMBERS_TAG);
			if (custodyInformation != null) tagPresenceList.Add(CUSTODY_INFORMATION_TAG);
			return tagPresenceList;
		}

		protected override void ReadContent(Stream inputStream)
		{
			var tlvIn = inputStream as TLVInputStream ?? new TLVInputStream(inputStream);
			int tagListTag = tlvIn.ReadTag();
			if (tagListTag != TAG_LIST_TAG) throw new System.ArgumentException("Expected tag list in DG11");
			int tagListLength = tlvIn.ReadLength();
			int expectedTagCount = tagListLength / 2;
			byte[] tagListValue = tlvIn.ReadValue();
			using var tagListBytesInputStream = new MemoryStream(tagListValue);
			var tagList = new List<int>(expectedTagCount + 1);
			int bytesRead = 0;
			while (bytesRead < tagListLength)
			{
				var tlv2 = new TLVInputStream(tagListBytesInputStream);
				int t = tlv2.ReadTag();
				tagList.Add(t);
				bytesRead += 2; // tags here are two bytes in this context
			}
			foreach (int t in tagList)
			{
				ReadField(t, tlvIn);
			}
		}

		protected override void WriteContent(Stream output)
		{
			var tlvOut = output as TLVOutputStream ?? new TLVOutputStream(output);
			tlvOut.WriteTag(TAG_LIST_TAG);
			var dataOut = new BinaryWriter(tlvOut, Encoding.UTF8, leaveOpen: true);
			var tags = GetTagPresenceList();
			foreach (int tag in tags)
			{
				dataOut.Write((short)tag);
			}
			dataOut.Flush();
			tlvOut.WriteValueEnd();

			foreach (int tag in tags)
			{
				switch (tag)
				{
					case FULL_NAME_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(Encoding.UTF8.GetBytes((nameOfHolder ?? string.Empty).Trim()));
						break;
					case OTHER_NAME_TAG:
						tlvOut.WriteTag(CONTENT_SPECIFIC_CONSTRUCTED_TAG);
						tlvOut.WriteTag(COUNT_TAG);
						tlvOut.WriteByte((byte)(this.otherNames?.Count ?? 0));
						tlvOut.WriteValueEnd();
						if (otherNames != null)
						{
							foreach (var otherName in otherNames)
							{
								tlvOut.WriteTag(OTHER_NAME_TAG);
								tlvOut.WriteValue(Encoding.UTF8.GetBytes(otherName.Trim()));
							}
						}
						tlvOut.WriteValueEnd();
						break;
					case PERSONAL_NUMBER_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(Encoding.UTF8.GetBytes((personalNumber ?? string.Empty).Trim()));
						break;
					case FULL_DATE_OF_BIRTH_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(Encoding.UTF8.GetBytes(fullDateOfBirth ?? string.Empty));
						break;
					case PLACE_OF_BIRTH_TAG:
						tlvOut.WriteTag(tag);
						WriteWithSeparators(tlvOut, placeOfBirth);
						break;
					case PERMANENT_ADDRESS_TAG:
						tlvOut.WriteTag(tag);
						WriteWithSeparators(tlvOut, permanentAddress);
						break;
					case TELEPHONE_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(Encoding.UTF8.GetBytes((telephone ?? string.Empty).Trim().Replace(' ', '<')));
						break;
					case PROFESSION_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(Encoding.UTF8.GetBytes((profession ?? string.Empty).Trim().Replace(' ', '<')));
						break;
					case TITLE_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(Encoding.UTF8.GetBytes((title ?? string.Empty).Trim().Replace(' ', '<')));
						break;
					case PERSONAL_SUMMARY_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(Encoding.UTF8.GetBytes((personalSummary ?? string.Empty).Trim().Replace(' ', '<')));
						break;
					case PROOF_OF_CITIZENSHIP_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(proofOfCitizenship ?? Array.Empty<byte>());
						break;
					case OTHER_VALID_TD_NUMBERS_TAG:
						tlvOut.WriteTag(tag);
						WriteWithSeparators(tlvOut, otherValidTDNumbers?.Select(s => s.Replace(' ', '<')).ToList());
						break;
					case CUSTODY_INFORMATION_TAG:
						tlvOut.WriteTag(tag);
						tlvOut.WriteValue(Encoding.UTF8.GetBytes((custodyInformation ?? string.Empty).Trim().Replace(' ', '<')));
						break;
					default:
						throw new System.InvalidOperationException($"Unknown tag in DG11: 0x{tag:X}");
				}
			}
		}

		private static void WriteWithSeparators(TLVOutputStream tlvOut, List<string>? parts)
		{
			bool isFirstOne = true;
			if (parts != null)
			{
				foreach (var detail in parts)
				{
					if (detail == null) continue;
					if (isFirstOne) isFirstOne = false; else tlvOut.WriteByte((byte)'<');
					tlvOut.Write(Encoding.UTF8.GetBytes(detail.Trim()));
				}
			}
			tlvOut.WriteValueEnd();
		}

		private void ReadField(int expectedFieldTag, TLVInputStream tlvInputStream)
		{
			int tag = tlvInputStream.ReadTag();
			if (tag == CONTENT_SPECIFIC_CONSTRUCTED_TAG)
			{
				int _len = tlvInputStream.ReadLength();
				int countTag = tlvInputStream.ReadTag();
				if (countTag != COUNT_TAG) throw new System.ArgumentException("Expected count tag");
				int countLength = tlvInputStream.ReadLength();
				if (countLength != 1) throw new System.ArgumentException("Expected single-byte count");
				byte[] countValue = tlvInputStream.ReadValue();
				int count = countValue[0] & 0xFF;
				for (int i = 0; i < count; i++)
				{
					tag = tlvInputStream.ReadTag();
					if (tag != OTHER_NAME_TAG) throw new System.ArgumentException("Expected other-name tag");
					int _ = tlvInputStream.ReadLength();
					byte[] value = tlvInputStream.ReadValue();
					ParseOtherName(value);
				}
			}
			else
			{
				if (tag != expectedFieldTag) throw new System.ArgumentException($"Expected 0x{expectedFieldTag:X}, found 0x{tag:X}");
				int _ = tlvInputStream.ReadLength();
				byte[] value = tlvInputStream.ReadValue();
				switch (tag)
				{
					case FULL_NAME_TAG: ParseNameOfHolder(value); break;
					case OTHER_NAME_TAG: ParseOtherName(value); break;
					case PERSONAL_NUMBER_TAG: ParsePersonalNumber(value); break;
					case FULL_DATE_OF_BIRTH_TAG: ParseFullDateOfBirth(value); break;
					case PLACE_OF_BIRTH_TAG: ParsePlaceOfBirth(value); break;
					case PERMANENT_ADDRESS_TAG: ParsePermanentAddress(value); break;
					case TELEPHONE_TAG: ParseTelephone(value); break;
					case PROFESSION_TAG: ParseProfession(value); break;
					case TITLE_TAG: ParseTitle(value); break;
					case PERSONAL_SUMMARY_TAG: ParsePersonalSummary(value); break;
					case PROOF_OF_CITIZENSHIP_TAG: ParseProofOfCitizenShip(value); break;
					case OTHER_VALID_TD_NUMBERS_TAG: ParseOtherValidTDNumbers(value); break;
					case CUSTODY_INFORMATION_TAG: ParseCustodyInformation(value); break;
					default: throw new System.ArgumentException($"Unknown field tag in DG11: 0x{tag:X}");
				}
			}
		}

		private void ParseCustodyInformation(byte[] value)
		{
			custodyInformation = Encoding.UTF8.GetString(value).Trim();
		}
		private void ParseOtherValidTDNumbers(byte[] value)
		{
			string field = Encoding.UTF8.GetString(value).Trim();
			otherValidTDNumbers = new List<string>();
			foreach (var part in field.Split('<')) if (!string.IsNullOrWhiteSpace(part)) otherValidTDNumbers.Add(part.Trim());
		}
		private void ParseProofOfCitizenShip(byte[] value) { proofOfCitizenship = value; }
		private void ParsePersonalSummary(byte[] value) { personalSummary = Encoding.UTF8.GetString(value).Trim(); }
		private void ParseTitle(byte[] value) { title = Encoding.UTF8.GetString(value).Trim(); }
		private void ParseProfession(byte[] value) { profession = Encoding.UTF8.GetString(value).Trim(); }
		private void ParseTelephone(byte[] value) { telephone = Encoding.UTF8.GetString(value).Replace('<', ' ').Trim(); }
		private void ParsePermanentAddress(byte[] value) { permanentAddress = SplitOnSeparators(value); }
		private void ParsePlaceOfBirth(byte[] value) { placeOfBirth = SplitOnSeparators(value); }
		private void ParseFullDateOfBirth(byte[] value) { fullDateOfBirth = Encoding.UTF8.GetString(value); }
		private void ParseOtherName(byte[] value)
		{
			if (otherNames == null) otherNames = new List<string>();
			otherNames.Add(Encoding.UTF8.GetString(value).Trim());
		}
		private void ParsePersonalNumber(byte[] value) { personalNumber = Encoding.UTF8.GetString(value).Trim(); }
		private void ParseNameOfHolder(byte[] value) { nameOfHolder = Encoding.UTF8.GetString(value).Trim(); }

		private static List<string> SplitOnSeparators(byte[] value)
		{
			string field = Encoding.UTF8.GetString(value);
			var list = new List<string>();
			foreach (var part in field.Split('<')) if (!string.IsNullOrWhiteSpace(part)) list.Add(part.Trim());
			return list;
		}
	}
}
