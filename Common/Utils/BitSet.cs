// Architecture scaffold only. Fill behavior in the implementation pass.
using System;
using System.Numerics;

namespace ElementalHearts.Core;

/// <summary>Compact mutable bitset used by typed ID sets. Implementation is intentionally small and allocation-light.</summary>
public sealed class BitSet
{
	private readonly ulong[] words;

	public int BitCount { get; }
	public int WordCount => words.Length;

	public BitSet(int bitCount)
	{
		BitCount = bitCount;
		words = new ulong[(bitCount + 63) / 64];
	}

	public bool Contains(int bit) => (words[bit / 64] & (1UL << (bit % 64))) != 0;
	public void Add(int bit) => words[bit / 64] |= 1UL << (bit % 64);
	public void Remove(int bit) => words[bit / 64] &= ~(1UL << (bit % 64));
	public void Clear() => Array.Clear(words);
	public ulong[] ToArray() => (ulong[])words.Clone();

	public void Load(ReadOnlySpan<ulong> source)
	{
		// Load compact words from saves/network packets into the fixed-size set.
		Clear();
		source.CopyTo(words);
	}

	public Enumerator GetEnumerator() => new(words);

	public struct Enumerator
	{
		private readonly ulong[] words;
		private int wordIndex;
		private ulong word;
		private int offset;

		public int Current { get; private set; }

		public Enumerator(ulong[] words)
		{
			this.words = words;
			wordIndex = -1;
			word = 0;
			offset = 0;
			Current = 0;
		}

		public bool MoveNext()
		{
			while (true)
			{
				if (word != 0)
				{
					int bit = BitOperations.TrailingZeroCount(word);
					Current = offset + bit;
					word &= word - 1UL;
					return true;
				}

				wordIndex++;
				if (wordIndex >= words.Length)
					return false;

				word = words[wordIndex];
				offset = wordIndex * 64;
			}
		}
	}
}
