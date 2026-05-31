using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using Terraria;
using Terraria.ModLoader;
using ElementalHearts.Common.Hearts;

namespace ElementalHearts.Content.Items.Vanilla.Exotic;

/// <summary>
/// Active heart: consuming it permanently unlocks the Cursor Focus camera ability (the camera eases
/// toward the cursor). Grants no HP — its payoff is the ability. The on/off toggle and all panning
/// logic live in <see cref="CursorFocusPlayer"/> / <see cref="Common.Camera.CursorFocusSystem"/>;
/// this stays a declaration that just names which character flag is its toggle.
/// </summary>
public sealed class MagnificationHeart : ElementalHeartItem
{
    public override HeartTier Tier => HeartTier.Exotic;

    public override int HpGain => 0;

    public override bool IsActiveAbility => true;

    public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<CursorFocusPlayer>().Enabled;

    public override void SetAbilityEnabled(bool enabled) =>
        Main.LocalPlayer.GetModPlayer<CursorFocusPlayer>().Enabled = enabled;

    public override void AddRecipes()
    {
        // Add recipes if needed
    }
}
