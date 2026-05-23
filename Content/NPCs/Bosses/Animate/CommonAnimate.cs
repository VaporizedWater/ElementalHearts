using ElementalHearts.Common.LifeShards;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using ElementalHearts.Content.Projectiles;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

[AutoloadBossHead]
public sealed class CommonAnimate : AnimateBoss
{
	public override int ProgressionTier => 0;
	public override LifeShardTier Tier => LifeShardTier.Common;
	public override SoundStyle? AmbientEmissionSound => AnimateBossSounds.CommonEmission;

	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/CommonMenacingHeart";
	public override string BossHeadTexture => "ElementalHearts/Content/Items/BossSpawns/CommonMenacingHeart";

	// State machine
	private enum State
	{
		Intro,
		Phase1_Roll,
		Hiding,
		Phase2_Spiral,
		Phase3_Dash
	}

	private State CurrentState
	{
		get => (State)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];
	private ref float Counter => ref NPC.ai[2];
	private ref float StartX => ref NPC.ai[3]; // Used for tracking X pos or sub-state

	private float PreviousState
	{
		get => NPC.localAI[1];
		set => NPC.localAI[1] = value;
	}

	public int teleportCooldown;
	public int lastHidingHpThreshold;

	// One-shot guards so each phase-transition stinger plays exactly once per fight
	private bool _enteredP2;
	private bool _enteredP3;

	private void PlayPhaseTransitionStinger(State newState)
	{
		if (newState == State.Phase2_Spiral && !_enteredP2)
		{
			_enteredP2 = true;
			SoundEngine.PlaySound(AnimateBossSounds.Phase2Transition, NPC.Center);
		}
		else if (newState == State.Phase3_Dash && !_enteredP3)
		{
			_enteredP3 = true;
			SoundEngine.PlaySound(AnimateBossSounds.Phase3Transition, NPC.Center);
		}
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		NPC.width = 22;
		NPC.height = 22;
		NPC.scale = 2.0f;
		NPC.lifeMax = 1200;
		NPC.damage = 50;
		NPC.defense = 13;
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		NPC.behindTiles = false; // Always visible

		if (!Main.dedServ)
		{
			Music = MusicLoader.GetMusicSlot(Mod, "Music/CommonAnimateTheme");
		}
	}

	public override void AI()
	{
		if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
		{
			NPC.TargetClosest();
		}

		Player player = Main.player[NPC.target];

		if (player.dead)
		{
			NPC.velocity.Y += 0.5f;
			NPC.EncourageDespawn(10);
			return;
		}
		
		if (lastHidingHpThreshold == 0)
		{
			lastHidingHpThreshold = NPC.lifeMax;
		}

		Lighting.AddLight(NPC.Center, 0.8f, 0.2f, 0.5f); // Pinkish light so always visible

		// Handle phase transitions based on health
		ManagePhases();
		
		// Anti-despawn safety teleport (1280 pixels = 80 blocks)
		if (CurrentState != State.Intro && CurrentState != State.Hiding)
		{
			if (Vector2.Distance(NPC.Center, player.Center) > 1280f)
			{
				SpawnTeleportVisuals();
				NPC.Center = player.Center - new Vector2(0, 160f); // Teleport above the player
				NPC.velocity = Vector2.Zero;
				SpawnTeleportVisuals();
			}
		}

		if (teleportCooldown > 0) teleportCooldown--;

		if (CurrentState == State.Intro || CurrentState == State.Phase1_Roll) NPC.scale = 1.2f;
		else if (CurrentState == State.Phase2_Spiral) NPC.scale = 1.1f;
		else if (CurrentState == State.Phase3_Dash) NPC.scale = 1.0f;

		switch (CurrentState)
		{
			case State.Intro:
				DoIntro(player);
				break;
			case State.Phase1_Roll:
				DoPhase1(player);
				break;
			case State.Hiding:
				DoHiding(player);
				break;
			case State.Phase2_Spiral:
				DoPhase2(player);
				break;
			case State.Phase3_Dash:
				DoPhase3(player);
				break;
		}
		
		// Clamp max velocity to prevent glitches
		float maxSpeed = 30f;
		if (NPC.velocity.Length() > maxSpeed)
		{
			NPC.velocity = Vector2.Normalize(NPC.velocity) * maxSpeed;
		}

		for (int i = NPC.oldPos.Length - 1; i > 0; i--)
		{
			NPC.oldPos[i] = NPC.oldPos[i - 1];
		}
		NPC.oldPos[0] = NPC.position;
	}

	private void ManagePhases()
	{
		float healthPct = (float)NPC.life / NPC.lifeMax;
		
		State desiredState = State.Phase1_Roll;
		if (healthPct <= 0.35f)
		{
			desiredState = State.Phase3_Dash;
		}
		else if (healthPct <= 0.70f)
		{
			desiredState = State.Phase2_Spiral;
		}

		// Only force transition if we are actively fighting (not hiding) and not already in the correct phase.
		if (CurrentState != State.Hiding && CurrentState != State.Intro && CurrentState != desiredState)
		{
			PlayPhaseTransitionStinger(desiredState);
			SpawnTeleportVisuals();
			CurrentState = desiredState;
			Timer = 0;
			Counter = 0;
			StartX = (desiredState == State.Phase1_Roll) ? NPC.Center.X : 0f;
			NPC.localAI[1] = 0f;
		}
	}

	private void SpawnTeleportVisuals()
	{
		SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
		for (int i = 0; i < 30; i++)
		{
			Dust.NewDustPerfect(NPC.Center, DustID.PinkCrystalShard, Main.rand.NextVector2CircularEdge(5f, 5f), 0, default, 1.5f).noGravity = true;
		}
	}

	private void DoIntro(Player player)
	{
		if (Timer == 0)
		{
			NPC.Center = player.Center + new Vector2(1060, -200);
			NPC.velocity = Vector2.Zero;
		}

		Timer++;
		if (Timer > 60)
		{
			SpawnTeleportVisuals();
			CurrentState = State.Phase1_Roll;
			Timer = 0;
			Counter = 0;
			StartX = NPC.Center.X;
			NPC.localAI[1] = 0f;
		}
	}

	private void DoPhase1(Player player)
	{
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		NPC.dontTakeDamage = false;
		NPC.alpha = 0;

		// Invisible Teleport Pause State
		if (Math.Abs(NPC.localAI[1]) == 3f)
		{
			NPC.velocity = Vector2.Zero;
			NPC.noGravity = true; // Stay in the air where he teleported
			NPC.dontTakeDamage = true; // Invincible while invisible
			NPC.alpha = 255;
			NPC.localAI[0]++;
			
			// Telegraph particles at the TARGET destination where he WILL appear
			Vector2 targetPos = new Vector2(NPC.localAI[2], NPC.localAI[3]);
			for (int i = 0; i < 2; i++)
			{
				Dust d = Dust.NewDustDirect(targetPos, NPC.width, NPC.height, DustID.PinkCrystalShard);
				d.velocity = Main.rand.NextVector2Circular(3f, 3f);
				d.noGravity = true;
			}

			if (NPC.localAI[0] >= 30f) // 0.5s pause
			{
				NPC.position = targetPos; // Actually teleport him now
				
				NPC.alpha = 0;
				NPC.dontTakeDamage = false;
				SpawnTeleportVisuals(); // Play second teleport sound & visuals at reappear location
				
				NPC.localAI[1] = 0f; // Will trigger sweep init below
				StartX = NPC.Center.X;
			}
			else
			{
				return;
			}
		}

		// Ground Reversal Pause State
		if (Math.Abs(NPC.localAI[1]) == 2f)
		{
			NPC.velocity.X *= 0.8f; // Skid to halt
			NPC.localAI[0]++;
			
			// Telegraph effects
			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard);
				d.velocity = new Vector2(0, -3f); // Sparks flying up
			}
			
			if (NPC.localAI[0] == 1f)
			{
				SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
			}

			if (NPC.localAI[0] >= 30f) // 0.5s pause
			{
				NPC.localAI[1] = 0f; // Will trigger sweep init below
			}
			else
			{
				// Apply gravity and basic collision so he doesn't float
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				if (NPC.velocity.X == 0 && NPC.velocity.Y == 0) NPC.velocity.Y = -6f; 
				return;
			}
		}

		// Initialize direction if it's 0
		if (NPC.localAI[1] == 0f)
		{
			NPC.localAI[1] = Math.Sign(player.Center.X - NPC.Center.X);
			if (NPC.localAI[1] == 0f) NPC.localAI[1] = 1f;
			NPC.localAI[0] = 0f; // Reset sweep timer
		}

		float dir = NPC.localAI[1];
		NPC.localAI[0]++; // Increment sweep timer

		// Lerp speed from 1.5x down to 1x over 60 ticks
		float baseSpeed = 4.0f;
		float speedMultiplier = 1f + Math.Max(0f, 0.5f * (1f - NPC.localAI[0] / 60f));
		
		NPC.velocity.X = baseSpeed * speedMultiplier * dir;
		NPC.rotation += NPC.velocity.X * 0.05f;

		// Dust trail
		if (Math.Abs(NPC.velocity.X) > 0 && NPC.velocity.Y == 0 && Main.rand.NextBool(3))
		{
			Dust.NewDust(NPC.BottomLeft, NPC.width, 4, DustID.Smoke);
		}

		// Tink sound with cooldown and variance
		if (NPC.velocity.Y == 0)
		{
			NPC.localAI[2]++;
			if (NPC.localAI[2] >= 30 && Main.rand.NextBool(30))
			{
				SoundEngine.PlaySound(SoundID.Tink, NPC.Center);
				NPC.localAI[2] = 0;
			}
		}
		else
		{
			NPC.localAI[2] = 0;
		}

		// Hop over small obstacles
		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
		if (NPC.velocity.X == 0 && NPC.velocity.Y == 0)
		{
			NPC.velocity.Y = -6f; // Jump
		}

		// Fall through platforms if the player is below him
		if (NPC.velocity.Y == 0 && player.Center.Y > NPC.Bottom.Y + 16f)
		{
			int tileX = (int)(NPC.Center.X / 16f);
			int tileY = (int)((NPC.Bottom.Y + 2f) / 16f);
			if (WorldGen.InWorld(tileX, tileY))
			{
				Tile tile = Main.tile[tileX, tileY];
				if (tile.HasTile && Main.tileSolidTop[tile.TileType])
				{
					NPC.position.Y += 2f; // Push him into the platform so he falls through
				}
			}
		}

		float distTraveled = Math.Abs(NPC.Center.X - StartX);
		float distFromPlayer = NPC.Center.X - player.Center.X;

		bool movingAway = (dir == -1f && distFromPlayer < 0) || (dir == 1f && distFromPlayer > 0);
		bool shouldTurn = false;
		bool wasStuck = false;

		// Stuck timer logic
		if (Math.Abs(NPC.position.X - NPC.oldPosition.X) < 0.5f)
		{
			NPC.localAI[3]++;
			if (NPC.localAI[3] > 120)
			{
				NPC.localAI[3] = 0;
				if (movingAway)
				{
					shouldTurn = true;
					wasStuck = true;
				}
				else
				{
					SpawnTeleportVisuals();
					NPC.position.Y -= 160f;
					while (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
						NPC.position.Y -= 16f;
				}
			}
		}
		else
		{
			NPC.localAI[3] = 0;
		}

		// Turn around if he has traveled too far (20 blocks max), or if he is moving away from the player by more than 310 pixels (~20 blocks)
		if (distTraveled > 320f) shouldTurn = true;
		if (movingAway && Math.Abs(distFromPlayer) > 310f) shouldTurn = true;

		if (shouldTurn)
		{
			if (teleportCooldown > 0)
			{
				NPC.velocity.X *= 0.8f; // Skid to a halt while waiting to teleport
			}
			else
			{
				teleportCooldown = 180; // 3 second cooldown
				Counter++;
				
				if (Counter >= 6)
				{
					StartHiding(State.Phase1_Roll);
				}
				else
				{
					if (!wasStuck && Main.rand.NextFloat() < 0.50f) // 50% chance for Ground Reversal
					{
						NPC.localAI[1] = 2f; // Enter ground reversal pause state
						NPC.localAI[0] = 0f; 
						StartX = NPC.Center.X;
					}
					else
					{
						// Instantly disappear and enter Invisible Teleport Pause state
						SpawnTeleportVisuals(); // Play first teleport sound & visuals at disappear location
						
						// Calculate target destination
						float targetY = Math.Min(NPC.position.Y - 160f, player.position.Y - 160f);
						
						// 66% chance to go straight up, 33% chance to mirror to the other side
						float targetX = NPC.position.X;
						if (Main.rand.NextFloat() < 0.33f)
						{
							float distX = NPC.position.X - player.position.X;
							targetX = player.position.X - distX;
						}
						
						Vector2 targetPos = new Vector2(targetX, targetY);
						
						while (Collision.SolidCollision(targetPos, NPC.width, NPC.height))
						{
							targetPos.Y -= 16f;
						}
						
						NPC.localAI[2] = targetPos.X;
						NPC.localAI[3] = targetPos.Y;
						
						NPC.alpha = 255; // Immediately invisible
						NPC.dontTakeDamage = true;
						
						NPC.localAI[1] = 3f; // Enter invisible teleport pause state
						NPC.localAI[0] = 0f;
					}
				}
			}
		}
	}

	private void StartHiding(State returnState)
	{
		if (NPC.life > lastHidingHpThreshold)
		{
			// Anti-stall: Skip hiding if he hasn't lost at least 5% HP since his last hide
			Timer = 0;
			Counter = 0;
			if (returnState == State.Phase1_Roll)
			{
				NPC.localAI[1] = 0f; // Reset sweep direction
			}
			return;
		}
		
		lastHidingHpThreshold = NPC.life - (int)(NPC.lifeMax * 0.05f);

		CurrentState = State.Hiding;
		Timer = -30; // 0.5s windup before the actual teleport
		PreviousState = (float)returnState;
		Counter = 0;
		NPC.localAI[0] = 0f;
		teleportCooldown = 180;
	}
	
	private void ExecuteHideTeleport()
	{
		SpawnTeleportVisuals();
		
		Vector2 bestPos = NPC.Center;
		Vector2 fallbackPos = Vector2.Zero;
		bool foundPerfectSpot = false;
		bool foundFallback = false;
		
		for (int attempts = 0; attempts < 60; attempts++)
		{
			// Hide between 80 and 100 blocks away (1280 to 1600 pixels)
			float hideDir = Main.rand.NextBool() ? -1f : 1f;
			float distance = Main.rand.NextFloat(1280f, 1600f);
			Vector2 tryPos = Main.player[NPC.target].Center + new Vector2(distance * hideDir, -400f);
			
			// Try to find ground
			for (int i = 0; i < 60; i++)
			{
				int tileX = (int)(tryPos.X / 16f);
				int tileY = (int)(tryPos.Y / 16f);
				if (WorldGen.InWorld(tileX, tileY) && Main.tile[tileX, tileY].HasTile && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType])
				{
					bool isPerfect = true;
					for (int x = -4; x <= 4; x++)
					{
						for (int y = 1; y <= 20; y++)
						{
							int checkX = tileX + x;
							int checkY = tileY - y;
							if (WorldGen.InWorld(checkX, checkY) && Main.tile[checkX, checkY].HasTile && Main.tileSolid[Main.tile[checkX, checkY].TileType] && !Main.tileSolidTop[Main.tile[checkX, checkY].TileType])
							{
								isPerfect = false;
								break;
							}
						}
						if (!isPerfect) break;
					}
					
					bool hasBasicAir = true;
					if (!isPerfect)
					{
						for (int y = 1; y <= 4; y++)
						{
							if (WorldGen.InWorld(tileX, tileY - y) && Main.tile[tileX, tileY - y].HasTile && Main.tileSolid[Main.tile[tileX, tileY - y].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY - y].TileType])
							{
								hasBasicAir = false;
								break;
							}
						}
					}
					
					if (isPerfect)
					{
						bestPos = new Vector2(tileX * 16f + 8f, tileY * 16f - (NPC.height / 2f) - 2f);
						foundPerfectSpot = true;
						break;
					}
					else if (hasBasicAir && !foundFallback)
					{
						fallbackPos = new Vector2(tileX * 16f + 8f, tileY * 16f - (NPC.height / 2f) - 2f);
						foundFallback = true;
					}
				}
				tryPos.Y += 16f;
			}
			
			if (foundPerfectSpot) break;
		}
		
		if (!foundPerfectSpot)
		{
			if (foundFallback) bestPos = fallbackPos;
			else 
			{
				float hideDir = Main.rand.NextBool() ? -1f : 1f;
				bestPos = Main.player[NPC.target].Center + new Vector2(1600f * hideDir, -200f);
			}
		}
		
		NPC.Center = bestPos;
		NPC.velocity = Vector2.Zero;
		NPC.noGravity = false; // fall onto the tile
		NPC.noTileCollide = false;
		SpawnTeleportVisuals();
	}

	private void DoHiding(Player player)
	{
		if (Timer < 0) // Pre-hide Windup!
		{
			NPC.velocity *= 0.8f;
			if (Timer == -30) SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
			if (Main.rand.NextBool(2)) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard);
			
			Timer++;
			if (Timer == 0)
			{
				ExecuteHideTeleport();
			}
			return;
		}
		
		if (Timer < 1000)
		{
			NPC.velocity.X *= 0.9f;
			NPC.alpha = 150; // Semi-transparent
			
			float phaseBonus = 1.10f; // Phase 1
			if (PreviousState == (float)State.Phase2_Spiral) phaseBonus = 1.20f;
			else if (PreviousState == (float)State.Phase3_Dash) phaseBonus = 1.30f;

			NPC.localAI[0] += (20f * NPC.lifeMax / 1200f * phaseBonus) / 60f;
			if (NPC.localAI[0] >= 1f)
			{
				int heal = (int)NPC.localAI[0];
				NPC.localAI[0] -= heal;
				if (NPC.life < NPC.lifeMax)
				{
					NPC.life += heal;
					if (NPC.life > NPC.lifeMax) NPC.life = NPC.lifeMax;
					NPC.HealEffect(heal, true);
				}
			}

			Timer++;
		}
		
		// Interrupt Dash logic
		if (Timer >= 1000) 
		{
			NPC.alpha = 0; // Fully visible
			NPC.localAI[1]++; // Use localAI[1] as the sequence timer
			NPC.noGravity = true; // Stay afloat during dash sequence

			if (NPC.localAI[1] < 30) // Telegraph
			{
				// Pull back
				NPC.velocity += Vector2.Normalize(NPC.Center - player.Center) * 0.2f;
				
				// Aim laser
				NPC.localAI[2] = player.Center.X;
				NPC.localAI[3] = player.Center.Y;

				if (Main.rand.NextBool())
				{
					Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard);
					d.velocity = Main.rand.NextVector2Circular(3f, 3f);
					d.noGravity = true;
				}
				
				if (NPC.localAI[1] == 1) SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
			}
			else if (NPC.localAI[1] == 30) // LAUNCH
			{
				teleportCooldown = 180;
				SpawnTeleportVisuals();
				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				
				Vector2 targetPos = new Vector2(NPC.localAI[2], NPC.localAI[3]);
				float healthPct = (float)NPC.life / NPC.lifeMax;
				float dashMultiplier = MathHelper.Lerp(1.5f, 2.0f, 1f - healthPct);
				NPC.velocity = Vector2.Normalize(targetPos - NPC.Center) * (12f * dashMultiplier);
			}
			else if (NPC.localAI[1] > 30) // Dashing & Cooldown
			{
				NPC.rotation += NPC.velocity.X * 0.05f;
				if (Main.rand.NextBool())
				{
					Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard);
					d.velocity = NPC.velocity * -0.5f;
				}

				if (NPC.localAI[1] > 50) NPC.velocity *= 0.96f; // Coast and smoothly decelerate
				
				if (NPC.localAI[1] > 210) // 3 seconds cooldown (30 + 180)
				{
					// Return to phase
					float hpPct = (float)NPC.life / NPC.lifeMax;
					State nextState = State.Phase1_Roll;
					if (hpPct <= 0.35f) nextState = State.Phase3_Dash;
					else if (hpPct <= 0.70f) nextState = State.Phase2_Spiral;
					
					CurrentState = nextState;
					Timer = 0;
					Counter = 0;
					NPC.localAI[0] = 0f;
					NPC.localAI[1] = 0f;
				}
			}
			return; // Skip normal hiding logic
		}

		// Wake up if 10 seconds pass without being hit
		if (Timer >= 600 && Timer < 1000)
		{
			NPC.alpha = 0;
			SpawnTeleportVisuals();
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
			for (int i = 0; i < 20; i++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch);
			}

			float healthPct = (float)NPC.life / NPC.lifeMax;
			State nextState = State.Phase1_Roll;
			if (healthPct <= 0.35f) nextState = State.Phase3_Dash;
			else if (healthPct <= 0.70f) nextState = State.Phase2_Spiral;

			// In case the player drained him into a new phase while hidden, fire the stinger here too.
			PlayPhaseTransitionStinger(nextState);

			CurrentState = nextState;
			Timer = 0;
			Counter = 0;
			StartX = (nextState == State.Phase1_Roll) ? NPC.Center.X : 0f;
			NPC.localAI[0] = 0f;
			NPC.localAI[1] = 0f;
		}
		
		// Interrupt from hit!
		if (Timer >= 0 && Timer < 1000 && (NPC.Hitbox.Intersects(player.Hitbox) || NPC.justHit))
		{
			Timer = 1000;
			NPC.localAI[1] = 0f; // Reset interrupt timer for the dash sequence
			return;
		}
	}

	private void DoPhase2(Player player)
	{
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.alpha = 0;
		
		// localAI[0] == 1f means AIMING state
		if (NPC.localAI[0] == 1f)
		{
			NPC.velocity *= 0.8f; // Slow to a halt
			NPC.localAI[1]++; // Aim timer
			
			// Lock onto player continuously
			NPC.localAI[2] = player.Center.X;
			NPC.localAI[3] = player.Center.Y;

			if (NPC.localAI[1] >= 30) // 0.5s elapsed
			{
				// Shoot
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 targetPos = new Vector2(NPC.localAI[2], NPC.localAI[3]);
					Vector2 projVelocity = Vector2.Normalize(targetPos - NPC.Center) * 6f; 
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projVelocity, ModContent.ProjectileType<AnimateShardProjectile>(), 10, 0, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
				
				// Reset state
				NPC.localAI[0] = 0f;
				NPC.localAI[1] = 0f;
			}
			return; // Don't process spiral logic while aiming
		}

		Timer++;
		
		// Slower, more deliberate spiral
		float maxRadius = 350f;
		float cycleLength = 600f; // Slower (10 seconds)
		float progress = Timer / cycleLength;
		
		if (progress > 1f)
		{
			StartHiding(State.Phase2_Spiral);
			return;
		}

		float radius = (float)Math.Sin(progress * Math.PI) * maxRadius;
		float angle = (float)(progress * Math.PI * 4f); // 2 full circles

		Vector2 targetPosSpiral = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
		
		// If off path by a huge margin (e.g. after a dash), teleport back to path
		if (Vector2.Distance(NPC.Center, targetPosSpiral) > 320f)
		{
			SpawnTeleportVisuals();
			NPC.Center = targetPosSpiral;
		}

		NPC.velocity = (targetPosSpiral - NPC.Center) * 0.05f; // Slower tracking
		NPC.rotation += 0.05f;

		// Dust trail
		if (Main.rand.NextBool(20))
		{
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard);
		}

		// Trigger aiming every 90 ticks
		if (Timer % 90 == 0)
		{
			NPC.localAI[0] = 1f; // Enter aiming mode
			NPC.localAI[1] = 0f; // Reset aim timer
		}
	}

	private void DoPhase3(Player player)
	{
		NPC.noGravity = true;
		NPC.noTileCollide = true; // Invulnerable to tiles
		NPC.alpha = 0;

		// localAI[0]: 0=Spiraling, 1=Prepare Dash, 2=Dashing, 3=Prepare Projectile
		
		if (NPC.localAI[0] == 1f) // Prepare Dash
		{
			NPC.velocity *= 0.8f;
			NPC.localAI[1]++;
			
			// Telegraph: Pull back slightly and glow
			NPC.velocity += Vector2.Normalize(NPC.Center - player.Center) * 0.2f; 
			
			// Lock onto player for telegraph line
			NPC.localAI[2] = player.Center.X;
			NPC.localAI[3] = player.Center.Y;

			if (Main.rand.NextBool())
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard);
				d.velocity = Main.rand.NextVector2Circular(3f, 3f);
				d.noGravity = true;
			}
			
			if (NPC.localAI[1] == 1) SoundEngine.PlaySound(SoundID.Item28, NPC.Center); // Magic charge sound
			
			if (NPC.localAI[1] >= 30) // 0.5s pause
			{
				NPC.localAI[0] = 2f; // Dashing
				NPC.localAI[1] = 0f;
				teleportCooldown = 180;
				SpawnTeleportVisuals();
				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				
				// Dash exactly towards where the line was pointing
				Vector2 targetPos = new Vector2(NPC.localAI[2], NPC.localAI[3]);
				float healthPct = (float)NPC.life / NPC.lifeMax;
				float dashMultiplier = MathHelper.Lerp(1.5f, 2.0f, 1f - healthPct);
				NPC.velocity = Vector2.Normalize(targetPos - NPC.Center) * (12f * dashMultiplier); // Scales up to 24f (2.0x) at 0 HP
			}
			return;
		}
		else if (NPC.localAI[0] == 2f) // Dashing
		{
			NPC.rotation += NPC.velocity.X * 0.05f; // Spin relative to speed
			
			if (Main.rand.NextBool())
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard);
				d.velocity = NPC.velocity * -0.5f; // Dust trailing backwards
			}

			NPC.localAI[1]++;
			if (NPC.localAI[1] > 20) // After 20 ticks start decelerating smoothly
			{
				NPC.velocity *= 0.96f; // Coast and smooth deceleration
			}
			
			if (NPC.localAI[1] > 180) // 3 seconds cooldown total
			{
				NPC.localAI[0] = 0f; // Back to spiraling
				NPC.localAI[1] = 0f;
			}
			return;
		}
		else if (NPC.localAI[0] == 3f) // Prepare Projectile
		{
			NPC.velocity *= 0.8f; // Slow to a halt
			NPC.localAI[1]++; // Aim timer
			
			// Lock onto player continuously
			NPC.localAI[2] = player.Center.X;
			NPC.localAI[3] = player.Center.Y;
			
			if (NPC.localAI[1] == 1) SoundEngine.PlaySound(SoundID.Item15, NPC.Center); // Laser charge sound

			if (NPC.localAI[1] >= 30) // 0.5s elapsed
			{
				// Shoot
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 targetPos = new Vector2(NPC.localAI[2], NPC.localAI[3]);
					Vector2 projVelocity = Vector2.Normalize(targetPos - NPC.Center) * 6f; 
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projVelocity, ModContent.ProjectileType<AnimateShardProjectile>(), 10, 0, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
				
				// Reset state
				NPC.localAI[0] = 0f;
				NPC.localAI[1] = 0f;
			}
			return;
		}

		// Spiraling (localAI[0] == 0)
		Timer++;
		
		// Slower spiral (slower than Phase 2's 600)
		float maxRadius = 400f;
		float cycleLength = 900f; // 15 seconds
		float progress = Timer / cycleLength;
		
		if (progress > 1f)
		{
			StartHiding(State.Phase3_Dash);
			return;
		}

		float radius = (float)Math.Sin(progress * Math.PI) * maxRadius;
		float angle = (float)(progress * Math.PI * 6f); // 3 full circles

		Vector2 targetPosSpiral = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
		
		// If off path by a huge margin (e.g. after a dash), teleport back to path
		if (Vector2.Distance(NPC.Center, targetPosSpiral) > 320f)
		{
			SpawnTeleportVisuals();
			NPC.Center = targetPosSpiral;
		}

		NPC.velocity = (targetPosSpiral - NPC.Center) * 0.04f; // Slower tracking
		NPC.rotation += 0.05f;

		if (Main.rand.NextBool(20))
		{
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard);
		}

		// Trigger attacks every 60 ticks (1s)
		if (Timer % 60 == 0)
		{
			Counter++; // Use Counter to track attack pattern
			if (Counter % 3 == 0) // Predictable pattern: 1 Dash every 3 attacks
			{
				NPC.localAI[0] = 1f; // Prepare Dash
				NPC.localAI[1] = 0f;
			}
			else // The other 2 are Projectiles
			{
				NPC.localAI[0] = 3f; // Prepare Projectile
				NPC.localAI[1] = 0f;
			}
		}
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		bool isHidingDash = CurrentState == State.Hiding && Timer >= 1000 && NPC.localAI[1] < 30;

		// Draw telegraph line
		if ((CurrentState == State.Phase2_Spiral && NPC.localAI[0] == 1f) || (CurrentState == State.Phase3_Dash && (NPC.localAI[0] == 3f || NPC.localAI[0] == 1f)) || isHidingDash)
		{
			float aimProgress = NPC.localAI[1] / 30f; // 0 to 1
			Color lineColor = Color.HotPink * aimProgress; // Fades in

			Vector2 targetPos = new Vector2(NPC.localAI[2], NPC.localAI[3]);
			Vector2 startPos = NPC.Center - screenPos;
			Vector2 endPos = targetPos - screenPos;

			Texture2D magicPixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			Texture2D glowTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
			Vector2 glowOrigin = new Vector2(32f, 32f);
			
			float angle = (endPos - startPos).ToRotation();

			// Thinner base line for dashes so it isn't WAY too big
			float baseThickness = ((CurrentState == State.Phase3_Dash && NPC.localAI[0] == 1f) || isHidingDash) ? 3f : 2f;

			// Switch to Additive Blending for a true glowing laser effect
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			// Shoot the beam infinitely off-screen so you never see a blunt tip
			float beamLength = 3000f;

			// 1. Draw the smooth outer aura (No overlapping staggered layers!)
			float auraThickness = baseThickness * 4f;
			Color auraColor = lineColor * 0.8f;
			spriteBatch.Draw(magicPixel, startPos, new Rectangle(0, 0, 1, 1), auraColor, angle, new Vector2(0, 0.5f), new Vector2(beamLength, auraThickness), SpriteEffects.None, 0f);
			
			// 2. Draw a massive soft glowing orb at the beginning of the laser so it emerges beautifully from the boss
			spriteBatch.Draw(glowTex, startPos, null, auraColor, 0f, glowOrigin, auraThickness / 20f, SpriteEffects.None, 0f);

			// 3. Draw the intense bright inner core
			float coreThickness = baseThickness * 1.5f;
			Color coreColor = Color.White * aimProgress;
			spriteBatch.Draw(magicPixel, startPos, new Rectangle(0, 0, 1, 1), coreColor, angle, new Vector2(0, 0.5f), new Vector2(beamLength, coreThickness), SpriteEffects.None, 0f);
			
			// 4. Cap the inner core with an intense white orb
			spriteBatch.Draw(glowTex, startPos, null, coreColor, 0f, glowOrigin, coreThickness / 20f, SpriteEffects.None, 0f);

			// Restore original blend state
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
		}
		
		return true; // Draw the boss normally
	}
}
