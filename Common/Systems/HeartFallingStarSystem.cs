using System.Collections.Generic;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Systems;

public class HeartFallingStarSystem : ModSystem
{
	private int droppedThisNight = 0;
	private bool eventActiveThisNight = false;
	private bool wasDayTime = true;

	public override void PostUpdateWorld()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
			return;

		bool isDay = Main.dayTime;

		// Transition from day to night
		if (wasDayTime && !isDay)
		{
			droppedThisNight = 0;
			eventActiveThisNight = false;

			if (Main.rand.Next(100) < ElementalHeartsServerConfig.Instance.WorldGen.HeartShootingStarChance)
			{
				eventActiveThisNight = true;
			}
		}

		wasDayTime = isDay;

		// During night
		if (!isDay && eventActiveThisNight && droppedThisNight < ElementalHeartsServerConfig.Instance.WorldGen.HeartShootingStarMaxPerNight)
		{
			// Try spawn based on frequency
			float chancePerTick = (1f / 10000f) * (ElementalHeartsServerConfig.Instance.WorldGen.HeartShootingStarFrequency / 100f);
			
			if (Main.rand.NextFloat() < chancePerTick)
			{
				TrySpawnFallingHeart();
			}
		}
	}

	private void TrySpawnFallingHeart()
	{
		// Find a valid player
		List<Player> activePlayers = new List<Player>();
		for (int i = 0; i < Main.maxPlayers; i++)
		{
			if (Main.player[i].active && !Main.player[i].dead)
				activePlayers.Add(Main.player[i]);
		}

		if (activePlayers.Count == 0)
			return;

		Player target = activePlayers[Main.rand.Next(activePlayers.Count)];

		// Find a valid heart
		HeartTier currentTier = (HeartTier)AnimateProgressionSystem.UnlockedTier;
		List<ElementalHeartItem> validHearts = new List<ElementalHeartItem>();

		foreach (ElementalHeartItem heart in ModContent.GetContent<ElementalHeartItem>())
		{
			if (heart.Tier == currentTier)
			{
				if (CraftableHeartSellValueSystem.TryGetSellValue(heart.Type, out _) &&
					!HeartConsumptionWorld.IsConsumed(heart.ConsumptionId))
				{
					validHearts.Add(heart);
				}
			}
		}

		if (validHearts.Count == 0)
			return;

		ElementalHeartItem selectedHeart = validHearts[Main.rand.Next(validHearts.Count)];

		// Spawn it high above the player, slightly randomized
		Vector2 spawnPosition = target.Center;
		spawnPosition.Y -= Main.rand.Next(800, 1200);
		spawnPosition.X += Main.rand.Next(-500, 500);

		// Calculate velocity so it falls similarly to a star
		Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(8f, 12f));

		Projectile.NewProjectile(
			target.GetSource_Misc("HeartFallingStar"),
			spawnPosition,
			velocity,
			ModContent.ProjectileType<FallingHeartProjectile>(),
			0, // Damage
			0f, // Knockback
			Main.myPlayer,
			selectedHeart.Type // ai[0] is the item type
		);

		droppedThisNight++;
	}

	public override void SaveWorldData(TagCompound tag)
	{
		tag["HeartShootingStarDropped"] = droppedThisNight;
		tag["HeartShootingStarActive"] = eventActiveThisNight;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		droppedThisNight = tag.GetInt("HeartShootingStarDropped");
		eventActiveThisNight = tag.GetBool("HeartShootingStarActive");
	}
}
