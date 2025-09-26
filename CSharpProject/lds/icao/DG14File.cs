using System;
using System.Collections.Generic;
using System.IO;
using org.jmrtd.lds;

namespace org.jmrtd.lds.icao
{
	public class DG14File : org.jmrtd.lds.DataGroup
	{
		private HashSet<SecurityInfo> securityInfos = new HashSet<SecurityInfo>();

		public DG14File(ICollection<SecurityInfo> securityInfos) : base(110)
		{
			if (securityInfos == null) throw new System.ArgumentNullException(nameof(securityInfos));
			this.securityInfos = new HashSet<SecurityInfo>(securityInfos);
		}

		public DG14File(Stream inputStream) : base(110, inputStream)
		{
		}

		protected override void ReadContent(Stream inputStream)
		{
			securityInfos = new HashSet<SecurityInfo>();
			using var ms = new MemoryStream();
			inputStream.CopyTo(ms);
			var data = ms.ToArray();
			try
			{
				var reader = new System.Formats.Asn1.AsnReader(data, System.Formats.Asn1.AsnEncodingRules.DER);
				// DG14 content is SET OF SecurityInfo
				var set = reader.ReadSetOf();
				while (set.HasData)
				{
					var encoded = set.ReadEncodedValue();
					var si = org.jmrtd.lds.SecurityInfo.GetInstance(encoded);
					if (si != null) securityInfos.Add(si);
				}
			}
			catch
			{
				// Leave empty if parsing fails
			}
		}

		protected override void WriteContent(Stream outputStream)
		{
			// TODO: Implement ASN.1 DER set serialization when ASN1 support is added
		}

		public ICollection<SecurityInfo> GetSecurityInfos() => securityInfos;

		public override string ToString()
		{
			return $"DG14File [{securityInfos}]";
		}
	}
}
