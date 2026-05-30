using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Epic;

public sealed class SoulOfMightHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		// 4 frames
		Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 4));
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.SoulofMight, RecipeCost(80))
			.AddOptionalIngredient(ModContent.ItemType<EpicLifeShard>(), 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
	}
}

