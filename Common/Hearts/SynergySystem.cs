using System.Collections.Generic;
using ElementalHearts.Content.Items.Hearts;
using Terraria.ModLoader;
using global::ElementalHearts.Content.Items.Vanilla.Exotic;
using global::ElementalHearts.Content.Items.Vanilla.Uncommon;

namespace ElementalHearts.Common.Hearts
{
	public class SynergySystem : ModSystem
	{
		// Dictionary mapping an ItemType to a list of its synergizing ItemTypes
		private static Dictionary<int, List<int>> synergies;

		public override void Load()
		{
			synergies = new Dictionary<int, List<int>>();
		}

		public override void Unload()
		{
			synergies = null;
		}

		public override void PostSetupContent()
		{
			// Register our first synergy here!
			RegisterSynergy(ModContent.ItemType<KingSlimeHeart>(), ModContent.ItemType<EncumberingHeart>());
		}

		/// <summary>
		/// Registers a two-way synergy between two items.
		/// </summary>
		public static void RegisterSynergy(int itemType1, int itemType2)
		{
			if (synergies == null) return;

			if (!synergies.ContainsKey(itemType1))
				synergies[itemType1] = new List<int>();
			
			if (!synergies.ContainsKey(itemType2))
				synergies[itemType2] = new List<int>();

			if (!synergies[itemType1].Contains(itemType2))
				synergies[itemType1].Add(itemType2);
				
			if (!synergies[itemType2].Contains(itemType1))
				synergies[itemType2].Add(itemType1);
		}

		/// <summary>
		/// Returns a list of ItemTypes that synergize with the provided ItemType.
		/// </summary>
		public static IReadOnlyList<int> GetSynergies(int itemType)
		{
			if (synergies != null && synergies.TryGetValue(itemType, out var list))
			{
				return list;
			}
			return new List<int>();
		}
	}
}
