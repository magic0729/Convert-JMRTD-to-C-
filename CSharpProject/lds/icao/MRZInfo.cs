using System;
using System.IO;

namespace org.jmrtd.lds.icao
{
	public class MRZInfo : AbstractLDSInfo
	{
		public const int DOC_TYPE_UNSPECIFIED = 0;
		public const int DOC_TYPE_ID1 = 1;
		public const int DOC_TYPE_ID2 = 2;
		public const int DOC_TYPE_ID3 = 3;

		private string documentCode = string.Empty;
		private string issuingState = string.Empty;
		private string primaryIdentifier = string.Empty;
		private string secondaryIdentifier = string.Empty;
		private string nationality = string.Empty;
		private string documentNumber = string.Empty;
		private string dateOfBirth = string.Empty;
		private string gender = string.Empty;
		private string dateOfExpiry = string.Empty;

		public MRZInfo(string str)
		{
			if (str == null) throw new ArgumentNullException(nameof(str));
			var text = str.Replace("\r", string.Empty);
			if (text.Contains("\n"))
			{
				ParseFromLines(text.Split('\n'));
			}
			else
			{
				// Single string; try to split based on known lengths (TD3: 44+44, TD2: 36+36, TD1: 30*3)
				if (text.Length == 88) ParseFromLines(new[] { text.Substring(0, 44), text.Substring(44, 44) });
				else if (text.Length == 72) ParseFromLines(new[] { text.Substring(0, 36), text.Substring(36, 36) });
				else if (text.Length == 90) ParseFromLines(new[] { text.Substring(0, 30), text.Substring(30, 30), text.Substring(60, 30) });
				else throw new ArgumentException("Unsupported MRZ format length");
			}
		}

		public MRZInfo(Stream inputStream, int length)
		{
			using var reader = new StreamReader(inputStream, leaveOpen: true);
			char[] buf = new char[length];
			int read = reader.Read(buf, 0, length);
			var str = new string(buf, 0, read);
			var lines = str.Replace("\r", string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries);
			ParseFromLines(lines);
		}

		public string GetDocumentNumber() => documentNumber;
		public string GetDateOfBirth() => dateOfBirth;
		public string GetDateOfExpiry() => dateOfExpiry;

		public static char CheckDigit(string str)
		{
			if (str == null) throw new ArgumentNullException(nameof(str));
			int[] weights = { 7, 3, 1 };
			int sum = 0;
			for (int i = 0; i < str.Length; i++)
			{
				int v = DecodeMRZChar(str[i]);
				sum += v * weights[i % 3];
			}
			return (char)('0' + (sum % 10));
		}

		public override void WriteObject(Stream outputStream)
		{
			using var writer = new StreamWriter(outputStream, leaveOpen: true);
			// Write in TD3 (2x44) format by default
			string name = (primaryIdentifier + "<<" + secondaryIdentifier).Replace(' ', '<');
			string line1 = PadRight(documentCode, 2) + PadRight(issuingState, 3) + PadRight(name, 39);
			line1 = line1.Substring(0, 44);
			string docNumField = PadRight(documentNumber, 9);
			char docNumCD = CheckDigit(docNumField);
			char dobCD = CheckDigit(dateOfBirth);
			char doeCD = CheckDigit(dateOfExpiry);
			string optionalData = new string('<', 14);
			string compositeSource = docNumField + docNumCD + dateOfBirth + dobCD + dateOfExpiry + doeCD + optionalData;
			char compCD = CheckDigit(compositeSource);
			string line2 = docNumField + docNumCD + PadRight(nationality, 3) + dateOfBirth + dobCD + gender + dateOfExpiry + doeCD + optionalData + compCD;
			line2 = line2.Substring(0, 44);
			writer.Write(line1);
			writer.Write("\n");
			writer.Write(line2);
			writer.Flush();
		}

		public override string ToString()
		{
			return $"MRZInfo[{documentCode}:{documentNumber}]";
		}

		private static int DecodeMRZChar(char ch)
		{
			if (ch >= '0' && ch <= '9') return ch - '0';
			if (ch >= 'A' && ch <= 'Z') return ch - 'A' + 10;
			if (ch == '<') return 0;
			// treat other as 0
			return 0;
		}

		private static string PadRight(string s, int len)
		{
			s ??= string.Empty;
			if (s.Length >= len) return s.Substring(0, len);
			return s + new string('<', len - s.Length);
		}

		private void ParseFromLines(string[] lines)
		{
			if (lines.Length == 2)
			{
				// TD3 format
				string l1 = lines[0].PadRight(44, '<');
				string l2 = lines[1].PadRight(44, '<');
				documentCode = l1.Substring(0, 2);
				issuingState = l1.Substring(2, 3);
				string names = l1.Substring(5, 39).Trim('<');
				var nameParts = names.Split(new[] { "<<" }, StringSplitOptions.None);
				primaryIdentifier = nameParts.Length > 0 ? nameParts[0].Replace('<', ' ').Trim() : string.Empty;
				secondaryIdentifier = nameParts.Length > 1 ? nameParts[1].Replace('<', ' ').Trim() : string.Empty;
				documentNumber = l2.Substring(0, 9).Replace('<', ' ').Trim();
				// skip doc num check digit at pos 9
				nationality = l2.Substring(10, 3);
				dateOfBirth = l2.Substring(13, 6);
				gender = l2.Substring(20, 1);
				dateOfExpiry = l2.Substring(21, 6);
			}
			else if (lines.Length == 3)
			{
				// TD1 format (simplified parsing)
				string l1 = lines[0].PadRight(30, '<');
				string l2 = lines[1].PadRight(30, '<');
				string l3 = lines[2].PadRight(30, '<');
				documentCode = l1.Substring(0, 2);
				issuingState = l1.Substring(2, 3);
				documentNumber = l1.Substring(5, 9).Replace('<', ' ').Trim();
				nationality = l2.Substring(15, 3);
				dateOfBirth = l2.Substring(0, 6);
				gender = l2.Substring(7, 1);
				dateOfExpiry = l2.Substring(8, 6);
				string names = l3.Substring(0, 30).Trim('<');
				var nameParts = names.Split(new[] { "<<" }, StringSplitOptions.None);
				primaryIdentifier = nameParts.Length > 0 ? nameParts[0].Replace('<', ' ').Trim() : string.Empty;
				secondaryIdentifier = nameParts.Length > 1 ? nameParts[1].Replace('<', ' ').Trim() : string.Empty;
			}
			else
			{
				throw new ArgumentException("Unsupported MRZ format");
			}
		}
	}
}