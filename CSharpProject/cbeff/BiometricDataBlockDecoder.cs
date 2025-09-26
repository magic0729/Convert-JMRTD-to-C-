using System;
using System.IO;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Interface for decoding biometric data blocks from streams
	/// </summary>
	/// <typeparam name="B">Type of BiometricDataBlock</typeparam>
	public interface BiometricDataBlockDecoder<B> where B : BiometricDataBlock
	{
		/// <summary>
		/// Decodes a biometric data block from the input stream
		/// </summary>
		/// <param name="inputStream">The input stream to read from</param>
		/// <param name="sbh">The standard biometric header</param>
		/// <param name="index">The index of the biometric data block</param>
		/// <param name="length">The length of the data to read</param>
		/// <returns>The decoded biometric data block</returns>
		/// <exception cref="IOException">If an I/O error occurs</exception>
		B Decode(Stream inputStream, StandardBiometricHeader sbh, int index, int length);
	}
}
