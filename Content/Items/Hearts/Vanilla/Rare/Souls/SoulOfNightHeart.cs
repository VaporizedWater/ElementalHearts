using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Rare.Souls;

public sealed class SoulOfNightHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		// 4 frames
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 4));
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofNight, RecipeCost(150))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}

