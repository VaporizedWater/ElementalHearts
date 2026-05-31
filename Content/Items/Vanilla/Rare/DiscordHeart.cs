using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;
using Terraria.ModLoader;
using Terraria.ID;

using ElementalHearts.Content.Items.Hearts;
using Terraria;
using ElementalHearts.Common.Players;

namespace ElementalHearts.Content.Items.Vanilla.Rare;

public sealed class DiscordHeart : ElementalHeartItem
{
	public override HeartTier Tier => HeartTier.Rare;

	public override int HpGain => 0;

	public override bool IsActiveAbility => true;

	public override int? ActiveAbilityDailyCost => 10;

	public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<DiscordAbilityPlayer>().Enabled;

	public override void SetAbilityEnabled(bool enabled) =>
		Main.LocalPlayer.GetModPlayer<DiscordAbilityPlayer>().Enabled = enabled;

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.PixieDust, RecipeCost(300))
			.AddOptionalIngredient(ModContent.ItemType<RareLifeShard>(), 3)
			.AddTile(TileID.CrystalBall)
			.Register();
	}
}
