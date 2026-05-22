using System.Collections.Generic;
using ElementalHearts.Common.LifeShards;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Items.BossSpawns;

public abstract class MenacingHeartItem : ModItem
{
	public abstract LifeShardTier Tier { get; }
	public abstract int NPCSpawnType { get; }

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 3;
		ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
	}

	public override void SetDefaults()
	{
		Item.width = 28;
		Item.height = 28;
		Item.maxStack = Item.CommonMaxStack;
		Item.rare = Tier.GetRarityType();
		Item.useAnimation = 45;
		Item.useTime = 45;
		Item.useStyle = ItemUseStyleID.HoldUp;
		Item.UseSound = SoundID.Roar;
		Item.consumable = true;
	}

	public override bool CanUseItem(Player player)
	{
		// Cannot use if Animate is already alive
		return !NPC.AnyNPCs(NPCSpawnType);
	}

	public override bool? UseItem(Player player)
	{
		if (player.whoAmI == Main.myPlayer)
			SoundEngine.PlaySound(SoundID.Roar, player.position);

		if (Main.netMode != NetmodeID.MultiplayerClient)
			NPC.SpawnOnPlayer(player.whoAmI, NPCSpawnType);

		return true;
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		foreach (TooltipLine line in tooltips)
		{
			if (line.Mod == "Terraria" && line.Name == "ItemName")
				line.OverrideColor = Tier.GetTextColor();
		}
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.LifeCrystal, 1)
			.AddIngredient(Tier.GetItemType(), 5)
			.AddTile(TileID.DemonAltar)
			.AddCondition(new Condition("Mods.ElementalHearts.Conditions.CurrentAnimateTier", () => global::ElementalHearts.Common.Systems.AnimateProgressionSystem.UnlockedTier == (int)Tier))
			.Register();
	}
}
