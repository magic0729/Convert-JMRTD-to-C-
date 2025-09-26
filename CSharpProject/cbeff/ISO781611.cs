using System;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Interface defining constants for ISO/IEC 7816-11 biometric data structures
	/// </summary>
	public interface ISO781611
	{
		// Template tags
		public const int BIOMETRIC_INFORMATION_GROUP_TEMPLATE_TAG = 32609;
		public const int BIOMETRIC_INFORMATION_TEMPLATE_TAG = 32608;
		public const int BIOMETRIC_INFO_COUNT_TAG = 2;
		public const int BIOMETRIC_HEADER_TEMPLATE_BASE_TAG = -95;
		public const int BIOMETRIC_DATA_BLOCK_TAG = 24366;
		public const int BIOMETRIC_DATA_BLOCK_CONSTRUCTED_TAG = 32558;
		public const int DISCRETIONARY_DATA_FOR_PAYLOAD_TAG = 83;
		public const int DISCRETIONARY_DATA_FOR_PAYLOAD_CONSTRUCTED_TAG = 115;
		public const int SMT_TAG = 125;
		public const int SMT_DO_PV = 129;
		public const int SMT_DO_CG = 133;
		public const int SMT_DO_CC = 142;
		public const int SMT_DO_DS = 158;

		// Header element tags
		public const int PATRON_HEADER_VERSION_TAG = 128;
		public const int BIOMETRIC_TYPE_TAG = 129;
		public const int BIOMETRIC_SUBTYPE_TAG = 130;
		public const int CREATION_DATE_AND_TIME_TAG = 131;
		public const int VALIDITY_PERIOD_TAG = 133;
		public const int CREATOR_OF_BIOMETRIC_REFERENCE_DATA = 134;
		public const int FORMAT_OWNER_TAG = 135;
		public const int FORMAT_TYPE_TAG = 136;
	}
}
