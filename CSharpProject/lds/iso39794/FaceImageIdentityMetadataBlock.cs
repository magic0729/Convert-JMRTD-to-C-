using System;
using System.Text;

namespace org.jmrtd.lds.iso39794
{
	public class FaceImageIdentityMetadataBlock : Block
	{
		public string SubjectId { get; }
		public string IssuingAuthority { get; }

		public FaceImageIdentityMetadataBlock(string subjectId, string issuingAuthority)
		{
			SubjectId = subjectId ?? string.Empty;
			IssuingAuthority = issuingAuthority ?? string.Empty;
			Length = Encoding.UTF8.GetByteCount(SubjectId) + Encoding.UTF8.GetByteCount(IssuingAuthority) + 2;
		}

	public override byte[] GetEncoded()
	{
		var sid = Encoding.UTF8.GetBytes(SubjectId);
		var ia = Encoding.UTF8.GetBytes(IssuingAuthority);
		var result = new byte[sid.Length + ia.Length + 2];
		result[0] = (byte)sid.Length;
		Buffer.BlockCopy(sid, 0, result, 1, sid.Length);
		result[1 + sid.Length] = (byte)ia.Length;
		Buffer.BlockCopy(ia, 0, result, 2 + sid.Length, ia.Length);
		return result;
	}

	internal override object GetASN1Object()
	{
		// TODO: Implement ASN1Util when ASN1 support is added
		return new object();
	}
	}
}
