using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Uncommon;

public sealed class WaterleafHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Uncommon;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(10, 8));
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.Waterleaf, RecipeCost(20))
			.AddOptionalIngredient(ModContent.ItemType<UncommonLifeShard>(), 1)
			.AddTile(TileID.HeavyWorkBench)
			.Register();
	}
}

