using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.Hearts.CrossMod;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Mythic;

public sealed class ZenithHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Mythic;

	public override void AddRecipes()
	{
		var recipe = CreateRecipe();

		// Only vanilla craftable material hearts qualify. That rules out: boss-drop hearts
		// (BossHeartItem — also covers the Pacified hearts and cross-mod boss hearts),
		// potion hearts (PotionHeartItem), cross-mod craftable hearts (CrossModHeartItem),
		// and the Zenith Heart itself. Menacing Hearts are plain ModItems, not
		// ElementalHeartItems, so they never appear here in the first place.
		foreach (ElementalHeartItem heart in ModContent.GetContent<ElementalHeartItem>())
		{
			if (heart is BossHeartItem or PotionHeartItem or CrossModHeartItem or ZenithHeart)
				continue;

			recipe.AddIngredient(heart.Type);
		}

		recipe.AddTile(TileID.LunarCraftingStation);
		recipe.AddCondition(global::ElementalHearts.Common.Systems.AnimateProgressionSystem.DownedLegendaryAnimate);
		recipe.Register();
	}
}
