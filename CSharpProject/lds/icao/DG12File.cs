using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds.icao
{
	public class DG12File : org.jmrtd.lds.DataGroup
	{
		private const int TAG_LIST_TAG = 0x5C;
		public const int ISSUING_AUTHORITY_TAG = 0x5F29; // 24345
		public const int DATE_OF_ISSUE_TAG = 0x5F36; // 24358
		public const int NAME_OF_OTHER_PERSON_TAG = 0x5F2A; // 24346
		public const int ENDORSEMENTS_AND_OBSERVATIONS_TAG = 0x5F2B; // 24347
		public const int TAX_OR_EXIT_REQUIREMENTS_TAG = 0x5F2C; // 24348
		public const int IMAGE_OF_FRONT_TAG = 0x5F2D; // 24349
		public const int IMAGE_OF_REAR_TAG = 0x5F2E; // 24350
		public const int DATE_AND_TIME_OF_PERSONALIZATION_TAG = 0x5F75; // 24405
		public const int PERSONALIZATION_SYSTEM_SERIAL_NUMBER_TAG = 0x5F76; // 24406
		private const int CONTENT_SPECIFIC_CONSTRUCTED_TAG = 0xA0;
		private const int COUNT_TAG = 0x02;

		private string? issuingAuthority;
		private string? dateOfIssue;
		private List<string>? namesOfOtherPersons;
		private string? endorsementsAndObservations;
		private string? taxOrExitRequirements;
		private byte[]? imageOfFront;
		private byte[]? imageOfRear;
		private string? dateAndTimeOfPersonalization;
		private string? personalizationSystemSerialNumber;
		private List<int>? tagPresenceList;

		public DG12File(string? issuingAuthority, string? dateOfIssue, List<string>? namesOfOtherPersons, string? endorsementsAndObservations, string? taxOrExitRequirements, byte[]? imageOfFront, byte[]? imageOfRear, string? dateAndTimeOfPersonalization, string? personalizationSystemSerialNumber)
			: base(108)
		{
			this.issuingAuthority = issuingAuthority;
			this.dateOfIssue = dateOfIssue;
			this.namesOfOtherPersons = namesOfOtherPersons?.ToList() ?? new List<string>();
			this.endorsementsAndObservations = endorsementsAndObservations;
			this.taxOrExitRequirements = taxOrExitRequirements;
			this.imageOfFront = imageOfFront;
			this.imageOfRear = imageOfRear;
			this.dateAndTimeOfPersonalization = dateAndTimeOfPersonalization;
			this.personalizationSystemSerialNumber = personalizationSystemSerialNumber;
		}

		public DG12File(Stream inputStream) : base(108, inputStream) { }

		public List<int> GetTagPresenceList()
		{
			if (tagPresenceList != null) return tagPresenceList;
			tagPresenceList = new List<int>(10);
			if (issuingAuthority != null) tagPresenceList.Add(ISSUING_AUTHORITY_TAG);
			if (dateOfIssue != null) tagPresenceList.Add(DATE_OF_ISSUE_TAG);
			if (namesOfOtherPersons != null && namesOfOtherPersons.Count > 0) tagPresenceList.Add(NAME_OF_OTHER_PERSON_TAG);
			if (endorsementsAndObservations != null) tagPresenceList.Add(ENDORSEMENTS_AND_OBSERVATIONS_TAG);
			if (taxOrExitRequirements != null) tagPresenceList.Add(TAX_OR_EXIT_REQUIREMENTS_TAG);
			if (imageOfFront != null) tagPresenceList.Add(IMAGE_OF_FRONT_TAG);
			if (imageOfRear != null) tagPresenceList.Add(IMAGE_OF_REAR_TAG);
			if (dateAndTimeOfPersonalization != null) tagPresenceList.Add(DATE_AND_TIME_OF_PERSONALIZATION_TAG);
			if (personalizationSystemSerialNumber != null) tagPresenceList.Add(PERSONALIZATION_SYSTEM_SERIAL_NUMBER_TAG);
			return tagPresenceList;
		}

		protected override void ReadContent(Stream inputStream)
		{
			var tlvIn = inputStream as TLVInputStream ?? new TLVInputStream(inputStream);
			int tagListTag = tlvIn.ReadTag();
			if (tagListTag != TAG_LIST_TAG) throw new System.ArgumentException("Expected tag list in DG12");
			int tagListLength = tlvIn.ReadLength();
			byte[] tagListValue = tlvIn.ReadValue();
			using var ms = new MemoryStream(tagListValue);
			int bytesRead = 0;
			var tags = new List<int>();
			while (bytesRead < tagListLength)
			{
				var tlv2 = new TLVInputStream(ms);
				int t = tlv2.ReadTag();
				tags.Add(t);
				bytesRead += 2;
			}
			foreach (var t in tags)
			{
				ReadField(t, tlvIn);
			}
		}

		protected override void WriteContent(Stream outputStream)
		{
			var tlvOut = outputStream as TLVOutputStream ?? new TLVOutputStream(outputStream);
			tlvOut.WriteTag(TAG_LIST_TAG);
			var bw = new BinaryWriter(tlvOut, Encoding.UTF8, leaveOpen: true);
			foreach (var tag in GetTagPresenceList()) bw.Write((short)tag);
			bw.Flush();
			tlvOut.WriteValueEnd();

			foreach (var tag in GetTagPresenceList())
			{
				switch (tag)
				{
					case var _ when tag == ISSUING_AUTHORITY_TAG:
						tlvOut.WriteTag(tag); tlvOut.WriteValue(Encoding.UTF8.GetBytes((issuingAuthority ?? string.Empty).Trim())); break;
					case var _ when tag == DATE_OF_ISSUE_TAG:
						tlvOut.WriteTag(tag); tlvOut.WriteValue(Encoding.UTF8.GetBytes(dateOfIssue ?? string.Empty)); break;
					case var _ when tag == NAME_OF_OTHER_PERSON_TAG:
						tlvOut.WriteTag(CONTENT_SPECIFIC_CONSTRUCTED_TAG);
						tlvOut.WriteTag(COUNT_TAG);
						tlvOut.WriteByte((byte)(namesOfOtherPersons?.Count ?? 0));
						tlvOut.WriteValueEnd();
						if (namesOfOtherPersons != null)
						{
							foreach (var n in namesOfOtherPersons)
							{
								tlvOut.WriteTag(NAME_OF_OTHER_PERSON_TAG);
								tlvOut.WriteValue(Encoding.UTF8.GetBytes(n.Trim()));
							}
						}
						tlvOut.WriteValueEnd();
						break;
					case var _ when tag == ENDORSEMENTS_AND_OBSERVATIONS_TAG:
						tlvOut.WriteTag(tag); tlvOut.WriteValue(Encoding.UTF8.GetBytes((endorsementsAndObservations ?? string.Empty).Trim())); break;
					case var _ when tag == TAX_OR_EXIT_REQUIREMENTS_TAG:
						tlvOut.WriteTag(tag); tlvOut.WriteValue(Encoding.UTF8.GetBytes((taxOrExitRequirements ?? string.Empty).Trim())); break;
					case var _ when tag == IMAGE_OF_FRONT_TAG:
						tlvOut.WriteTag(tag); tlvOut.WriteValue(imageOfFront ?? Array.Empty<byte>()); break;
					case var _ when tag == IMAGE_OF_REAR_TAG:
						tlvOut.WriteTag(tag); tlvOut.WriteValue(imageOfRear ?? Array.Empty<byte>()); break;
					case var _ when tag == DATE_AND_TIME_OF_PERSONALIZATION_TAG:
						tlvOut.WriteTag(tag); tlvOut.WriteValue(Encoding.UTF8.GetBytes(dateAndTimeOfPersonalization ?? string.Empty)); break;
					case var _ when tag == PERSONALIZATION_SYSTEM_SERIAL_NUMBER_TAG:
						tlvOut.WriteTag(tag); tlvOut.WriteValue(Encoding.UTF8.GetBytes((personalizationSystemSerialNumber ?? string.Empty).Trim())); break;
					default: throw new System.ArgumentException($"Unknown field tag in DG12: 0x{tag:X}");
				}
			}
		}

		private void ReadField(int expectedFieldTag, TLVInputStream tlvIn)
		{
			int tag = tlvIn.ReadTag();
			if (tag == CONTENT_SPECIFIC_CONSTRUCTED_TAG)
			{
				int _len = tlvIn.ReadLength();
				int countTag = tlvIn.ReadTag();
				if (countTag != COUNT_TAG) throw new System.ArgumentException("Expected count tag");
				int countLength = tlvIn.ReadLength();
				if (countLength != 1) throw new System.ArgumentException("Expected single-byte count");
				int count = tlvIn.ReadValue()[0] & 0xFF;
				for (int i = 0; i < count; i++)
				{
					tag = tlvIn.ReadTag();
					if (tag != NAME_OF_OTHER_PERSON_TAG) throw new System.ArgumentException("Expected other-person tag");
					int _ = tlvIn.ReadLength();
					byte[] value = tlvIn.ReadValue();
					ParseNameOfOtherPerson(value);
				}
			}
			else
			{
				if (tag != expectedFieldTag) throw new System.ArgumentException($"Expected 0x{expectedFieldTag:X}, but found 0x{tag:X}");
				int _ = tlvIn.ReadLength();
				byte[] value = tlvIn.ReadValue();
				switch (tag)
				{
					case ISSUING_AUTHORITY_TAG: ParseIssuingAuthority(value); break;
					case DATE_OF_ISSUE_TAG: ParseDateOfIssue(value); break;
					case NAME_OF_OTHER_PERSON_TAG: ParseNameOfOtherPerson(value); break;
					case ENDORSEMENTS_AND_OBSERVATIONS_TAG: ParseEndorsementsAndObservations(value); break;
					case TAX_OR_EXIT_REQUIREMENTS_TAG: ParseTaxOrExitRequirements(value); break;
					case IMAGE_OF_FRONT_TAG: imageOfFront = value; break;
					case IMAGE_OF_REAR_TAG: imageOfRear = value; break;
					case DATE_AND_TIME_OF_PERSONALIZATION_TAG: dateAndTimeOfPersonalization = Encoding.UTF8.GetString(value).Trim(); break;
					case PERSONALIZATION_SYSTEM_SERIAL_NUMBER_TAG: personalizationSystemSerialNumber = Encoding.UTF8.GetString(value).Trim(); break;
					default: throw new System.ArgumentException($"Unknown field tag in DG12: 0x{tag:X}");
				}
			}
		}

		private void ParseTaxOrExitRequirements(byte[] value) => taxOrExitRequirements = Encoding.UTF8.GetString(value).Trim();
		private void ParseEndorsementsAndObservations(byte[] value) => endorsementsAndObservations = Encoding.UTF8.GetString(value).Trim();
		private void ParseNameOfOtherPerson(byte[] value)
		{
			if (namesOfOtherPersons == null) namesOfOtherPersons = new List<string>();
			namesOfOtherPersons.Add(Encoding.UTF8.GetString(value).Trim());
		}
		private void ParseDateOfIssue(byte[] value) => dateOfIssue = Encoding.UTF8.GetString(value).Trim();
		private void ParseIssuingAuthority(byte[] value) => issuingAuthority = Encoding.UTF8.GetString(value).Trim();
	}
}
