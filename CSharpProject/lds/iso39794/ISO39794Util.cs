using System;

namespace org.jmrtd.lds.iso39794
{
	public static class ISO39794Util
	{
		public const int BDB_TAG = 0x5F66; // 24366 (typical, may vary by modality)
		public const int BDB_CONSTRUCTED_TAG = 0x7F66; // 32558

		public static int Clamp(int value, int min, int max)
		{
			if (value < min) return min;
			if (value > max) return max;
			return value;
		}
	}
}
