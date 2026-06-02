using System;
using ElementalHearts.Common.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;

namespace ElementalHearts.Common.Players;

public class DiscordAbilityPlayer : ToggleAbilityPlayer
{
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (!Enabled)
			return;

		if (DiscordAbilitySystem.UseDiscordAbilityKeybind.JustPressed)
		{
			UseDiscordAbility();
		}
	}

	private void UseDiscordAbility()
	{
		// Cannot use while dead or frozen etc.
		if (Player.dead || Player.CCed || Player.noItems)
			return;

		Vector2 targetPos = new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);
		
		// Clamp to world bounds
		targetPos.X = MathHelper.Clamp(targetPos.X, 16f, (Main.maxTilesX * 16) - 16);
		targetPos.Y = MathHelper.Clamp(targetPos.Y, 16f, (Main.maxTilesY * 16) - 16);
		
		Player.LimitPointToPlayerReachableArea(ref targetPos);

		// Adjust so player feet match target and center horizontally
		targetPos.X -= Player.width / 2f;
		targetPos.Y -= Player.height;

		// Check for collision
		if (!Collision.SolidCollision(targetPos, Player.width, Player.height))
		{
			// Teleport
			Player.Teleport(targetPos, 1);
			NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Player.whoAmI, targetPos.X, targetPos.Y, 1);

			// Apply chaos state logic
			if (Player.HasBuff(BuffID.ChaosState))
			{
				int damage = Player.statLifeMax2 / 7;
				Player.Hurt(PlayerDeathReason.ByOther(13), damage, 0); // 13 is generic rod of discord death reason
			}

			Player.AddBuff(BuffID.ChaosState, 360);
		}
	}
}
