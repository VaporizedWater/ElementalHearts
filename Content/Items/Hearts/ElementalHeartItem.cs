using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Systems;
using Terraria;
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
	/// Stable identifier used to record consumption. Defaults to the class name; only override
	/// if a class is renamed and the original ID must be preserved for save compatibility.
	/// </summary>
	public virtual string ConsumptionId => Name;

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

	public int HpGain => Tier.GetHpGain();

	protected int RecipeCost(int baseAmount)
	{
		float multiplier = ElementalHeartsRecipeConfig.Instance.RecipeDifficulty / 10f;
		int raw = (int)System.Math.Round(baseAmount * multiplier);

		if (raw <= 5) return System.Math.Max(1, raw);

		int step;
		if (raw <= 50) step = 5;
		else if (raw <= 100) step = 5;
		else if (raw <= 500) step = 25;
		else if (raw <= 1000) step = 50;
		else step = 100;

		// Round to the nearest step, but if it's exactly in between, round up
		int rounded = (int)System.Math.Round(raw / (float)step, System.MidpointRounding.AwayFromZero) * step;
		
		// If the user provided specific examples like 210 -> 225 and 42 -> 45, 
		// they are effectively rounding UP to the nearest step. Let's do ceiling to match their examples exactly.
		int ceilingRounded = (int)System.Math.Ceiling(raw / (float)step) * step;

		// Actually, standard rounding is more intuitive long-term, but since they explicitly
		// said 210 -> 225 (which is up to 25) and 42 -> 45 (which is up to 5),
		// it seems they prefer rounding UP to the next "nice" number.
		return System.Math.Max(1, ceilingRounded);
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
		Item.maxStack = 9999;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.useAnimation = 30;
		Item.useTime = 30;
		Item.UseSound = SoundID.Item3;
		Item.consumable = false;
		Item.rare = Tier.GetRarityType();
		Item.value = Item.sellPrice(silver: (int)Tier * 25);
	}

	public override bool CanUseItem(Player player)
	{
		return !HeartConsumptionWorld.IsConsumed(ConsumptionId);
	}

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI != Main.myPlayer)
			return false;

		bool consumed = HeartConsumptionWorld.TryConsume(ConsumptionId, HpGain);
		if (consumed)
		{
			PlayShockwave(player);
		}
		return consumed;
	}

	protected virtual void PlayShockwave(Player player)
	{
		float effectMultiplier = ElementalHeartsVisualConfig.Instance.ConsumptionEffectStrength / 3f;

		// 1. Play baseline universal sound
		// Higher tier = slightly deeper pitch
		float pitch = 0.5f - ((int)Tier * 0.1f);
		Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4.WithPitchOffset(pitch), player.Center);

		// 2. Procedural Vibrant Colors
		System.Random rand = new System.Random(Type);
		
		// Generate a highly vibrant, saturated, beautiful HSL color
		float hue = rand.Next(0, 360) / 360f;
		Microsoft.Xna.Framework.Color customColor = Main.hslToRgb(hue, 1f, 0.6f);

		// Scaling factors
		int tierLevel = (int)Tier;
		int dustCount = (int)((30 + (tierLevel * 15)) * effectMultiplier);
		float dustSpeed = (3f + (tierLevel * 1.5f)) * effectMultiplier;

		// Premium, dopamine-inducing colorful dust types in Terraria
		int[] dustTypes = new int[] { 
			DustID.FireworksRGB, 
			DustID.CrystalSerpent, 
			DustID.Shadowflame, 
			DustID.Clentaminator_Cyan, 
			DustID.Clentaminator_Purple, 
			DustID.Clentaminator_Red 
		};
		int selectedDust = dustTypes[rand.Next(dustTypes.Length)];

		// Ring 1: Fast expanding spherical premium shockwave blast
		for (int i = 0; i < dustCount; i++)
		{
			Microsoft.Xna.Framework.Vector2 direction = new Microsoft.Xna.Framework.Vector2(0, -1).RotatedByRandom(Microsoft.Xna.Framework.MathHelper.TwoPi);
			float speed = dustSpeed * (0.8f + (float)rand.NextDouble() * 0.4f);
			float scale = (2.0f + (tierLevel * 0.3f)) * effectMultiplier;
			
			Dust dust = Dust.NewDustPerfect(player.Center, selectedDust, direction * speed, 0, customColor, scale);
			dust.noGravity = true;
			dust.fadeIn = 1.2f;
			
			// Ring 2: Slower sparkling inner glow (tinted to match customColor perfectly!)
			Microsoft.Xna.Framework.Vector2 slowVelocity = direction * speed * 0.4f;
			float innerScale = (1.5f + (tierLevel * 0.2f)) * effectMultiplier;
			Dust dust2 = Dust.NewDustPerfect(player.Center, DustID.GemDiamond, slowVelocity, 100, customColor, innerScale);
			dust2.noGravity = true;
		}

		// 3. Screen Shake for ALL tiers (scaled by tier level and visual config multiplier)
		float shakeStrength = (1.5f + (tierLevel * 1.5f)) * effectMultiplier;
		Microsoft.Xna.Framework.Vector2 punchDir = new Microsoft.Xna.Framework.Vector2(0, -1).RotatedByRandom(Microsoft.Xna.Framework.MathHelper.TwoPi);
		Terraria.Graphics.CameraModifiers.PunchCameraModifier modifier = new Terraria.Graphics.CameraModifiers.PunchCameraModifier(
			player.Center, 
			punchDir, 
			shakeStrength, 
			5f + tierLevel, // vibration speed
			15 + (tierLevel * 5), // duration frames
			1000f, 
			Name);
		Main.instance.CameraModifiers.Add(modifier);

		// 4. Trigger High-Intensity Ambient Colored Lighting Flash
		if (Main.netMode != NetmodeID.Server)
		{
			Lighting.AddLight(player.Center, customColor.ToVector3() * (2.5f + tierLevel * 0.8f) * effectMultiplier);
		}
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		tooltips.Add(new TooltipLine(Mod, "ElementalHeartHp",
			Language.GetTextValue("Mods.ElementalHearts.Common.HpGain", HpGain)));

		if (HeartConsumptionWorld.IsConsumed(ConsumptionId))
		{
			tooltips.Add(new TooltipLine(Mod, "ElementalHeartConsumed",
				Language.GetTextValue("Mods.ElementalHearts.Common.ElementalPowerActivated", ElementalPowerMaterial))
			{
				OverrideColor = Microsoft.Xna.Framework.Color.Gray,
			});
		}
	}
}
