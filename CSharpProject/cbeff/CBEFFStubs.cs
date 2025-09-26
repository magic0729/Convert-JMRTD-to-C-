using System;
using System.Collections.Generic;
using System.Linq;
using org.jmrtd.CustomJavaAPI;

namespace org.jmrtd.cbeff
{
	/// <summary>
	/// Interface for biometric data blocks as defined in CBEFF standard
	/// </summary>
	public interface BiometricDataBlock : ISerializable
	{
		StandardBiometricHeader GetStandardBiometricHeader();
	}

	/// <summary>
	/// Enumeration of biometric encoding types supported by CBEFF
	/// </summary>
	public enum BiometricEncodingType
	{
		UNKNOWN,
		ISO_19794,
		ISO_39794
	}

	/// <summary>
	/// Extension methods for BiometricEncodingType
	/// </summary>
	public static class BiometricEncodingTypeExtensions
	{
		/// <summary>
		/// Determines the biometric encoding type from a BDB tag
		/// </summary>
		/// <param name="bioDataBlockTag">The BDB tag value</param>
		/// <returns>The corresponding BiometricEncodingType</returns>
		public static BiometricEncodingType FromBDBTag(int bioDataBlockTag)
		{
			return bioDataBlockTag switch
			{
				24366 => BiometricEncodingType.ISO_19794,
				32558 => BiometricEncodingType.ISO_39794,
				_ => BiometricEncodingType.UNKNOWN
			};
		}
	}

	/// <summary>
	/// Standard Biometric Header as defined in CBEFF standard
	/// Contains metadata about biometric data in a tag-value format
	/// </summary>
	public class StandardBiometricHeader : ISerializable
	{
        // serialVersionUID kept for parity in comments; not used in .NET
		private readonly SortedDictionary<int, byte[]> elements;

		/// <summary>
		/// Creates a new StandardBiometricHeader with the given elements
		/// </summary>
		/// <param name="elements">Map of tag-value pairs</param>
		public StandardBiometricHeader(IDictionary<int, byte[]> elements)
		{
			this.elements = new SortedDictionary<int, byte[]>(elements ?? throw new ArgumentNullException(nameof(elements)));
		}

		/// <summary>
		/// Gets a copy of the elements map
		/// </summary>
		/// <returns>Sorted dictionary of tag-value pairs</returns>
		public SortedDictionary<int, byte[]> GetElements()
		{
			return new SortedDictionary<int, byte[]>(elements);
		}

		/// <summary>
		/// Gets the value for a specific tag
		/// </summary>
		/// <param name="tag">The tag to look up</param>
		/// <returns>The value bytes, or null if not found</returns>
		public byte[]? GetElement(int tag)
		{
			return elements.TryGetValue(tag, out byte[]? value) ? value : null;
		}

		/// <summary>
		/// Sets a value for a specific tag
		/// </summary>
		/// <param name="tag">The tag</param>
		/// <param name="value">The value bytes</param>
		public void SetElement(int tag, byte[] value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			elements[tag] = value;
		}

		/// <summary>
		/// Removes an element by tag
		/// </summary>
		/// <param name="tag">The tag to remove</param>
		/// <returns>True if the element was removed, false if it didn't exist</returns>
		public bool RemoveElement(int tag)
		{
			return elements.Remove(tag);
		}

		/// <summary>
		/// Gets the number of elements in this header
		/// </summary>
		public int Count => elements.Count;

		/// <summary>
		/// Checks if an element with the given tag exists
		/// </summary>
		/// <param name="tag">The tag to check</param>
		/// <returns>True if the element exists</returns>
		public bool ContainsElement(int tag)
		{
			return elements.ContainsKey(tag);
		}

		public override string ToString()
		{
			var result = new System.Text.StringBuilder();
			result.Append("StandardBiometricHeader [");
			bool isFirst = true;
			foreach (var entry in elements)
			{
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					result.Append(", ");
				}
				result.Append($"0x{entry.Key:X}").Append(" -> ").Append(BitConverter.ToString(entry.Value).Replace("-", ""));
			}
			result.Append("]");
			return result.ToString();
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + (elements?.GetHashCode() ?? 0);
			return result;
		}

		public override bool Equals(object? obj)
		{
			if (this == obj) return true;
			if (obj == null) return false;
			if (GetType() != obj.GetType()) return false;

			StandardBiometricHeader other = (StandardBiometricHeader)obj;
			return Equals(elements, other.elements);
		}

		private static bool Equals(IDictionary<int, byte[]>? elements1, IDictionary<int, byte[]>? elements2)
		{
			if (elements1 == null && elements2 != null) return false;
			if (elements1 != null && elements2 == null) return false;
			if (elements1 == elements2) return true;

			if (elements1!.Count != elements2!.Count) return false;
			if (!elements1.Keys.SequenceEqual(elements2.Keys)) return false;

			foreach (var entry in elements1)
			{
				int key = entry.Key;
				byte[] bytes = entry.Value;
				if (!elements2.TryGetValue(key, out byte[]? otherBytes) || !bytes.SequenceEqual(otherBytes))
				{
					return false;
				}
			}
			return true;
		}
	}
}

