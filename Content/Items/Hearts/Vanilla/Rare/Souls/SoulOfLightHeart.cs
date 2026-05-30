using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;

namespace ElementalHearts.Content.Items.Hearts.Vanilla.Rare.Souls;

public sealed class SoulOfLightHeart : ElementalHeartItem
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
			.AddIngredient(ItemID.SoulofLight, RecipeCost(150))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}

