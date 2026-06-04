using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Content.Items.Hearts;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Vanilla.Exotic;

/// <summary>
/// Active heart: consuming it unlocks "passive income". The idle shards you bank *while it's equipped*
/// can be cashed out as coins — a new "Sell" button appears under "Claim" in the Heart Log. Grants no
/// HP; the economy is the payoff. The on/off flag and all the selling logic live in
/// <see cref="PiggyBankPlayer"/> / <see cref="Common.Players.IdleShardPlayer"/>; this stays a
/// declaration that only names which character flag is its toggle (mirrors <see cref="MagnificationHeart"/>),
/// plus the one anti-exploit hook below.
/// </summary>
public sealed class PiggyBankHeart : ElementalHeartItem
{
    public override HeartTier Tier => HeartTier.Exotic;

    public override int HpGain => 0;

    public override bool IsActiveAbility => true;

    public override bool IsAbilityEnabled => Main.LocalPlayer.GetModPlayer<PiggyBankPlayer>().Enabled;

    /// <summary>
    /// Switching the ability *on* first force-claims the existing bank as ordinary shard items, so
    /// shards hoarded before you had passive income can't be retroactively cashed out as coins — only
    /// what you bank while it's equipped is sellable.
    /// </summary>
    public override void SetAbilityEnabled(bool enabled)
    {
        var pb = Main.LocalPlayer.GetModPlayer<PiggyBankPlayer>();
        if (enabled && !pb.Enabled)
            Main.LocalPlayer.GetModPlayer<IdleShardPlayer>().ClaimShards();
        pb.Enabled = enabled;
    }

    public override void AddRecipes() =>
        CreateRecipe()
            .AddIngredient(ItemID.PiggyBank, 1)
            .AddIngredient(ItemID.GoldCoin, RecipeCost(10))
            .AddTile(TileID.WorkBenches)
            .Register();
}
