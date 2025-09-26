using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.lds.icao
{
	public class COMFile : org.jmrtd.lds.AbstractTaggedLDSFile
	{
		private const int TAG_LIST_TAG = 0x5C; // 92
		private const int VERSION_UNICODE_TAG = 0x5F36; // 24374
		private const int VERSION_LDS_TAG = 0x5F01; // 24321

		private string versionLDS = string.Empty;
		private string updateLevelLDS = string.Empty;
		private string majorVersionUnicode = string.Empty;
		private string minorVersionUnicode = string.Empty;
		private string releaseLevelUnicode = string.Empty;
		private List<int> tagList = new List<int>();

		public COMFile(string versionLDS, string updateLevelLDS, string majorVersionUnicode, string minorVersionUnicode, string releaseLevelUnicode, int[] tagList)
			: base(0x60)
		{
			Initialize(versionLDS, updateLevelLDS, majorVersionUnicode, minorVersionUnicode, releaseLevelUnicode, tagList);
		}

		public COMFile(string ldsVer, string unicodeVer, int[] tagList)
			: base(0x60)
		{
			if (ldsVer == null) throw new System.ArgumentNullException(nameof(ldsVer));
			if (unicodeVer == null) throw new System.ArgumentNullException(nameof(unicodeVer));

			var ldsParts = ldsVer.Split('.');
			if (ldsParts.Length != 2) throw new System.ArgumentException("Could not parse LDS version. Expecting x.y");
			var unicodeParts = unicodeVer.Split('.');
			if (unicodeParts.Length != 3) throw new System.ArgumentException("Could not parse unicode version. Expecting x.y.z");

			int vMajor = int.Parse(ldsParts[0]);
			int vMinor = int.Parse(ldsParts[1]);
			int uMaj = int.Parse(unicodeParts[0]);
			int uMin = int.Parse(unicodeParts[1]);
			int uRel = int.Parse(unicodeParts[2]);

			Initialize(vMajor.ToString("00"), vMinor.ToString("00"), uMaj.ToString("00"), uMin.ToString("00"), uRel.ToString("00"), tagList);
		}

		public COMFile(Stream input) : base(0x60, input) { }

		protected override void ReadContent(Stream input)
		{
			var tlvIn = input as TLVInputStream ?? new TLVInputStream(input);

			int versionLDSTag = tlvIn.ReadTag();
			if (versionLDSTag != VERSION_LDS_TAG) throw new System.ArgumentException($"Expected VERSION_LDS_TAG (0x{VERSION_LDS_TAG:X}), found 0x{versionLDSTag:X}");
			int versionLDSLength = tlvIn.ReadLength();
			if (versionLDSLength != 4) throw new System.ArgumentException("Wrong length of LDS version object");
			byte[] versionLDSBytes = tlvIn.ReadValue();
			versionLDS = Encoding.ASCII.GetString(versionLDSBytes, 0, 2);
			updateLevelLDS = Encoding.ASCII.GetString(versionLDSBytes, 2, 2);

			int versionUnicodeTag = tlvIn.ReadTag();
			if (versionUnicodeTag != VERSION_UNICODE_TAG) throw new System.ArgumentException($"Expected VERSION_UNICODE_TAG (0x{VERSION_UNICODE_TAG:X}), found 0x{versionUnicodeTag:X}");
			int versionUnicodeLength = tlvIn.ReadLength();
			if (versionUnicodeLength != 6) throw new System.ArgumentException("Wrong length of Unicode version object");
			byte[] versionUnicodeBytes = tlvIn.ReadValue();
			majorVersionUnicode = Encoding.ASCII.GetString(versionUnicodeBytes, 0, 2);
			minorVersionUnicode = Encoding.ASCII.GetString(versionUnicodeBytes, 2, 2);
			releaseLevelUnicode = Encoding.ASCII.GetString(versionUnicodeBytes, 4, 2);

			int tagListTag = tlvIn.ReadTag();
			if (tagListTag != TAG_LIST_TAG) throw new System.ArgumentException($"Expected TAG_LIST_TAG (0x{TAG_LIST_TAG:X}), found 0x{tagListTag:X}");
			int tagListLen = tlvIn.ReadLength();
			byte[] tagBytes = tlvIn.ReadValue();
			tagList = new List<int>(tagBytes.Length);
			foreach (byte b in tagBytes) tagList.Add(b & 0xFF);
		}

		protected override void WriteContent(Stream output)
		{
			var tlvOut = output as TLVOutputStream ?? new TLVOutputStream(output);
			tlvOut.WriteTag(VERSION_LDS_TAG);
			tlvOut.WriteValue(Encoding.ASCII.GetBytes(versionLDS + updateLevelLDS));
			tlvOut.WriteTag(VERSION_UNICODE_TAG);
			tlvOut.WriteValue(Encoding.ASCII.GetBytes(majorVersionUnicode + minorVersionUnicode + releaseLevelUnicode));
			tlvOut.WriteTag(TAG_LIST_TAG);
			tlvOut.WriteLength(tagList.Count);
			foreach (int tag in tagList)
			{
				output.WriteByte((byte)tag);
			}
		}

		public string GetLDSVersion()
		{
			string ldsVersion = versionLDS + "." + updateLevelLDS;
			if (int.TryParse(versionLDS, out int major) && int.TryParse(updateLevelLDS, out int minor))
			{
				ldsVersion = major + "." + minor;
			}
			return ldsVersion;
		}

		public string GetUnicodeVersion()
		{
			string unicodeVersion = majorVersionUnicode + "." + minorVersionUnicode + "." + releaseLevelUnicode;
			if (int.TryParse(majorVersionUnicode, out int maj) && int.TryParse(minorVersionUnicode, out int min) && int.TryParse(releaseLevelUnicode, out int rel))
			{
				unicodeVersion = maj + "." + min + "." + rel;
			}
			return unicodeVersion;
		}

		public int[] GetTagList()
		{
			var result = new int[tagList.Count];
			for (int i = 0; i < tagList.Count; i++) result[i] = tagList[i];
			return result;
		}

		public void InsertTag(int tag)
		{
			if (tagList.Contains(tag)) return;
			tagList.Add(tag);
			tagList.Sort();
		}

		private void Initialize(string versionLDS, string updateLevelLDS, string majorVersionUnicode, string minorVersionUnicode, string releaseLevelUnicode, int[] tagList)
		{
			if (tagList == null) throw new System.ArgumentNullException(nameof(tagList));
			if (string.IsNullOrEmpty(versionLDS) || versionLDS.Length != 2) throw new System.ArgumentException();
			if (string.IsNullOrEmpty(updateLevelLDS) || updateLevelLDS.Length != 2) throw new System.ArgumentException();
			if (string.IsNullOrEmpty(majorVersionUnicode) || majorVersionUnicode.Length != 2) throw new System.ArgumentException();
			if (string.IsNullOrEmpty(minorVersionUnicode) || minorVersionUnicode.Length != 2) throw new System.ArgumentException();
			if (string.IsNullOrEmpty(releaseLevelUnicode) || releaseLevelUnicode.Length != 2) throw new System.ArgumentException();

			this.versionLDS = versionLDS;
			this.updateLevelLDS = updateLevelLDS;
			this.majorVersionUnicode = majorVersionUnicode;
			this.minorVersionUnicode = minorVersionUnicode;
			this.releaseLevelUnicode = releaseLevelUnicode;
			this.tagList = new List<int>(tagList.Length);
			foreach (var t in tagList) this.tagList.Add(t);
		}
	}
}
