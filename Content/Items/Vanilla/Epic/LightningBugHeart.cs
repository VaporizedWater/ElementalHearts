using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
namespace ElementalHearts.Content.Items.Vanilla.Epic;

/// <summary>
/// A utility heart: instead of max life it grants 8% increased movement speed.
/// It trades HP for an ability — the actual movement speed buff lives in
/// <see cref="LightningBugHeartPlayer"/>, which checks this heart's toggle.
/// Returning 0 from <see cref="HpGain"/> suppresses both the HP tooltip line
/// and the floating "+HP" combat text (see the base).
/// </summary>
public sealed class LightningBugHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Epic;

	public override int HpGain => 0;

	public override bool IsActiveAbility => true;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<LightningBugHeartPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) => Main.LocalPlayer.GetModPlayer<LightningBugHeartPlayer>().Enabled = enabled;

	public override void AddRecipes() =>
		CreateRecipe()
			.AddIngredient(ItemID.LightningBug, RecipeCost(50))
			.AddOptionalIngredient(ModContent.ItemType<EpicLifeShard>(), 2)
			.AddTile(TileID.CrystalBall)
			.Register();
}
