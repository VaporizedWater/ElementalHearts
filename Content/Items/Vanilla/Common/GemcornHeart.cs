using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

/// <summary>
/// A utility heart that seeds raw gems into ordinary trees: once consumed, chopping any
/// vanilla tree has a chance to shed gemstones. Like the Magnification Heart it trades HP
/// for an ability — the harvest itself lives in <see cref="Common.Tiles.TreeLootGlobalTile"/>,
/// which keys off this heart's <see cref="ElementalHeartItem.ConsumptionId"/>. Returning 0
/// from <see cref="HpGain"/> suppresses both the HP tooltip line and the floating "+HP" text.
/// </summary>
public sealed class GemcornHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override int HpGain => 0;

	public override bool IsActiveAbility => true;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<GemcornHeartPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) => Main.LocalPlayer.GetModPlayer<GemcornHeartPlayer>().Enabled = enabled;

	protected override int AnimationFrameCount => 6;

	// Themed on the vanilla gemcorns (acorn + gem): crafted from acorns, a wooden core, and
	// one of each tree-plantable gem so the heart visibly "contains" every gem it can grow.
	public override void AddRecipes() =>
		CreateRecipe()
			.AddIngredient(ItemID.Acorn, RecipeCost(50))
			.AddIngredient(ItemID.Wood, RecipeCost(50))
			.AddIngredient(ItemID.Amethyst)
			.AddIngredient(ItemID.Topaz)
			.AddIngredient(ItemID.Sapphire)
			.AddIngredient(ItemID.Emerald)
			.AddIngredient(ItemID.Ruby)
			.AddIngredient(ItemID.Diamond)
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 2)
			.AddTile(TileID.Sawmill)
			.Register();
}

