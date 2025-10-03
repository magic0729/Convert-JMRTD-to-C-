using System;
using System.Runtime.Serialization;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd
{
	public interface BACKeySpec : AccessKeySpec
	{
		string DocumentNumber { get; }
		string DateOfBirth { get; }
		string DateOfExpiry { get; }
	}

	[Serializable]
	public class BACKeySpecImpl : BACKeySpec, IBACKeySpec
	{
		public string DocumentNumber { get; }
		public string DateOfBirth { get; }
		public string DateOfExpiry { get; }
		public string Algorithm => "BAC";
		public byte[] Key { get; }

		public BACKeySpecImpl(string documentNumber, string dateOfBirth, string dateOfExpiry)
		{
		DocumentNumber = documentNumber ?? throw new ArgumentNullException(nameof(documentNumber));
		DateOfBirth = dateOfBirth ?? throw new ArgumentNullException(nameof(dateOfBirth));
		DateOfExpiry = dateOfExpiry ?? throw new ArgumentNullException(nameof(dateOfExpiry));

		// Compute the BAC key from MRZ data
		Key = Util.ComputeKeySeed(documentNumber, dateOfBirth, dateOfExpiry, "SHA-1", doTruncate: true);
		}

		public byte[] GetKey() => Key;

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("DocumentNumber", DocumentNumber);
			info.AddValue("DateOfBirth", DateOfBirth);
			info.AddValue("DateOfExpiry", DateOfExpiry);
		}

		public override string ToString()
		{
			return $"BACKeySpec[doc={DocumentNumber}, dob={DateOfBirth}, doe={DateOfExpiry}]";
		}
	}
}

