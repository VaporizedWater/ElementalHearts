using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Common;

/// <summary>
/// A utility heart: instead of max life it teaches the forest to give a little more back,
/// boosting tree loot. Like the Magnification Heart it trades HP for an ability — the actual
/// drop boost lives in <see cref="Common.Tiles.TreeLootGlobalTile"/>, which keys off this
/// heart's <see cref="ElementalHeartItem.ConsumptionId"/>. Returning 0 from <see cref="HpGain"/>
/// suppresses both the HP tooltip line and the floating "+HP" combat text (see the base).
/// </summary>
public sealed class AcornHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Common;

	public override int HpGain => 0;

	public override bool IsActiveAbility => true;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<AcornHeartPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) => Main.LocalPlayer.GetModPlayer<AcornHeartPlayer>().Enabled = enabled;

	public override void AddRecipes() =>
		CreateRecipe()
			.AddIngredient(ItemID.Acorn, RecipeCost(50))
			.AddOptionalIngredient(ModContent.ItemType<CommonLifeShard>(), 2)
			.AddTile(TileID.Sawmill)
			.Register();
}
