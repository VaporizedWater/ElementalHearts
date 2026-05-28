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

		foreach (ElementalHeartItem heart in ModContent.GetContent<ElementalHeartItem>())
		{
			if (heart is CrossModHeartItem or ZenithHeart)
				continue;

			recipe.AddIngredient(heart.Type);
		}

		recipe.AddTile(TileID.LunarCraftingStation);
		recipe.AddCondition(global::ElementalHearts.Common.Systems.AnimateProgressionSystem.DownedLegendaryAnimate);
		recipe.Register();
	}
}
