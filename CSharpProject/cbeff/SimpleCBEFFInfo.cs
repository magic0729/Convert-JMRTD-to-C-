using System;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Simple CBEFF information containing a single biometric data block
	/// </summary>
	/// <typeparam name="B">Type of BiometricDataBlock</typeparam>
	public class SimpleCBEFFInfo<B> : CBEFFInfo<B> where B : BiometricDataBlock
	{
		private readonly B bdb;

		/// <summary>
		/// Creates a new SimpleCBEFFInfo with the given biometric data block
		/// </summary>
		/// <param name="bdb">The biometric data block</param>
		public SimpleCBEFFInfo(B bdb)
		{
			this.bdb = bdb ?? throw new ArgumentNullException(nameof(bdb));
		}

		/// <summary>
		/// Gets the biometric data block
		/// </summary>
		/// <returns>The biometric data block</returns>
		public B GetBiometricDataBlock()
		{
			return bdb;
		}

		public override bool Equals(object? obj)
		{
			if (obj == null) return false;
			if (obj == this) return true;
			if (GetType() != obj.GetType()) return false;

			SimpleCBEFFInfo<B> other = (SimpleCBEFFInfo<B>)obj;
			return Equals(bdb, other.bdb);
		}

		public override int GetHashCode()
		{
			return bdb?.GetHashCode() ?? 0;
		}

		public override string ToString()
		{
			return $"SimpleCBEFFInfo [bdb: {bdb}]";
		}
	}
}
