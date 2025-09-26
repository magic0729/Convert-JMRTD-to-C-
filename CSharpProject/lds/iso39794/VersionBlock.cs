using System;

namespace org.jmrtd.lds.iso39794
{
	public class VersionBlock
	{
		public int Major { get; }
		public int Minor { get; }
		public int Patch { get; }

		public VersionBlock(int major, int minor, int patch)
		{
			Major = major; Minor = minor; Patch = patch;
		}

		public override string ToString() => $"{Major}.{Minor}.{Patch}";
	}
}
