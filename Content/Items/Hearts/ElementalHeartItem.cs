using System.Collections.Generic;
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

		return HeartConsumptionWorld.TryConsume(ConsumptionId, HpGain);
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
