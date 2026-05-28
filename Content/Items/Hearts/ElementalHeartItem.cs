using System;
using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.Hearts;

/// <summary>
/// Base class for every elemental heart. Concrete hearts only override <see cref="Tier"/>
/// and <see cref="AddRecipes"/>. Defaults, rarity, HP grant, tooltips, per-world use gating,
/// and multiplayer sync are all handled here.
/// </summary>
public abstract class ElementalHeartItem : ModItem
{
	public abstract HeartTier Tier { get; }

	/// <summary>
	/// Stable identifier used to record consumption. Defaults to the class name; only
	/// override if a class is renamed and the original ID must be preserved for save
	/// compatibility.
	/// </summary>
	public virtual string ConsumptionId => Name;

	/// <summary>
	/// Internal name of the mod this heart is themed around (e.g. <c>"CalamityMod"</c>),
	/// or <c>null</c> for a vanilla heart. Drives the per-mod load gate in
	/// <see cref="IsLoadingEnabled"/>, so every cross-mod heart — boss or craftable —
	/// must report its source mod.
	/// </summary>
	public virtual string SourceMod => null;

	/// <summary>
	/// Vanilla hearts always load; a cross-mod heart loads only when its mod is enabled
	/// in <see cref="ElementalHeartsCrossModConfig"/>.
	/// </summary>
	public override bool IsLoadingEnabled(Mod mod) =>
		SourceMod == null || ElementalHeartsCrossModConfig.ShouldLoadHeartsFor(SourceMod);

	/// <summary>
	/// Material name for the activated-power tooltip. Defaults to
	/// <see cref="ElementalPowerRegistry"/>; override via <c>Items.{Name}.ElementalPower</c>
	/// in localization.
	/// </summary>
	public virtual string ElementalPowerMaterial
	{
		get
		{
			string key = $"Mods.{Mod.Name}.Items.{Name}.ElementalPower";
			return Language.Exists(key)
				? Language.GetTextValue(key)
				: ElementalPowerRegistry.Get(Name);
		}
	}

	/// <summary>
	/// Max-life granted on consumption. Virtual so a subclass can opt out of the HP
	/// grant entirely — see <see cref="PotionHeartItem"/>, where buff-granting hearts
	/// trade HP for a world-wide buff and return 0 here while that buff is active.
	/// </summary>
	public virtual int HpGain => Tier.GetHpGain();

	/// <summary>Base consumption sound. Boss hearts layer extra audio on top of this.</summary>
	protected virtual SoundStyle ConsumeSound =>
		SoundID.Item4.WithPitchOffset(0.5f - ((int)Tier * 0.1f));

	protected int RecipeCost(int baseAmount)
	{
		float multiplier = ElementalHeartsRecipeConfig.Instance.RecipeDifficulty / 10f;
		int raw = (int)Math.Round(baseAmount * multiplier);

		if (raw <= 5)
			return Math.Max(1, raw);

		int step = raw switch
		{
			<= 100 => 5,
			<= 500 => 25,
			<= 1000 => 50,
			_ => 100,
		};

		// Ceiling-to-step gives "nice" numbers (e.g. 42 → 45, 210 → 225).
		return (int)Math.Ceiling(raw / (float)step) * step;
	}

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;
		ItemID.Sets.ItemNoGravity[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.width = 18;
		Item.height = 18;
		Item.maxStack = 1;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.useAnimation = 30;
		Item.useTime = 30;
		Item.UseSound = SoundID.Item3;
		Item.consumable = false;
		Item.rare = Tier.GetRarityType();
		Item.value = Item.sellPrice(silver: (int)Tier * 25);
	}

	public override bool CanUseItem(Player player) =>
		!HeartConsumptionWorld.IsConsumed(ConsumptionId);

	public override Nullable<bool> UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
	{
		if (player.whoAmI != Main.myPlayer)
			return false;

		bool consumed = HeartConsumptionWorld.TryConsume(this);
		if (consumed)
			PlayConsumeEffect(player.Center);

		return consumed;
	}

	/// <summary>
	/// Plays the full audio-visual consumption effect at <paramref name="center"/>.
	/// Callable on a remote-client visual (see <see cref="HeartConsumptionWorld"/>),
	/// so it takes a position rather than the consuming <see cref="Player"/>.
	/// </summary>
	public virtual void PlayConsumeEffect(Vector2 center)
	{
		float strength = ElementalHeartsVisualConfig.Instance.ConsumptionEffectStrength / 3f;
		HeartEffect effect = HeartEffectRegistry.Get(ConsumptionId);
		Color tierColor = Tier.GetEffectColor();

		PlayConsumeSound(center);
		EmitConsumeDust(center, effect, tierColor, strength);
		ShakeCamera(center, strength);
		AddConsumeLight(center, effect.Rainbow ? tierColor : effect.Primary, strength);
	}

	protected virtual void PlayConsumeSound(Vector2 center)
	{
		SoundEngine.PlaySound(ConsumeSound, center);
	}

	/// <summary>
	/// Two concentric particle rings: an outer burst tinted to the heart's material
	/// (<paramref name="effect"/>) and a tighter inner core in the tier colour.
	/// Per-tier growth is deliberately gentle so a Mythic heart feels grander than a
	/// Common one without dwarfing it.
	/// </summary>
	protected virtual void EmitConsumeDust(Vector2 center, HeartEffect effect, Color tierColor, float strength)
	{
		int tierLevel = (int)Tier;

		// Outer ring — material colour.
		int outerCount = (int)((22 + (tierLevel * 3)) * strength);
		float outerSpeed = (2.6f + (tierLevel * 0.45f)) * strength;
		float outerScale = (1.35f + (tierLevel * 0.06f)) * strength;

		for (int i = 0; i < outerCount; i++)
		{
			Vector2 direction = new Vector2(0f, -1f).RotatedByRandom(MathHelper.TwoPi);
			float speed = outerSpeed * (0.75f + (Main.rand.NextFloat() * 0.4f));

			Dust outer = Dust.NewDustPerfect(center, DustID.FireworksRGB, direction * speed, 0, effect.Pick(Main.rand), outerScale);
			outer.noGravity = true;
			outer.fadeIn = 1.1f;
		}

		// Inner ring — tier colour, a slower, smaller sparkling core.
		int innerCount = (int)((12 + (tierLevel * 2)) * strength);
		float innerSpeed = outerSpeed * 0.4f;
		float innerScale = (0.85f + (tierLevel * 0.04f)) * strength;

		for (int i = 0; i < innerCount; i++)
		{
			Vector2 direction = new Vector2(0f, -1f).RotatedByRandom(MathHelper.TwoPi);
			float speed = innerSpeed * (0.7f + (Main.rand.NextFloat() * 0.4f));

			Dust inner = Dust.NewDustPerfect(center, DustID.GemDiamond, direction * speed, 120, tierColor, innerScale);
			inner.noGravity = true;
		}
	}

	protected virtual void ShakeCamera(Vector2 center, float strength)
	{
		int tierLevel = (int)Tier;
		float magnitude = (1.1f + (tierLevel * 0.32f)) * strength;
		Vector2 punchDir = new Vector2(0f, -1f).RotatedByRandom(MathHelper.TwoPi);

		Main.instance.CameraModifiers.Add(new PunchCameraModifier(
			center,
			punchDir,
			magnitude,
			4f + (tierLevel * 0.3f),  // vibration cycles per second
			14 + tierLevel,           // duration in frames
			1000f,                    // distance falloff
			Name));
	}

	protected virtual void AddConsumeLight(Vector2 center, Color color, float strength)
	{
		if (Main.netMode == NetmodeID.Server)
			return;

		int tierLevel = (int)Tier;
		Lighting.AddLight(center, color.ToVector3() * (1.6f + (tierLevel * 0.16f)) * strength);
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		// Hearts that opt out of HP entirely (buff-granting potion hearts while the
		// world-wide effect is enabled) skip the HP line — printing "Permanently
		// increases maximum life by 0" would be wrong and ugly.
		if (HpGain > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "ElementalHeartHp",
				Language.GetTextValue("Mods.ElementalHearts.Common.HpGain", HpGain)));
		}

		if (HeartConsumptionWorld.IsConsumed(ConsumptionId))
		{
			tooltips.Add(new TooltipLine(Mod, "ElementalHeartConsumed",
				Language.GetTextValue("Mods.ElementalHearts.Common.ElementalPowerActivated", ElementalPowerMaterial))
			{
				OverrideColor = Color.Gray,
			});
		}
	}
}
