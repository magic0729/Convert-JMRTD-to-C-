using System;
using System.Collections.Generic;

namespace org.jmrtd.io
{
	[Serializable]
	public class FragmentBuffer
	{
		private const int DEFAULT_SIZE = 2000;
		private byte[] buffer;
		private ISet<Fragment> fragments;

		public FragmentBuffer() : this(DEFAULT_SIZE) { }

		public FragmentBuffer(int length)
		{
			buffer = new byte[length];
			fragments = new HashSet<Fragment>();
		}

		public void UpdateFrom(FragmentBuffer other)
		{
			lock (this)
			{
				foreach (var otherFragment in other.fragments)
				{
					AddFragment(otherFragment.Offset, other.buffer, otherFragment.Offset, otherFragment.Length);
				}
			}
		}

		public void AddFragment(int offset, byte b) => AddFragment(offset, new[] { b });

		public void AddFragment(int offset, byte[] bytes) => AddFragment(offset, bytes, 0, bytes.Length);

		public void AddFragment(int offset, byte[] bytes, int srcOffset, int srcLength)
		{
			lock (this)
			{
				if (offset + srcLength > buffer.Length)
				{
					SetLength(2 * Math.Max(offset + srcLength, buffer.Length));
				}
				Buffer.BlockCopy(bytes, srcOffset, buffer, offset, srcLength);
				int thisOffset = offset;
				int thisLength = srcLength;
				var otherFragments = new List<Fragment>(fragments);
				foreach (var other in otherFragments)
				{
					if (other.Offset <= thisOffset && thisOffset + thisLength <= other.Offset + other.Length)
					{
						return;
					}
					if (other.Offset <= thisOffset && thisOffset <= other.Offset + other.Length)
					{
						thisLength = thisOffset + thisLength - other.Offset;
						thisOffset = other.Offset;
						fragments.Remove(other);
						continue;
					}
					if (thisOffset <= other.Offset && other.Offset + other.Length <= thisOffset + thisLength)
					{
						fragments.Remove(other);
						continue;
					}
					if (thisOffset > other.Offset || other.Offset > thisOffset + thisLength) continue;
					thisLength = other.Offset + other.Length - thisOffset;
					fragments.Remove(other);
				}
				fragments.Add(Fragment.GetInstance(thisOffset, thisLength));
			}
		}

		public int GetPosition()
		{
			lock (this)
			{
				int result = 0;
				for (int i = 0; i < buffer.Length; i++)
				{
					if (!IsCoveredByFragment(i)) continue;
					result = i + 1;
				}
				return result;
			}
		}

		public int GetBytesBuffered()
		{
			lock (this)
			{
				int result = 0;
				for (int i = 0; i < buffer.Length; i++) if (IsCoveredByFragment(i)) result++;
				return result;
			}
		}

		public bool IsCoveredByFragment(int offset) => IsCoveredByFragment(offset, 1);

		public bool IsCoveredByFragment(int offset, int length)
		{
			lock (this)
			{
				foreach (var fragment in fragments)
				{
					int left = fragment.Offset;
					int right = fragment.Offset + fragment.Length;
					if (left <= offset && offset + length <= right) return true;
				}
				return false;
			}
		}

		public int GetBufferedLength(int index)
		{
			lock (this)
			{
				int result = 0;
				if (index >= buffer.Length) return 0;
				foreach (var fragment in fragments)
				{
					int left = fragment.Offset;
					int right = fragment.Offset + fragment.Length;
					if (left > index || index >= right) continue;
					int newResult = right - index;
					if (newResult > result) result = newResult;
				}
				return result;
			}
		}

		public ISet<Fragment> GetFragments() => fragments;
		public byte[] GetBuffer() => buffer;
		public int GetLength() { lock (this) { return buffer.Length; } }

		public Fragment GetSmallestUnbufferedFragment(int offset, int length)
		{
			lock (this)
			{
				int thisOffset = offset;
				int thisLength = length;
				foreach (var other in fragments)
				{
					if (other.Offset <= thisOffset && thisOffset + thisLength <= other.Offset + other.Length)
					{
						thisLength = 0;
						break;
					}
					if (other.Offset <= thisOffset && thisOffset < other.Offset + other.Length)
					{
						int newOffset = other.Offset + other.Length;
						int newLength = thisOffset + thisLength - newOffset;
						thisOffset = newOffset;
						thisLength = newLength;
						continue;
					}
					if ((thisOffset <= other.Offset && other.Offset + other.Length <= thisOffset + thisLength) || offset > other.Offset || other.Offset >= thisOffset + thisLength) continue;
					thisLength = other.Offset - thisOffset;
				}
				return Fragment.GetInstance(thisOffset, thisLength);
			}
		}

		public override string ToString()
		{
			lock (this) { return $"FragmentBuffer [{buffer.Length}, {fragments}]"; }
		}

		public override bool Equals(object? obj)
		{
			if (obj is not FragmentBuffer other) return false;
			if (other.buffer == null && buffer != null) return false;
			if (other.buffer != null && buffer == null) return false;
            if (other.fragments == null && fragments != null) return false;
            if (other.fragments != null && fragments == null) return false;
            return EqualityComparer<byte[]>.Default.Equals(other.buffer, buffer) && (other.fragments?.SetEquals(fragments ?? new HashSet<Fragment>()) ?? false);
		}

		public override int GetHashCode() => 3 * (buffer?.GetHashCode() ?? 0) + 2 * (fragments?.GetHashCode() ?? 0) + 7;

		private void SetLength(int length)
		{
			lock (this)
			{
				if (length <= buffer.Length) return;
				var newBuffer = new byte[length];
				Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
				buffer = newBuffer;
			}
		}

		[Serializable]
		public sealed class Fragment
		{
			public int Offset { get; }
			public int Length { get; }
			private Fragment(int offset, int length) { Offset = offset; Length = length; }
			public static Fragment GetInstance(int offset, int length) => new Fragment(offset, length);
			public override string ToString() => $"[{Offset} .. {Offset + Length - 1} ({Length})]";
			public override bool Equals(object? obj) => obj is Fragment f && f.Offset == Offset && f.Length == Length;
			public override int GetHashCode() => 2 * Offset + 3 * Length + 5;
		}
	}
}


