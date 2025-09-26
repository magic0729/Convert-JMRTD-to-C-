using System;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Interface for CBEFF (Common Biometric Exchange Formats Framework) information
	/// </summary>
	/// <typeparam name="R">Type of BiometricDataBlock</typeparam>
	public interface CBEFFInfo<R> where R : BiometricDataBlock
	{
		// Biometric type constants
		public const int BIOMETRIC_TYPE_NO_INFORMATION_GIVEN = 0;
		public const int BIOMETRIC_TYPE_MULTIPLE_BIOMETRICS_USED = 1;
		public const int BIOMETRIC_TYPE_FACIAL_FEATURES = 2;
		public const int BIOMETRIC_TYPE_VOICE = 4;
		public const int BIOMETRIC_TYPE_FINGERPRINT = 8;
		public const int BIOMETRIC_TYPE_IRIS = 16;
		public const int BIOMETRIC_TYPE_RETINA = 32;
		public const int BIOMETRIC_TYPE_HAND_GEOMETRY = 64;
		public const int BIOMETRIC_TYPE_SIGNATURE_DYNAMICS = 128;
		public const int BIOMETRIC_TYPE_KEYSTROKE_DYNAMICS = 256;
		public const int BIOMETRIC_TYPE_LIP_MOVEMENT = 512;
		public const int BIOMETRIC_TYPE_THERMAL_FACE_IMAGE = 1024;
		public const int BIOMETRIC_TYPE_THERMAL_HAND_IMAGE = 2048;
		public const int BIOMETRIC_TYPE_GAIT = 4096;
		public const int BIOMETRIC_TYPE_BODY_ODOR = 8192;
		public const int BIOMETRIC_TYPE_DNA = 16384;
		public const int BIOMETRIC_TYPE_EAR_SHAPE = 32768;
		public const int BIOMETRIC_TYPE_FINGER_GEOMETRY = 65536;
		public const int BIOMETRIC_TYPE_PALM_PRINT = 131072;
		public const int BIOMETRIC_TYPE_VEIN_PATTERN = 262144;
		public const int BIOMETRIC_TYPE_FOOT_PRINT = 524288;

		// Biometric subtype constants
		public const int BIOMETRIC_SUBTYPE_NONE = 0;
		public const int BIOMETRIC_SUBTYPE_MASK_RIGHT = 1;
		public const int BIOMETRIC_SUBTYPE_MASK_LEFT = 2;
		public const int BIOMETRIC_SUBTYPE_MASK_THUMB = 4;
		public const int BIOMETRIC_SUBTYPE_MASK_POINTER_FINGER = 8;
		public const int BIOMETRIC_SUBTYPE_MASK_MIDDLE_FINGER = 12;
		public const int BIOMETRIC_SUBTYPE_MASK_RING_FINGER = 16;
		public const int BIOMETRIC_SUBTYPE_MASK_LITTLE_FINGER = 20;
	}
}
