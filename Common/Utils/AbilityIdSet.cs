// Architecture scaffold only. Fill behavior in the implementation pass.
using System;

namespace ElementalHearts.Core;

/// <summary>Typed bitset wrapper for AbilityId values. ID value 0 is reserved for None and is not stored.</summary>
public sealed class AbilityIdSet
{
	private readonly BitSet bits = new((int)AbilityId.Length - 1);

	public bool Contains(AbilityId id) => bits.Contains(BitIndex(id));
	public void Add(AbilityId id) => bits.Add(BitIndex(id));
	public void Remove(AbilityId id) => bits.Remove(BitIndex(id));
	public void Clear() => bits.Clear();
	public ulong[] ToArray() => bits.ToArray();
	public void Load(ReadOnlySpan<ulong> words) => bits.Load(words);
	public Enumerator GetEnumerator() => new(bits.GetEnumerator());

	private static int BitIndex(AbilityId id) => (int)id - 1;
	private static AbilityId FromBitIndex(int bit) => (AbilityId)(bit + 1);

	public struct Enumerator
	{
		private BitSet.Enumerator inner;
		public AbilityId Current { get; private set; }

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
