using System;
using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
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

	/// <summary>
	/// Munchies-checklist presentation for this heart. Each hook has a sensible default so
	/// new hearts appear in the Munchies list with no extra work; only override when a
	/// specific heart needs custom phrasing, difficulty, or availability gating.
	/// See <see cref="Common.CrossMod.Munchies.MunchiesIntegration"/>.
	/// </summary>
	public virtual string MunchiesDifficulty => "classic";

	/// <summary>Text colour used for the heart's row in Munchies. Defaults to its tier colour.</summary>
	public virtual Color? MunchiesTextColor => Tier.GetEffectColor();

	/// <summary>
	/// Func returning whether the heart can currently be consumed. <c>null</c> means
	/// always available — the default, since hearts only require the right materials.
	/// </summary>
	public virtual Func<bool> MunchiesAvailability => null;

	/// <summary>Extra tooltip line for the Munchies entry, or <c>null</c> for none.</summary>
	public virtual LocalizedText MunchiesExtraTooltip => null;

	/// <summary>Acquisition hint shown at the top of the Munchies hover tooltip, or <c>null</c>.</summary>
	public virtual LocalizedText MunchiesAcquisitionText => null;

	/// <summary>
	/// Base consumption sound: the bespoke per-tier "crystal" chime authored for the mod
	/// (one recording per tier, Common through Mythic). Boss hearts pin the neutral chime in
	/// <see cref="BossHeartItem"/> and layer their own signature on top instead.
	/// </summary>
	protected virtual SoundStyle ConsumeSound => new($"ElementalHearts/Sounds/{Tier}CrystalPickup");

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

		// Craftable hearts are capped at 1/10 of their recipe's material sell value
		// (see CraftableHeartSellValueSystem). Boss-drop hearts and any heart without a
		// registered recipe keep the tier-based value above.
		if (CraftableHeartSellValueSystem.TryGetSellValue(Item.type, out int recipeCappedValue))
			Item.value = recipeCappedValue;
	}

	/// <summary>Debounces the "already consumed" cue so holding the button can't machine-gun it.</summary>
	private static uint _lastDepletedCueTick;

	public override bool CanUseItem(Player player)
	{
		bool isConsumed = ElementalHeartsWorldConfig.Instance.SharedProgression
			? HeartConsumptionWorld.IsConsumed(ConsumptionId)
			: player.GetModPlayer<HeartConsumptionPlayer>().IsConsumedLocally(ConsumptionId);

		if (!isConsumed)
			return true;

		// This heart's power is already claimed in this world. A dull thud + gray puff
		// reads as "nope" far better than the item silently refusing to do anything.
		if (player.whoAmI == Main.myPlayer && Main.GameUpdateCount - _lastDepletedCueTick > 40)
		{
			_lastDepletedCueTick = Main.GameUpdateCount;
			SoundEngine.PlaySound(SoundID.Item4.WithVolumeScale(0.5f).WithPitchOffset(-0.7f), player.Center);

			for (int i = 0; i < 6; i++)
			{
				Dust puff = Dust.NewDustPerfect(
					player.Center + Main.rand.NextVector2Circular(10f, 10f),
					DustID.Smoke, Vector2.UnitY * -Main.rand.NextFloat(0.6f), 160, Color.Gray, 0.9f);
				puff.noGravity = true;
			}
		}

		return false;
	}

	public override Nullable<bool> UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */
	{
		if (player.whoAmI != Main.myPlayer)
			return false;

		bool consumed = ElementalHeartsWorldConfig.Instance.SharedProgression
			? HeartConsumptionWorld.TryConsume(this)
			: player.GetModPlayer<HeartConsumptionPlayer>().TryConsumeLocally(this);

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

		ReLogic.Utilities.SlotId boomSlot = default;
		// The very top of the ladder earns a real "moment": shockwave + sub-bass on top of the
		// shared wash/burst above.
		if ((int)Tier >= (int)HeartTier.Epic)
			boomSlot = EmitTopTierPayoff(center, effect, tierColor, strength);

		// A faint, fast, rarity-coloured wash over the screen — barely there on a Common heart,
		// a touch more present on a Mythic one. ScreenFlashSystem eases it smoothly in and out.
		ScreenFlashSystem.Flash(GetRarityColor(), (0.05f + (GetRarityScale() * 0.1f)) * strength, 4f, 14f, boomSlot, 0.45f);

		// Tell the player exactly what they just earned. The number is the whole point of
		// the heart, so float it up in the tier colour — dramatic (larger) for the top tiers.
		if (HpGain > 0)
		{
			Rectangle textArea = new((int)center.X - 12, (int)center.Y - 24, 24, 24);
			CombatText.NewText(textArea, tierColor, $"+{HpGain}", dramatic: (int)Tier >= 5);
		}
	}

	/// <summary>
	/// Reserved-for-the-best flourish layered on top of the normal consume effect for Epic
	/// and higher hearts: an expanding heart-shaped shockwave and a deep sub-bass boom under the
	/// chime. Kept exclusive so it never stops feeling rare.
	/// </summary>
	protected virtual ReLogic.Utilities.SlotId EmitTopTierPayoff(Vector2 center, HeartEffect effect, Color tierColor, float strength)
	{
		// Expanding shockwave: one evenly-walked pass round the heart curve, so the wave front
		// itself reads as a heart silhouette.
		int ringCount = (int)(48 * strength);
		float ringSpeed = 9f * strength;
		for (int i = 0; i < ringCount; i++)
		{
			Vector2 direction = HeartBurstDirection(MathHelper.TwoPi * i / ringCount);
			Dust ring = Dust.NewDustPerfect(center, DustID.FireworksRGB, direction * ringSpeed, 0, effect.Pick(Main.rand), 1.5f);
			ring.noGravity = true;
			ring.fadeIn = 1.3f;
		}

		// Deep boom felt under the bright consume chime. Started at 0 volume so the 
		// ScreenFlashSystem can cleanly fade it in and out over the same envelope as the flash.
		return SoundEngine.PlaySound(SoundID.Item14.WithVolumeScale(0f).WithPitchOffset(-0.6f), center);
	}

	/// <summary>
	/// Direction for a particle so that a full burst settles into a soft heart silhouette as it
	/// expands. <paramref name="t"/> walks the classic heart parametric curve over [0, 2π); the
	/// result is flipped to screen space (point downward), recentred and scaled to ~unit reach.
	/// The per-particle speed jitter at the call sites keeps it a soft cloud, not a rigid outline.
	/// </summary>
	private static Vector2 HeartBurstDirection(float t)
	{
		float sin = (float)Math.Sin(t);
		float x = 16f * sin * sin * sin;
		float y = (13f * (float)Math.Cos(t)) - (5f * (float)Math.Cos(2f * t))
				- (2f * (float)Math.Cos(3f * t)) - (float)Math.Cos(4f * t);

		// y is math-up: flip so the heart points down, recentre vertically, then normalise.
		return new Vector2(x, -y - 6f) / 16f;
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
			Vector2 direction = HeartBurstDirection(Main.rand.NextFloat() * MathHelper.TwoPi);
			float speed = outerSpeed * (0.85f + (Main.rand.NextFloat() * 0.2f));

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
			Vector2 direction = HeartBurstDirection(Main.rand.NextFloat() * MathHelper.TwoPi);
			float speed = innerSpeed * (0.8f + (Main.rand.NextFloat() * 0.3f));

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

	// ---- Idle glow ---------------------------------------------------------------------
	// A dropped heart is a reward, not loot clutter: it should breathe in its tier colour so
	// it announces itself on the ground and reads by rarity in a full inventory. All of the
	// following flows from Tier, so every heart — vanilla or cross-mod — gets it for free.

	/// <summary>
	/// 0 (Common) … 1 (Mythic) ramp that drives how big and bold the idle glow is (size, alpha,
	/// copy count and pulse speed), so rarity reads at a glance — barely-there on a Common heart,
	/// unmistakable on a Mythic one. Mostly a smooth ladder, except Exotic is deliberately placed
	/// between Rare and Epic rather than near the top so its (many) boss hearts stay calm.
	/// </summary>
	private float GetRarityScale() => Tier switch
	{
		HeartTier.Common    => 0f,
		HeartTier.Uncommon  => 1f / 6f,
		HeartTier.Rare      => 2f / 6f,
		HeartTier.Exotic    => 0.42f, // intentionally between Rare and Epic
		HeartTier.Epic      => 3f / 6f,
		HeartTier.Legendary => 4f / 6f,
		HeartTier.Mythic    => 1f,
		_ => 0f,
	};

	/// <summary>Tier colour plus a 0..1 breathing value shared by the world and inventory glows.</summary>
	private Color GetGlowPulse(out float pulse)
	{
		// Higher tiers breathe a touch faster so they feel more "alive", but only gently:
		// 0.8 (Common) → 1.2 (Mythic), smoothly aligned across the tiers in between.
		float speed = 0.8f + (GetRarityScale() * 0.4f);
		pulse = 0.55f + (0.45f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * speed));
		return Tier.GetEffectColor();
	}

	/// <summary>
	/// The heart's live rarity colour. For Legendary/Exotic/Mythic this is animated (see
	/// <see cref="Content.Rarities.RarityShimmer"/>), so anything tinted with it shimmers too.
	/// </summary>
	private Color GetRarityColor() =>
		RarityLoader.GetRarity(Item.rare)?.RarityColor ?? Tier.GetEffectColor();

	/// <summary>Adds <paramref name="add"/> onto <paramref name="baseColor"/> at a fraction of its strength, clamped.</summary>
	private static Color AdditiveBlend(Color baseColor, Color add, float amount) => new(
		MathHelper.Min(1f, (baseColor.R / 255f) + (add.R / 255f * amount)),
		MathHelper.Min(1f, (baseColor.G / 255f) + (add.G / 255f * amount)),
		MathHelper.Min(1f, (baseColor.B / 255f) + (add.B / 255f * amount)));

	/// <summary>
	/// Draws many offset, tinted copies of the icon to fake a soft halo. The base tint is the
	/// tier colour with the item's (animated) rarity colour layered on additively — that rarity
	/// hue is the primary "what tier is this?" signal. Size and strength scale only gently with
	/// rarity (see <see cref="GetRarityScale"/>) so the tiers feel like a family rather than a
	/// blowout; lots of low-opacity copies keep it a smooth bloom rather than a hard ring.
	/// </summary>
	private void DrawTierGlow(SpriteBatch spriteBatch, Texture2D texture, Vector2 drawCenter,
		Rectangle? sourceRect, Vector2 origin, float rotation, float scale, float baseAlpha, float baseRadius,
		float sizeDampen = 1f)
	{
		float rarityScale = GetRarityScale();          // 0 (Common) … 1 (Mythic)
		float sizeMult = 0.7f + (rarityScale * 0.7f);  // ~0.7× up to ~1.4×
		float alphaMult = 0.7f + (rarityScale * 0.6f); // ~0.7× up to ~1.3×
		int copies = 12 + (int)((rarityScale * 12f) + 0.5f); // 12 → 24 copies

		// Rarity colour blended in firmly enough to read as the tier, while keeping the heart's
		// own material tint underneath.
		Color glow = AdditiveBlend(GetGlowPulse(out float pulse), GetRarityColor(), 0.5f);
		float radius = (baseRadius + (pulse * 1.5f)) * sizeMult * sizeDampen;
		glow *= baseAlpha * alphaMult * pulse;

		for (int i = 0; i < copies; i++)
		{
			Vector2 offset = ((MathHelper.TwoPi * i / copies) + Main.GlobalTimeWrappedHourly).ToRotationVector2() * radius;
			spriteBatch.Draw(texture, drawCenter + offset, sourceRect, glow, rotation, origin, scale, SpriteEffects.None, 0f);
		}
	}

	public override void Update(ref float gravity, ref float maxFallSpeed)
	{
		if (Main.netMode == NetmodeID.Server)
			return;

		int tierLevel = (int)Tier;
		Color tierColor = GetGlowPulse(out float pulse);

		// Real coloured light so the heart actually lights the ground around it.
		Lighting.AddLight(Item.Center, tierColor.ToVector3() * pulse * (0.35f + (tierLevel * 0.12f)));

		// Occasional rising mote — denser for the showier high tiers.
		if (Main.rand.NextBool(tierLevel >= 4 ? 9 : 20))
		{
			Dust mote = Dust.NewDustPerfect(
				Item.Center + Main.rand.NextVector2Circular(8f, 8f),
				DustID.GemDiamond, Vector2.UnitY * -Main.rand.NextFloat(0.5f), 120, tierColor, 0.7f);
			mote.noGravity = true;
		}
	}

	public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		Texture2D texture = TextureAssets.Item[Item.type].Value;

		// Animated items use a multi-frame sheet; glow with only the current frame, otherwise the
		// halo draws the whole stacked sheet behind the item.
		DrawAnimation animation = Main.itemAnimations[Item.type];
		Rectangle? frame = animation?.GetFrame(texture);
		Vector2 origin = frame.HasValue
			? new Vector2(frame.Value.Width, frame.Value.Height) / 2f
			: texture.Size() / 2f;

		// Rein in the on-ground bloom for the showy top tiers so the pulsing doesn't balloon —
		// the higher the tier, the more its maximum world size is pulled back. Inventory glow,
		// where slots are small and fixed, is left at full size.
		float worldSizeDampen = Tier switch
		{
			HeartTier.Legendary => 0.86f,
			HeartTier.Exotic    => 0.92f,
			HeartTier.Mythic    => 0.71f,
			_ => 1f,
		};

		DrawTierGlow(spriteBatch, texture, Item.Center - Main.screenPosition, frame, origin,
			rotation, scale, baseAlpha: 0.16f, baseRadius: 3f, sizeDampen: worldSizeDampen);
		return true; // still draw the heart itself on top
	}

	public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		Texture2D texture = TextureAssets.Item[Item.type].Value;
		DrawTierGlow(spriteBatch, texture, position, frame, origin, 0f, scale, baseAlpha: 0.1f, baseRadius: 2.5f);
		return true; // still draw the icon on top
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

		bool isConsumed = ElementalHeartsWorldConfig.Instance.SharedProgression
			? HeartConsumptionWorld.IsConsumed(ConsumptionId)
			: Main.LocalPlayer?.GetModPlayer<HeartConsumptionPlayer>().IsConsumedLocally(ConsumptionId) ?? false;

		if (isConsumed)
		{
			tooltips.Add(new TooltipLine(Mod, "ElementalHeartConsumed",
				Language.GetTextValue("Mods.ElementalHearts.Common.ElementalPowerActivated", ElementalPowerMaterial))
			{
				OverrideColor = Color.Gray,
			});
		}
	}
}
