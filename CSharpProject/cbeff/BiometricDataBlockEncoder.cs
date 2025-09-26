using System;
using System.IO;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Interface for encoding biometric data blocks to streams
	/// </summary>
	/// <typeparam name="B">Type of BiometricDataBlock</typeparam>
	public interface BiometricDataBlockEncoder<B> where B : BiometricDataBlock
	{
		/// <summary>
		/// Encodes a biometric data block to the output stream
		/// </summary>
		/// <param name="bdb">The biometric data block to encode</param>
		/// <param name="outputStream">The output stream to write to</param>
		/// <exception cref="IOException">If an I/O error occurs</exception>
		void Encode(B bdb, Stream outputStream);
	}
}
