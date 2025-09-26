using System;
using System.Collections.Generic;
using System.Linq;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Complex CBEFF information containing multiple sub-records
	/// </summary>
	/// <typeparam name="R">Type of BiometricDataBlock</typeparam>
	public class ComplexCBEFFInfo<R> : CBEFFInfo<R> where R : BiometricDataBlock
	{
		private readonly List<CBEFFInfo<R>> subRecords;

		/// <summary>
		/// Creates a new ComplexCBEFFInfo with an empty list of sub-records
		/// </summary>
		public ComplexCBEFFInfo()
		{
			subRecords = new List<CBEFFInfo<R>>();
		}

		/// <summary>
		/// Creates a new ComplexCBEFFInfo with the given sub-records
		/// </summary>
		/// <param name="subRecords">The initial sub-records</param>
		public ComplexCBEFFInfo(IEnumerable<CBEFFInfo<R>> subRecords)
		{
			this.subRecords = new List<CBEFFInfo<R>>(subRecords ?? throw new ArgumentNullException(nameof(subRecords)));
		}

		/// <summary>
		/// Gets a copy of the sub-records list
		/// </summary>
		/// <returns>List of sub-records</returns>
		public List<CBEFFInfo<R>> GetSubRecords()
		{
			return new List<CBEFFInfo<R>>(subRecords);
		}

		/// <summary>
		/// Adds a sub-record to this complex CBEFF info
		/// </summary>
		/// <param name="subRecord">The sub-record to add</param>
		public void Add(CBEFFInfo<R> subRecord)
		{
			if (subRecord == null) throw new ArgumentNullException(nameof(subRecord));
			subRecords.Add(subRecord);
		}

		/// <summary>
		/// Adds all sub-records from the given collection
		/// </summary>
		/// <param name="subRecords">The sub-records to add</param>
		public void AddAll(IEnumerable<CBEFFInfo<R>> subRecords)
		{
			if (subRecords == null) throw new ArgumentNullException(nameof(subRecords));
			this.subRecords.AddRange(subRecords);
		}

		/// <summary>
		/// Removes a sub-record at the specified index
		/// </summary>
		/// <param name="index">The index of the sub-record to remove</param>
		public void Remove(int index)
		{
			if (index < 0 || index >= subRecords.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			subRecords.RemoveAt(index);
		}

		/// <summary>
		/// Gets the number of sub-records
		/// </summary>
		public int Count => subRecords.Count;

		/// <summary>
		/// Gets a sub-record at the specified index
		/// </summary>
		/// <param name="index">The index</param>
		/// <returns>The sub-record at the index</returns>
		public CBEFFInfo<R> Get(int index)
		{
			if (index < 0 || index >= subRecords.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			return subRecords[index];
		}

		/// <summary>
		/// Checks if this complex CBEFF info is empty
		/// </summary>
		public bool IsEmpty => subRecords.Count == 0;

		public override bool Equals(object? obj)
		{
			if (obj == null) return false;
			if (obj == this) return true;
			if (GetType() != obj.GetType()) return false;

			ComplexCBEFFInfo<R> other = (ComplexCBEFFInfo<R>)obj;
			return subRecords.SequenceEqual(other.subRecords);
		}

		public override int GetHashCode()
		{
			return 7 * subRecords.GetHashCode() + 11;
		}

		public override string ToString()
		{
			return $"ComplexCBEFFInfo [subRecords: {subRecords.Count}]";
		}
	}
}
