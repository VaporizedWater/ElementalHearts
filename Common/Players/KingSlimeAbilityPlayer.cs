using System;
using ElementalHearts.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

public class KingSlimeAbilityPlayer : ModPlayer
{
	public bool Enabled { get; set; }

	// Combo mechanics
	private int _bounceCombo;
	private int _jumpJustPressedTimer;
	private bool _oldControlJump;
	private bool _comboLockout;

	public override void SaveData(TagCompound tag)
	{
		if (Enabled)
			tag["Enabled"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = tag.ContainsKey("Enabled");
	}

	public override void PreUpdate()
	{
		if (Player.controlJump && !_oldControlJump)
			_jumpJustPressedTimer = 0;
		else if (_jumpJustPressedTimer < 1000)
			_jumpJustPressedTimer++;

		_oldControlJump = Player.controlJump;

		CheckPowerBounceCollision();
	}

	public override void PostUpdateMiscEffects()
	{
		// Reset combo when hitting the ground (ensure they are landing, not at the apex of a jump)
		if (Player.velocity.Y == 0 && Player.oldVelocity.Y >= 0)
		{
			_bounceCombo = 0;
			_comboLockout = false;
		}

		if (_bounceCombo > 0 || _comboLockout)
		{
			// Disable midair flight and dashes during the combo AND after a failed combo, until touching ground
			Player.wingTime = 0;
			Player.rocketTime = 0;
			Player.carpetTime = 0;
			Player.dashType = 0;
		}
	}

	public override void PostUpdateRunSpeeds()
	{
		if (_bounceCombo > 0)
		{
			// Make horizontal acceleration and max speed faster during a power bounce
			// to make it easier to land the next jump
			Player.runAcceleration *= 2.5f;
			Player.maxRunSpeed *= 1.5f;
			Player.accRunSpeed *= 1.5f;
		}
	}

	private void CheckPowerBounceCollision()
	{
		if (!Enabled || Player.velocity.Y <= 0 || _comboLockout)
			return;

		Rectangle playerHitbox = Player.Hitbox;
		// Predict the exact position vanilla collision will use this frame
		playerHitbox.Offset((int)Player.velocity.X, (int)Player.velocity.Y);

		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (npc.active)
			{
				// We check if Player.Bottom.Y (current actual bottom) is above the enemy's top 80% threshold
				// while the offset playerHitbox (predicted next frame) intersects the enemy.
				if (Player.Bottom.Y <= npc.position.Y + npc.height * 0.8f && playerHitbox.Intersects(npc.Hitbox))
				{
					bool timingSuccess = false;

					if (_bounceCombo == 0)
					{
						timingSuccess = true;
					}
					else
					{
						int allowedFrames = GetAllowedFrames(_bounceCombo);
						if (_jumpJustPressedTimer <= allowedFrames)
						{
							timingSuccess = true;
							_jumpJustPressedTimer = 1000; // Consume the jump press
						}
					}

					if (timingSuccess)
					{
						_bounceCombo++;

						// Jump height scales with combo progression from 0.8x (Nice) to 1.15x (Excellent)
						int comboLevel = Math.Min(_bounceCombo, 7); // Cap at 7 (Excellent)
						float heightMultiplier = 0.8f + ((comboLevel - 1) * (0.35f / 6f));
						Player.velocity.Y = -12f * heightMultiplier;

						// Damage DEALT increases by 10% each step, capping at Excellent
						int bounceDamageToEnemy;
						if (npc.friendly || npc.type == NPCID.TargetDummy || npc.damage == 0)
						{
							// Friendly NPCs, Dummies, and 0-damage entities take exactly 1-7 damage based on the combo streak
							bounceDamageToEnemy = comboLevel;
						}
						else
						{
							// Hostile enemies take scaled damage based on their contact damage
							float damageMult = (float)Math.Pow(1.1, comboLevel - 1);
							bounceDamageToEnemy = (int)((npc.damage / 2f) * damageMult);
							if (bounceDamageToEnemy < 1)
								bounceDamageToEnemy = 1;
						}

						// Knockback scales from 2f (Nice) to 3f (Excellent)
						float knockbackValue = 2f + ((comboLevel - 1) * (1f / 6f));

						NPC.HitInfo hitInfo = new NPC.HitInfo
						{
							Damage = bounceDamageToEnemy,
							Knockback = knockbackValue,
							HitDirection = Math.Sign(npc.Center.X - Player.Center.X)
						};

						npc.StrikeNPC(hitInfo);
						if (Main.netMode != NetmodeID.SinglePlayer)
						{
							NetMessage.SendStrikeNPC(npc, hitInfo);
						}

						bool killed = !npc.active || npc.life <= 0;
						SpawnComboText(npc, _bounceCombo, killed);

						// Handle damage to player (only if they aren't already immune)
						// We explicitly do not take damage from friendly NPCs, critters, or Target Dummies.
						if (!Player.immune && npc.damage > 0 && !npc.friendly && npc.type != NPCID.TargetDummy)
						{
							// Damage RECEIVED decreases by 10% each step, capping at Excellent
							// Reduced by another 1/2 across the board as requested (base is now 1/4 of npc.damage)
							float receiveMult = (float)Math.Pow(0.9, comboLevel - 1);
							int playerDamage = (int)((npc.damage / 4f) * receiveMult);
							if (playerDamage < 1) playerDamage = 1;

							Player.HurtInfo hurt = new Player.HurtInfo
							{
								Damage = playerDamage,
								DamageSource = PlayerDeathReason.ByNPC(npc.whoAmI),
								HitDirection = 0, // No horizontal knockback from the bounce hurt
								Knockback = 0f,
								Dodgeable = true
							};
							
							Player.Hurt(hurt);
						}

						// Always grant at least a tiny bit of immunity after a successful bounce
						// to ensure vanilla collision doesn't double-hit them on the same frame.
						if (Player.immuneTime < 2)
						{
							Player.immune = true;
							Player.immuneTime = 2;
						}

						break; // Only bounce on one enemy per frame
					}
					else
					{
						// Failed timing -> reset combo and lock out the ability until they touch the ground
						_bounceCombo = 0;
						_comboLockout = true;
						
						// Break here to let vanilla collision take over and deal full contact damage/knockback
						break;
					}
				}
			}
		}
	}

	private int GetAllowedFrames(int combo)
	{
		return combo switch
		{
			1 => 30, // Second Bounce (Nice 2)
			2 => 25, // Third Bounce (Nice 3)
			3 => 20, // Fourth Bounce (Good)
			4 => 15, // Fifth Bounce (Great)
			5 => 12, // Sixth Bounce (Wonderful)
			6 => 10, // Seventh Bounce (Excellent)
			_ => 8   // Eighth+ Bounce (Excellent)
		};
	}

	private void SpawnComboText(NPC npc, int combo, bool killed)
	{
		if (Main.myPlayer != Player.whoAmI) return; // Only owner spawns the projectile to avoid dupes

		// Determine the color and text based on combo
		Projectile.NewProjectile(
			Player.GetSource_Misc("KingSlimePowerBounce"),
			npc.Top + new Vector2(0, -16f), // spawn slightly above the enemy
			new Vector2(0, -2f), // Float upwards
			ModContent.ProjectileType<KingSlimeComboTextProjectile>(),
			0, 0, Player.whoAmI, combo, killed ? 1f : 0f);
	}
}
