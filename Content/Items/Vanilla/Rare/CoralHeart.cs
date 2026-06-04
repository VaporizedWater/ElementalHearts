using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Vanilla.Rare;

/// <summary>
/// Active heart that trades max life for a reef-current speed boost while submerged.
/// The toggle state and water-speed behaviour live in <see cref="CoralHeartPlayer"/>.
/// </summary>
public sealed class CoralHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override int HpGain => 0;

	public override bool IsActiveAbility => true;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<CoralHeartPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) =>
		Main.LocalPlayer.GetModPlayer<CoralHeartPlayer>().Enabled = enabled;

	public override void AddRecipes() =>
		CreateRecipe()
			.AddIngredient(ItemID.Coral, RecipeCost(30))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 2)
			.AddTile(TileID.WorkBenches)
			.Register();
}
