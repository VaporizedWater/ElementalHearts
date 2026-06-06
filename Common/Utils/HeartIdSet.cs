// Architecture scaffold only. Fill behavior in the implementation pass.
using System;

namespace ElementalHearts.Core;

/// <summary>Typed bitset wrapper for HeartId values. ID value 0 is reserved for None and is not stored.</summary>
public sealed class HeartIdSet
{
	private readonly BitSet bits = new((int)HeartId.Length - 1);

	public bool Contains(HeartId id) => bits.Contains(BitIndex(id));
	public void Add(HeartId id) => bits.Add(BitIndex(id));
	public void Remove(HeartId id) => bits.Remove(BitIndex(id));
	public void Clear() => bits.Clear();
	public ulong[] ToArray() => bits.ToArray();
	public void Load(ReadOnlySpan<ulong> words) => bits.Load(words);
	public Enumerator GetEnumerator() => new(bits.GetEnumerator());

	private static int BitIndex(HeartId id) => (int)id - 1;
	private static HeartId FromBitIndex(int bit) => (HeartId)(bit + 1);

	public struct Enumerator
	{
		private BitSet.Enumerator inner;
		public HeartId Current { get; private set; }

		public Enumerator(BitSet.Enumerator inner)
		{
			this.inner = inner;
			Current = default;
		}

		public bool MoveNext()
		{
			if (!inner.MoveNext())
				return false;

			Current = FromBitIndex(inner.Current);
			return true;
		}
	}
}
