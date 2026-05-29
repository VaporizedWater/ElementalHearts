using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Systems;
using ElementalHearts.Content.Items.LifeShards;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Hearts;

/// <summary>
/// Heart themed around a vanilla buff potion. Buff-granting hearts trade the usual HP
/// gain for a permanent world-wide buff: while <see cref="ElementalHeartsPotionEffectConfig"/>
/// has the feature enabled, <see cref="HpGain"/> returns 0 and the buff is applied each
/// tick by <see cref="Common.Players.PotionHeartEffectsPlayer"/>. When the feature is
/// disabled the heart reverts to a plain HP grant — fulfilling the original spec that
/// disabling the feature makes them behave like regular hearts.
/// A subclass may return <c>0</c> from <see cref="BuffType"/> to opt out of the world-wide
/// effect entirely — used by novelty potions (Love, Stink) whose vanilla buffs are not
/// meaningful gameplay grants. Those hearts always give HP like any other.
/// </summary>
public abstract class PotionHeartItem : ElementalHeartItem
{
	/// <summary>
	/// BuffID granted to every player while this heart is consumed in the world. Return
	/// <c>0</c> to skip applying any buff — the heart will then only grant its HP boost.
	/// </summary>
	public abstract int BuffType { get; }

	/// <summary>ItemID of the vanilla potion this heart is themed around. Used in the recipe.</summary>
	public abstract int PotionItemId { get; }

	/// <summary>
	/// Buff-granting hearts skip the HP grant while the world-wide effect is active.
	/// Novelty hearts (BuffType == 0) always grant their tier HP. With the config
	/// toggle off, every potion heart falls back to the standard tier HP.
	/// </summary>
	public override int HpGain
	{
		get
		{
			if (BuffType <= 0)
				return base.HpGain;

			return ElementalHeartsPotionEffectConfig.Instance.WorldwidePotionEffectsEnabled
				? 0
				: base.HpGain;
		}
	}

	/// <summary>
	/// True for hearts whose consumption-registry slot can be flipped off and on by
	/// re-using the item. Only buff-granting Potion Hearts support this — novelty
	/// hearts (Love, Stink) behave like standard one-shot hearts.
	/// </summary>
	public bool IsToggleable => BuffType > 0;

	public sealed override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(PotionItemId, RecipeCost(5))
			.AddOptionalIngredient(MatchingLifeShard(), 1)
			.AddTile(TileID.Bottles)
			.Register();
	}

	/// <summary>
	/// Toggleable hearts stay usable while consumed so re-using them deactivates the
	/// world-wide effect. Non-toggleable hearts fall through to the base one-shot rule.
	/// </summary>
	public override bool CanUseItem(Player player) =>
		IsToggleable || base.CanUseItem(player);

	/// <summary>
	/// Toggleable hearts flip between consumed (effect active) and not-consumed
	/// (effect off). A standard heart's <see cref="ElementalHeartItem.UseItem"/>
	/// always tries to consume; we intercept that so a re-use deactivates instead.
	/// </summary>
	public override bool? UseItem(Player player)
	{
		if (player.whoAmI != Main.myPlayer)
			return false;

		if (!IsToggleable)
			return base.UseItem(player);

		bool wasConsumed = HeartConsumptionWorld.IsConsumed(ConsumptionId);
		bool ok = wasConsumed
			? HeartConsumptionWorld.TryDeactivate(this)
			: HeartConsumptionWorld.TryConsume(this);

		// Same visual feedback either direction so the player gets clear confirmation
		// that the toggle landed.
		if (ok)
			PlayConsumeEffect(player.Center);

		return ok;
	}

	private int MatchingLifeShard() => Tier switch
	{
		HeartTier.Common    => ModContent.ItemType<CommonLifeShard>(),
		HeartTier.Uncommon  => ModContent.ItemType<UncommonLifeShard>(),
		HeartTier.Rare      => ModContent.ItemType<RareLifeShard>(),
		HeartTier.Epic      => ModContent.ItemType<EpicLifeShard>(),
		HeartTier.Legendary => ModContent.ItemType<LegendaryLifeShard>(),
		_ => ModContent.ItemType<CommonLifeShard>(),
	};

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		bool effectsEnabled = ElementalHeartsPotionEffectConfig.Instance.WorldwidePotionEffectsEnabled;
		bool isBuffHeart = BuffType > 0;

		// Novelty hearts (Love, Stink) always behave as standard HP hearts, and any
		// buff heart with the world-wide effect disabled also reverts to standard HP
		// behaviour — both go through the base implementation unchanged.
		if (!isBuffHeart || !effectsEnabled)
		{
			base.ModifyTooltips(tooltips);
			return;
		}

		// Active buff heart: a single line in vanilla-tooltip style, "Permanently <effect>".
		// Each heart subclass curates its own phrasing via PermanentEffectText so the
		// grammar reads cleanly even when the vanilla potion tooltip starts with a
		// number or symbol (e.g. "25% increased movement speed"). Greyed out once
		// consumed so the player can tell the effect is currently live; a second gray
		// line explains they can re-use the item to switch it back off.
		bool consumed = HeartConsumptionWorld.IsConsumed(ConsumptionId);
		var effectLine = new TooltipLine(Mod, "PotionHeartEffect", PermanentEffectText);
		if (consumed)
			effectLine.OverrideColor = Color.Gray;
		tooltips.Add(effectLine);

		if (consumed)
		{
			tooltips.Add(new TooltipLine(Mod, "PotionHeartDeactivateHint", "Use again to deactivate")
			{
				OverrideColor = Color.Gray,
			});
		}
	}

	/// <summary>
	/// The "Permanently &lt;effect&gt;" text shown on a buff-active potion heart.
	/// Each concrete heart overrides this with a clean sentence. The default falls
	/// back to the vanilla potion's own tooltip (first line, first letter lowercased),
	/// which works for most phrasings but reads awkwardly for the few vanilla tooltips
	/// that start with a number or symbol — those should override.
	/// </summary>
	public virtual string PermanentEffectText
	{
		get
		{
			var vanillaTip = Lang.GetTooltip(PotionItemId);
			if (vanillaTip != null && vanillaTip.Lines > 0)
			{
				string raw = vanillaTip.GetLine(0);
				if (!string.IsNullOrWhiteSpace(raw))
				{
					string body = char.IsUpper(raw[0])
						? char.ToLowerInvariant(raw[0]) + raw[1..]
						: raw;
					return $"Permanently {body}";
				}
			}
			return $"Permanently grants {Lang.GetBuffName(BuffType)}";
		}
	}
}
