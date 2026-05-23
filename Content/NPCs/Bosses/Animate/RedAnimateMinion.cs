using ElementalHearts.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

public sealed class RedAnimateMinion : ModNPC
{
	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/CommonMenacingHeart";

	// Cmd values written by UncommonAnimate
	public const int CmdIdleAir = 0;          // Track toward (TargetX, TargetY) in the air
	public const int CmdP1Roll = 1;           // Autonomous ground rolling (Common's Phase1)
	public const int CmdTelegraphShoot = 2;   // Telegraph (TelegraphDuration ticks, default 30), then fire shard toward (TargetX, TargetY). Auto-returns to CmdIdleAir.
	public const int CmdTelegraphDash = 3;    // Telegraph (TelegraphDuration ticks, default 30), then dash toward (TargetX, TargetY). Auto-returns to CmdIdleAir after the dash.
	public const int CmdFallToGround = 4;     // Drift toward TargetX while falling, then auto-switches to CmdIdleGround.
	public const int CmdIdleGround = 5;       // Stand still on ground.
	public const int CmdDespawn = 6;          // Poof and remove.
	public const int CmdP2P3Roll = 7;         // Teleport to ground and roll at player.
	public const int CmdDashImmediate = 8;    // Set velocity to (TargetX, TargetY) and dash for TelegraphDuration ticks (no telegraph). Auto-returns to CmdIdleAir.

	private ref float Cmd => ref NPC.ai[0];
	private ref float TelegraphDuration => ref NPC.ai[1];
	private ref float TargetX => ref NPC.ai[2];
	private ref float TargetY => ref NPC.ai[3];

	private float Sub0 { get => NPC.localAI[0]; set => NPC.localAI[0] = value; }
	private float Sub1 { get => NPC.localAI[1]; set => NPC.localAI[1] = value; }
	private float Sub2 { get => NPC.localAI[2]; set => NPC.localAI[2] = value; }
	private float Sub3 { get => NPC.localAI[3]; set => NPC.localAI[3] = value; }

	private int _lastCmd = -1;
	private float _rollStartX;

	// Polish — scale pulse decays toward 1 each tick
	private const float BaseScale = 1.2f;
	private float _scalePulse = 1f;
	private void PulseScale(float amount) { if (amount > _scalePulse) _scalePulse = amount; }

	// Wipe oldPos[] so the segmented trail in PreDraw doesn't streak across the gap after a teleport.
	private void ResetTrail()
	{
		for (int i = 0; i < NPC.oldPos.Length; i++)
			NPC.oldPos[i] = NPC.position;
	}

	// Exposed so the boss can read mid-action progress (e.g., "is the dash done?")
	public bool ActionInProgress => Cmd == CmdTelegraphShoot || Cmd == CmdTelegraphDash || Cmd == CmdFallToGround || Cmd == CmdDashImmediate;

	public bool IsOnGround() => Cmd == CmdIdleGround;

	// --- Command setters, invoked by UncommonAnimate ---
	public void Cmd_SetIdleAir(Vector2 target)
	{
		Cmd = CmdIdleAir;
		TelegraphDuration = 0f;
		TargetX = target.X;
		TargetY = target.Y;
	}

	public void SetIdleTarget(Vector2 target)
	{
		TargetX = target.X;
		TargetY = target.Y;
	}

	public void Cmd_SetP1Roll()
	{
		if (Cmd != CmdP1Roll)
		{
			Cmd = CmdP1Roll;
			TelegraphDuration = 0f;
		}
	}

	public void Cmd_SetTelegraphShoot(Vector2 target, float duration = 30f)
	{
		Cmd = CmdTelegraphShoot;
		TelegraphDuration = duration;
		TargetX = target.X;
		TargetY = target.Y;
	}

	public void Cmd_SetTelegraphDash(Vector2 target, float duration = 30f)
	{
		Cmd = CmdTelegraphDash;
		TelegraphDuration = duration;
		TargetX = target.X;
		TargetY = target.Y;
	}

	public void Cmd_SetFallToGround(float targetX)
	{
		Cmd = CmdFallToGround;
		TelegraphDuration = 0f;
		TargetX = targetX;
		TargetY = 0f;
	}

	public void Cmd_SetDespawn()
	{
		Cmd = CmdDespawn;
	}

	public void Cmd_SetP2P3Roll()
	{
		Cmd = CmdP2P3Roll;
		TelegraphDuration = 0f;
	}

	// Launch an instant dash with the given velocity for `duration` ticks. No telegraph;
	// boss has already drawn the laser externally during a longer charge-up.
	public void Cmd_SetDashImmediate(Vector2 velocity, float duration = 55f)
	{
		Cmd = CmdDashImmediate;
		TargetX = velocity.X;
		TargetY = velocity.Y;
		TelegraphDuration = duration;
	}

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 1;
		NPCID.Sets.CantTakeLunchMoney[NPC.type] = true;
		NPCID.Sets.NPCBestiaryDrawOffset[NPC.type] = new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true };
		NPCID.Sets.TrailCacheLength[NPC.type] = 8;
	}

	public override void SetDefaults()
	{
		NPC.width = 14;
		NPC.height = 14;
		NPC.damage = 40;
		NPC.defense = 0;
		NPC.lifeMax = 1;
		NPC.HitSound = SoundID.NPCHit1;
		NPC.knockBackResist = 0f;
		NPC.aiStyle = -1;
		NPC.npcSlots = 0f;
		NPC.value = 0;
		NPC.dontTakeDamage = true;
		NPC.dontCountMe = true;
		NPC.immortal = true;
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		NPC.scale = 2.0f;
		NPC.alpha = 0;
	}

	public override bool CheckActive() => false;
	public override bool? CanBeHitByItem(Player player, Item item) => false;
	public override bool? CanBeHitByProjectile(Projectile projectile) => false;

	public override void AI()
	{
		if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
		{
			NPC.TargetClosest();
		}
		Player player = Main.player[NPC.target];
		if (player.dead)
		{
			NPC.EncourageDespawn(10);
			return;
		}

		Lighting.AddLight(NPC.Center, 0.9f, 0.2f, 0.2f);

		int cmd = (int)Cmd;
		if (cmd != _lastCmd)
		{
			Sub0 = 0f; Sub1 = 0f; Sub2 = 0f; Sub3 = 0f;
			_lastCmd = cmd;
		}

		switch (cmd)
		{
			case CmdIdleAir: DoIdleAir(player); break;
			case CmdP1Roll: DoP1Roll(player); break;
			case CmdTelegraphShoot: DoTelegraphShoot(player); break;
			case CmdTelegraphDash: DoTelegraphDash(player); break;
			case CmdFallToGround: DoFallToGround(player); break;
			case CmdIdleGround: DoIdleGround(player); break;
			case CmdDespawn: DoDespawn(); break;
			case CmdP2P3Roll: DoP2P3Roll(player); break;
			case CmdDashImmediate: DoDashImmediate(player); break;
		}

		if (NPC.velocity.Length() > 30f)
			NPC.velocity = Vector2.Normalize(NPC.velocity) * 30f;

		// Decay pulse
		if (_scalePulse > 1f) _scalePulse = MathHelper.Lerp(_scalePulse, 1f, 0.12f);
		NPC.scale = BaseScale * _scalePulse;

		for (int i = NPC.oldPos.Length - 1; i > 0; i--)
		{
			NPC.oldPos[i] = NPC.oldPos[i - 1];
		}
		NPC.oldPos[0] = NPC.position;
	}

	public void PoofVisuals()
	{
		SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
		for (int i = 0; i < 30; i++)
		{
			Dust.NewDustPerfect(NPC.Center, DustID.RedTorch, Main.rand.NextVector2CircularEdge(5f, 5f), 0, default, 1.5f).noGravity = true;
		}
	}

	// === IDLE AIR: smoothly track (TargetX, TargetY) target position ===
	private void DoIdleAir(Player player)
	{
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		Vector2 target = new Vector2(TargetX, TargetY);
		Vector2 delta = target - NPC.Center;
		// Hard-warp if absurdly far so we don't lag visually
		if (delta.Length() > 1200f)
		{
			NPC.Center = target;
			NPC.velocity = Vector2.Zero;
			ResetTrail();
		}
		else
		{
			NPC.velocity = delta * 0.15f;
		}
		NPC.rotation += 0.05f;
		if (Main.rand.NextBool(20))
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
	}

	// === IDLE GROUND: stay still, slight drag, gravity on ===
	private void DoIdleGround(Player player)
	{
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		NPC.velocity.X *= 0.85f;
		NPC.rotation *= 0.9f;
	}

	// === P1 ROLL: ported from CommonAnimate.DoPhase1, minus the hide-trigger ===
	// Uses Sub0=sweep timer, Sub1=direction/sub-state, Sub2=tink timer/target X, Sub3=stuck timer/target Y
	private void DoP1Roll(Player player)
	{
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		NPC.alpha = 0;

		// Invisible teleport pause state
		if (Math.Abs(Sub1) == 3f)
		{
			NPC.velocity = Vector2.Zero;
			NPC.noGravity = true;
			NPC.alpha = 255;
			Sub0++;

			Vector2 targetPos = new(Sub2, Sub3);
			for (int i = 0; i < 2; i++)
			{
				Dust d = Dust.NewDustDirect(targetPos, NPC.width, NPC.height, DustID.RedTorch);
				d.velocity = Main.rand.NextVector2Circular(3f, 3f);
				d.noGravity = true;
			}

			if (Sub0 >= 90f)
			{
				NPC.position = targetPos;
				NPC.alpha = 0;
				ResetTrail();
				PoofVisuals();
				Sub1 = 0f;
			}
			else
			{
				return;
			}
		}

		// Ground reversal pause state
		if (Math.Abs(Sub1) == 2f)
		{
			NPC.velocity.X *= 0.8f;
			Sub0++;
			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
				d.velocity = new Vector2(0, -3f);
			}
			if (Sub0 == 1f) SoundEngine.PlaySound(SoundID.Item28 with { PitchVariance = 0.2f }, NPC.Center);
			if (Sub0 >= 90f)
			{
				Sub1 = 0f;
			}
			else
			{
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				if (NPC.velocity.X == 0 && NPC.velocity.Y == 0) NPC.velocity.Y = -6f;
				return;
			}
		}

		// Initialize direction
		if (Sub1 == 0f)
		{
			Sub1 = Math.Sign(player.Center.X - NPC.Center.X);
			if (Sub1 == 0f) Sub1 = 1f;
			Sub0 = 0f;
			_rollStartX = NPC.Center.X;
		}

		float dir = Sub1;
		Sub0++;

		float baseSpeed = 4.0f;
		float speedMultiplier = 1f + Math.Max(0f, 0.5f * (1f - Sub0 / 60f));
		NPC.velocity.X = baseSpeed * speedMultiplier * dir;
		NPC.rotation += NPC.velocity.X * 0.05f;

		if (Math.Abs(NPC.velocity.X) > 0 && NPC.velocity.Y == 0 && Main.rand.NextBool(3))
			Dust.NewDust(NPC.BottomLeft, NPC.width, 4, DustID.Smoke);

		if (NPC.velocity.Y == 0)
		{
			Sub2++;
			if (Sub2 >= 30 && Main.rand.NextBool(30))
			{
				SoundEngine.PlaySound(SoundID.Tink with { PitchVariance = 0.2f }, NPC.Center);
				Sub2 = 0;
			}
		}
		else
		{
			Sub2 = 0;
		}

		// Step over small bumps but do NOT bunny-hop on stuck. The stuck timer below
		// handles turn-around / teleport-up for taller obstacles.
		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

		// Fall through platforms if player is below
		if (NPC.velocity.Y == 0 && player.Center.Y > NPC.Bottom.Y + 16f)
		{
			int tileX = (int)(NPC.Center.X / 16f);
			int tileY = (int)((NPC.Bottom.Y + 2f) / 16f);
			if (WorldGen.InWorld(tileX, tileY))
			{
				Tile tile = Main.tile[tileX, tileY];
				if (tile.HasTile && Main.tileSolidTop[tile.TileType])
					NPC.position.Y += 2f;
			}
		}

		float distTraveled = Math.Abs(NPC.Center.X - _rollStartX);
		float distFromPlayer = NPC.Center.X - player.Center.X;
		bool movingAway = (dir == -1f && distFromPlayer < 0) || (dir == 1f && distFromPlayer > 0);
		bool shouldTurn = false;
		bool wasStuck = false;

		if (Math.Abs(NPC.position.X - NPC.oldPosition.X) < 0.5f)
		{
			Sub3++;
			if (Sub3 > 120)
			{
				Sub3 = 0;
				if (movingAway)
				{
					shouldTurn = true;
					wasStuck = true;
				}
				else
				{
					PoofVisuals();
					NPC.position.Y -= 160f;
					while (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
						NPC.position.Y -= 16f;
				}
			}
		}
		else
		{
			Sub3 = 0;
		}

		if (distTraveled > 480f) shouldTurn = true;
		if (movingAway && Math.Abs(distFromPlayer) > 480f) shouldTurn = true;

		if (shouldTurn)
		{
			if (!wasStuck && Main.rand.NextFloat() < 0.50f)
			{
				Sub1 = 2f;
				Sub0 = 0f;
				_rollStartX = NPC.Center.X;
			}
			else
			{
				PoofVisuals();
				float targetY = Math.Min(NPC.position.Y - 160f, player.position.Y - 160f);
				float targetX = NPC.position.X;
				if (Main.rand.NextFloat() < 0.33f)
				{
					float distX = NPC.position.X - player.position.X;
					targetX = player.position.X - distX;
				}
				Vector2 targetPos = new(targetX, targetY);
				while (Collision.SolidCollision(targetPos, NPC.width, NPC.height))
					targetPos.Y -= 16f;

				Sub2 = targetPos.X;
				Sub3 = targetPos.Y;
				NPC.alpha = 255;
				Sub1 = 3f;
				Sub0 = 0f;
			}
		}
	}

	private float EffectiveTelegraphDuration => TelegraphDuration > 0f ? TelegraphDuration : 30f;

	// === TELEGRAPH SHOOT: laser telegraph, then fire AnimateShardProjectile toward (TargetX, TargetY) ===
	// Sub0 = timer
	private void DoTelegraphShoot(Player player)
	{
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.velocity *= 0.85f;
		Sub0++;

		if (Sub0 == 1f) SoundEngine.PlaySound(SoundID.Item15 with { PitchVariance = 0.2f }, NPC.Center);

		if (Main.rand.NextBool(2))
		{
			Vector2 spawnPos = NPC.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
			Dust d = Dust.NewDustPerfect(spawnPos, DustID.RedTorch);
			d.velocity = (NPC.Center - spawnPos) * 0.08f;
			d.noGravity = true;
		}

		if (Sub0 >= EffectiveTelegraphDuration)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Vector2 targetPos = new(TargetX, TargetY);
				Vector2 vel = Vector2.Normalize(targetPos - NPC.Center) * 9f;
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<AnimateShardProjectile>(), 10, 0, Main.myPlayer);
			}
			SoundEngine.PlaySound(SoundID.Item8 with { PitchVariance = 0.2f }, NPC.Center);
			PulseScale(1.18f);
			NPC.velocity += Vector2.Normalize(NPC.Center - new Vector2(TargetX, TargetY)) * 1.4f;
			Cmd = CmdIdleAir;
			TelegraphDuration = 0f;
			_lastCmd = (int)Cmd;
			Sub0 = 0f;
		}
	}

	// === TELEGRAPH DASH: laser telegraph (TelegraphDuration ticks), then dash to (TargetX, TargetY) ===
	// Sub0 = timer, Sub1 = post-dash counter
	private void DoTelegraphDash(Player player)
	{
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		if (Sub1 == 0f) // Telegraph phase
		{
			NPC.velocity = Vector2.Zero;
			Sub0++;

			if (Sub0 == 1f) SoundEngine.PlaySound(SoundID.Item28 with { PitchVariance = 0.2f }, NPC.Center);

			if (Main.rand.NextBool(2))
			{
				Vector2 spawnPos = NPC.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
				Dust d = Dust.NewDustPerfect(spawnPos, DustID.RedTorch);
				d.velocity = (NPC.Center - spawnPos) * 0.08f;
				d.noGravity = true;
			}

			if (Sub0 >= EffectiveTelegraphDuration)
			{
				PoofVisuals();
				SoundEngine.PlaySound(SoundID.Roar with { PitchVariance = 0.1f }, NPC.Center);
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				Vector2 targetPos = new(TargetX, TargetY);
				NPC.velocity = Vector2.Normalize(targetPos - NPC.Center) * 20f;
				PulseScale(1.30f);
				Sub1 = 1f;
				Sub0 = 0f;
			}
		}
		else // Dashing — full-commit, no decay so he never visibly halts mid-dash
		{
			NPC.rotation += NPC.velocity.X * 0.05f;
			Sub0++;
			if (Main.rand.NextBool())
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
				d.velocity = NPC.velocity * -0.5f;
			}
			if (Sub0 > 55f)
			{
				Cmd = CmdIdleAir;
				TelegraphDuration = 0f;
				_lastCmd = (int)Cmd;
			}
		}
	}

	// === FALL TO GROUND: drift to TargetX while falling. After landing or timeout, switch to CmdIdleGround. ===
	// Sub0 = fall timer
	private void DoFallToGround(Player player)
	{
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		Sub0++;
		float dx = TargetX - NPC.Center.X;
		NPC.velocity.X = MathHelper.Clamp(dx * 0.05f, -8f, 8f);

		if (Main.rand.NextBool(8))
		{
			Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
			d.noGravity = true;
		}

		// Landed?
		if (NPC.velocity.Y == 0 && Sub0 > 5f)
		{
			Cmd = CmdIdleGround;
			_lastCmd = (int)Cmd;
		}
		// Timeout — hover where we are
		else if (Sub0 > 90f)
		{
			NPC.noGravity = true;
			NPC.velocity = Vector2.Zero;
			Cmd = CmdIdleGround;
			_lastCmd = (int)Cmd;
		}
	}

	// === DESPAWN: poof and remove ===
	private void DoDespawn()
	{
		PoofVisuals();
		NPC.active = false;
	}

	// === DASH IMMEDIATE: launch with (TargetX, TargetY) as velocity vector; no telegraph.
	// Used by Move 3 Ground Sweep where the boss draws the telegraph externally so it can
	// track the player's Y until the very last frame before launch. ===
	private void DoDashImmediate(Player player)
	{
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		if (Sub0 == 0f)
		{
			// First-tick launch: stamp velocity from the passed-in vector
			NPC.velocity = new Vector2(TargetX, TargetY);
			PoofVisuals();
			SoundEngine.PlaySound(SoundID.Roar with { PitchVariance = 0.1f }, NPC.Center);
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
			PulseScale(1.30f);
		}

		NPC.rotation += NPC.velocity.X * 0.05f;
		Sub0++;
		if (Main.rand.NextBool())
		{
			Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.RedTorch);
			d.velocity = NPC.velocity * -0.5f;
		}
		if (Sub0 > TelegraphDuration)
		{
			TargetX = NPC.Center.X;
			TargetY = NPC.Center.Y;
			Cmd = CmdIdleAir;
			TelegraphDuration = 0f;
			_lastCmd = (int)Cmd;
		}
	}

	// === P2/P3 ROLL: Teleport to side of player, then fast roll ===
	private void DoP2P3Roll(Player player)
	{
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		
		if (Sub0 == 0f) // Init teleport
		{
			PoofVisuals(); // Show that he is deliberately leaving
			NPC.velocity = Vector2.Zero;
			NPC.noGravity = true;
			NPC.alpha = 255;
			
			float dir = Main.rand.NextBool() ? -1f : 1f;
			Vector2 tryPos = player.Center + new Vector2(400f * dir, -160f); // Closer, on-screen
			while (Collision.SolidCollision(tryPos, NPC.width, NPC.height) && tryPos.Y > 16f)
				tryPos.Y -= 16f;
				
			Sub2 = tryPos.X;
			Sub3 = tryPos.Y;
			Sub1 = Math.Sign(player.Center.X - Sub2);
			if (Sub1 == 0f) Sub1 = 1f;
			
			Sub0 = 1f;
		}
		
		if (Sub0 >= 1f && Sub0 < 45f) // Teleport telegraph
		{
			Sub0++;
			NPC.velocity = Vector2.Zero;
			NPC.noGravity = true;
			NPC.alpha = 255;
			Vector2 targetPos = new Vector2(Sub2, Sub3);
			
			// Telegraph effects - denser so it's obvious
			for (int i = 0; i < 2; i++)
			{
				Dust d = Dust.NewDustDirect(targetPos, NPC.width, NPC.height, DustID.RedTorch);
				d.velocity = Main.rand.NextVector2Circular(5f, 5f);
				d.noGravity = true;
			}
			
			if (Sub0 == 45f)
			{
				NPC.position = targetPos;
				NPC.alpha = 0;
				ResetTrail();
				PoofVisuals();
				SoundEngine.PlaySound(SoundID.Item28 with { PitchVariance = 0.2f }, NPC.Center);
			}
			return;
		}
		
		// Rolling
		Sub0++;
		float baseSpeed = 8.0f; // More conservative, deliberate speed
		NPC.velocity.X = baseSpeed * Sub1;
		NPC.rotation += NPC.velocity.X * 0.05f;
		
		if (NPC.velocity.Y == 0 && Sub0 % 15 == 0)
		{
			SoundEngine.PlaySound(SoundID.Tink with { PitchVariance = 0.2f }, NPC.Center);
		}
		
		Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
		
		// Step through platforms
		if (NPC.velocity.Y == 0 && player.Center.Y > NPC.Bottom.Y + 16f)
		{
			int tileX = (int)(NPC.Center.X / 16f);
			int tileY = (int)((NPC.Bottom.Y + 2f) / 16f);
			if (WorldGen.InWorld(tileX, tileY))
			{
				Tile tile = Main.tile[tileX, tileY];
				if (tile.HasTile && Main.tileSolidTop[tile.TileType])
					NPC.position.Y += 2f;
			}
		}

		if (Math.Abs(NPC.position.X - NPC.oldPosition.X) < 0.5f && NPC.velocity.Y == 0)
		{
			NPC.velocity.Y = -7f; 
			if (Sub0 % 20 == 0) Sub1 *= -1f; 
		}
		
		if (Sub0 > 225f) // Failsafe despawn
		{
			Cmd_SetDespawn();
		}
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Color redTint = new(255, 60, 40);

		// === AURA GLOW underlay ===
		if (NPC.alpha < 255)
		{
			Texture2D glow = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
			Vector2 glowOrigin = glow.Size() / 2f;
			float pulse = 1f + 0.12f * (float)Math.Sin(Main.GameUpdateCount * 0.10f);
			float alphaMul = 1f - NPC.alpha / 255f;
			Color glowColor = redTint * (0.55f * alphaMul);
			glowColor.A = 0;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			spriteBatch.Draw(glow, NPC.Center - screenPos, null, glowColor, NPC.rotation, glowOrigin, NPC.scale * 0.85f * pulse, SpriteEffects.None, 0f);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
		}

		// (Single trail block lives at the bottom of PreDraw; no separate speed-gated trail here
		// — having both running at once double-renders afterimages on dashes.)

		// Draw a red laser telegraph during TelegraphShoot / TelegraphDash setup phases
		bool isTelegraphing =
			Cmd == CmdTelegraphShoot ||
			(Cmd == CmdTelegraphDash && Sub1 == 0f);

		if (isTelegraphing)
		{
			float dur = EffectiveTelegraphDuration;
			float aimProgress = MathHelper.Clamp(Sub0 / dur, 0f, 1f);
			Color baseColor = new Color(255, 60, 40) * aimProgress;

			Vector2 targetPos = new(TargetX, TargetY);
			Vector2 startPos = NPC.Center - screenPos;
			Vector2 endPos = targetPos - screenPos;

			Texture2D magicPixel = TextureAssets.MagicPixel.Value;
			Texture2D glowTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
			Vector2 glowOrigin = new Vector2(32f, 32f);

			float angle = (endPos - startPos).ToRotation();
			float baseThickness = Cmd == CmdTelegraphDash ? 3f : 2f;
			float beamLength = 3000f;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			float auraThickness = baseThickness * 4f;
			Color auraColor = baseColor * 0.8f;
			spriteBatch.Draw(magicPixel, startPos, new Rectangle(0, 0, 1, 1), auraColor, angle, new Vector2(0, 0.5f), new Vector2(beamLength, auraThickness), SpriteEffects.None, 0f);
			spriteBatch.Draw(glowTex, startPos, null, auraColor, 0f, glowOrigin, auraThickness / 20f, SpriteEffects.None, 0f);

			float coreThickness = baseThickness * 1.5f;
			Color coreColor = Color.White * aimProgress;
			spriteBatch.Draw(magicPixel, startPos, new Rectangle(0, 0, 1, 1), coreColor, angle, new Vector2(0, 0.5f), new Vector2(beamLength, coreThickness), SpriteEffects.None, 0f);
			spriteBatch.Draw(glowTex, startPos, null, coreColor, 0f, glowOrigin, coreThickness / 20f, SpriteEffects.None, 0f);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
		}

		Texture2D texture = TextureAssets.Npc[NPC.type].Value;
		Vector2 origin = NPC.frame.Size() / 2f;
		
		float pulseRate = 0.15f;
		float scalePulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * pulseRate * 60f) * 0.08f;
		float drawScale = NPC.scale * scalePulse;

		// Trails (only draw segments that are far enough apart to prevent the "weird pulse" overlapping blob).
		// Skip uninitialized slots and bail on huge gaps so we don't streak across the gap after a teleport.
		Vector2 lastDrawnPos = NPC.Center;
		for (int i = 1; i < NPC.oldPos.Length; i++)
		{
			if (NPC.oldPos[i] == Vector2.Zero) continue;
			Vector2 oldCenter = NPC.oldPos[i] + NPC.Size / 2f;
			if (Vector2.Distance(NPC.Center, oldCenter) > 300f) break;
			
			if (Vector2.Distance(lastDrawnPos, oldCenter) < 2f) continue;

			Vector2 oldDrawPos = oldCenter - screenPos + new Vector2(0f, NPC.gfxOffY);
			Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length);
			spriteBatch.Draw(texture, oldDrawPos, NPC.frame, color, NPC.rotation, origin, drawScale, SpriteEffects.None, 0f);
			lastDrawnPos = oldCenter;
		}

		// Main body
		spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, drawScale, SpriteEffects.None, 0f);

		return false;
	}
}
