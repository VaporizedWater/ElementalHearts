using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Vanilla.Exotic;

/// <summary>
/// Active heart: consuming it permanently upgrades your dash so every dash spits a mini
/// jack-o'-lantern out the back. Grants no HP — the dash burst is the payoff. The on/off toggle,
/// dash detection and lantern spawn all live in <see cref="JackOLanternDashPlayer"/>; this stays a
/// declaration that only names which character flag is its toggle (mirrors <see cref="MagnificationHeart"/>).
/// </summary>
public sealed class JackOLanternHeart : ElementalHeartItem
{
    public override HeartTier Tier => HeartTier.Exotic;

    public override int HpGain => 0;

    public override bool IsActiveAbility => true;

    /// <summary>Costs 5 shards/day to keep the dash burst running — pricier than the Exotic tier
    /// default (2) because a free-firing projectile on every dash earns its keep.</summary>
    public override int? ActiveAbilityDailyCost => 5;

    public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<JackOLanternDashPlayer>().Enabled;

    public override void SetAbilityEnabled(bool enabled) =>
        Main.LocalPlayer.GetModPlayer<JackOLanternDashPlayer>().Enabled = enabled;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        // 3-frame animated sprite.
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 3));
    }

    public override void AddRecipes() =>
        CreateRecipe()
            .AddIngredient(ItemID.JackOLantern, RecipeCost(10))
            .AddIngredient(ItemID.Pumpkin, RecipeCost(50))
            .AddTile(TileID.Sawmill)
            .Register();
}
