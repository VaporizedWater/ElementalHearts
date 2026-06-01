using System;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

/// <summary>
/// Rare Animate — "Blue", the master of the trio. Blue holds the entire shared life pool;
/// the Red and Green Enforcers (<see cref="RareAnimateEnforcer"/>) are invulnerable puppets it
/// spawns and commands. Blue's death poofs them.
/// <para/>
/// Phase flow is health-gated (Intro → P1 spatial puzzle → P2 bullet-hell → P3 execution test),
/// with the family's signature hide-and-heal retreat folded in between attack loops, and a
/// terminal sub-5% Finale where coordination collapses into a chain-dash scramble.
/// </summary>
[AutoloadBossHead]
public sealed class RareAnimate : AnimateBoss
{
	public override int ProgressionTier => 2;
	public override LifeShardTier Tier => LifeShardTier.Rare;
	public override SoundStyle? AmbientEmissionSound => AnimateBossSounds.RareEmission;

	public override string Texture => "ElementalHearts/Content/Items/BossSpawns/RareMenacingHeart";
	public override string BossHeadTexture => "ElementalHearts/Content/Items/BossSpawns/RareMenacingHeart";

	// ===== Tuning (Pre-Wall of Flesh) =====
	private const int LifePool = 5400;
	private const int ContactDamage = 56;
	private const int TrapDamage = 44;
	private const int CometDamage = 50;
	private const int ShardDamage = 31;
	private const int DeathrayDamage = 124;

	private const float P2Threshold = 0.65f;
	private const float P3Threshold = 0.35f;
	private const float FinaleThreshold = 0.05f;

	private const int IntroDuration = 600;     // staggered SmoothStep descent (exactly 10 seconds)
	private const float OrbitRadius = 330f;
	private const float OrbitAngularSpeed = 0.016f;

	// Phase 2 tuning
	private const float Phase2OrbitRadius = 650f;   // enforcers fire from much farther out
	private const float Phase2OrbitSpeed = 0.008f;  // slow + direct-on-arc → no abrupt side swaps
	private const int Phase2FireInterval = 62;      // ticks between alternating enforcer shots
	private const int Phase2CometCooldown = 119;     // ticks between Blue's comet bursts
	private const int Phase2CometBursts = 5;         // comet bursts before Blue retreats to heal

	// ===== State machine =====
	private enum State { Intro, Phase1, Hiding, Phase2, Phase3, Finale, Transitioning }

	private State CurrentState { get => (State)NPC.ai[0]; set => NPC.ai[0] = (float)value; }
	private ref float Timer => ref NPC.ai[1];
	private ref float Counter1 => ref NPC.ai[2];
	private ref float Counter2 => ref NPC.ai[3];
	private float SubTimer { get => NPC.localAI[0]; set => NPC.localAI[0] = value; }
	private float SubMode { get => NPC.localAI[1]; set => NPC.localAI[1] = value; }
	private float TgX { get => NPC.localAI[2]; set => NPC.localAI[2] = value; }
	private float TgY { get => NPC.localAI[3]; set => NPC.localAI[3] = value; }

	// ===== Entity bookkeeping (server-authoritative class fields) =====
	private int _redWho = -1;
	private int _greenWho = -1;
	private int _deathrayWho = -1;
	private int _currentPhaseTarget = -1;
	private int _lastHidingHpThreshold;
	private bool _inFinale;
	private bool _justWokeFromHide;
	private State _previousState;     // for the hide-and-heal phase bonus
	private int _seqFiring = -1;      // Phase 2 sequential firing: -1 = none, 0 = red, 1 = green
	private int _seqTimer;
	private Vector2 _seqBaseDir;
	private Vector2 _sweepCenter;
	private int _sweepDir = 1;
	public int teleportCooldown;

	// Intro descent anchors
	private Vector2 _introBlueStart, _introRedStart, _introGreenStart;

	// Box-In (Phase 3, Attack 1) chosen cardinal sequence
	private readonly Vector2[] _boxDirs = new Vector2[3];

	// Polish
	private const float BaseScale = 2.2f;
	private float _scalePulse = 1f;
	private float _hitFlash;
	private bool _enteredP2, _enteredP3;

	private void PulseScale(float amt) { if (amt > _scalePulse) _scalePulse = amt; }

	// ====================================================================
	// Defaults
	// ====================================================================
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		NPCID.Sets.TrailCacheLength[NPC.type] = 8;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		NPC.width = 26;
		NPC.height = 26;
		NPC.scale = BaseScale;
		NPC.lifeMax = LifePool;
		NPC.damage = ContactDamage;
		NPC.defense = 12;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.behindTiles = false;

		if (!Main.dedServ)
		{
			string musicPath = "Music/RareAnimateTheme";
			if (MusicLoader.MusicExists(Mod, musicPath))
			{
				Music = MusicLoader.GetMusicSlot(Mod, musicPath);
			}
		}
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		Rectangle customHitbox = NPC.Hitbox;
		customHitbox.Inflate(-customHitbox.Width / 4, -customHitbox.Height / 4);
		if (!customHitbox.Intersects(target.Hitbox)) return false;

		cooldownSlot = ImmunityCooldownID.Bosses;
		// Throughout the entire fight, Blue only deals contact damage during his active dash in the Finale.
		// In all other states (Intro, Phase 1, Phase 2, Phase 3, Hiding, Transitioning),
		// Blue is not doing any contact attacks, so disable contact collision.
		if (CurrentState == State.Finale)
		{
			return SubMode == 1f; // only deal contact damage during the active dash phase
		}
		return false;
	}

	// ====================================================================
	// Main AI
	// ====================================================================
	public override void AI()
	{
		// Lock onto one player for the whole phase (rotated only at phase boundaries in MP).
		if (_currentPhaseTarget < 0 || _currentPhaseTarget == 255 || !Main.player[_currentPhaseTarget].active || Main.player[_currentPhaseTarget].dead)
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				System.Collections.Generic.List<int> validTargets = new System.Collections.Generic.List<int>();
				for (int i = 0; i < Main.maxPlayers; i++)
				{
					if (Main.player[i].active && !Main.player[i].dead)
					{
						validTargets.Add(i);
					}
				}
				if (validTargets.Count > 0)
				{
					_currentPhaseTarget = validTargets[Main.rand.Next(validTargets.Count)];
				}
				else
				{
					NPC.TargetClosest();
					_currentPhaseTarget = NPC.target;
				}
			}
			else
			{
				NPC.TargetClosest();
				_currentPhaseTarget = NPC.target;
			}
		}
		NPC.target = _currentPhaseTarget;
		Player player = Main.player[NPC.target];

		if (player.dead)
		{
			NPC.velocity.Y -= 0.04f;
			NPC.EncourageDespawn(10);
			DespawnEnforcers();
			KillDeathray();
			return;
		}

		// Keep both enforcers locked to Blue's target so the trio never splits across players.
		if (TryGetRed(out var rSync)) rSync.NPC.target = NPC.target;
		if (TryGetGreen(out var gSync)) gSync.NPC.target = NPC.target;

		if (teleportCooldown > 0) teleportCooldown--;
		if (_lastHidingHpThreshold == 0) _lastHidingHpThreshold = NPC.lifeMax;

		Lighting.AddLight(NPC.Center, 0.25f, 0.45f, 0.95f);

		ManagePhases();

		// Anti-despawn safety warp
		if (CurrentState != State.Intro && CurrentState != State.Hiding && CurrentState != State.Transitioning)
		{
			if (Vector2.Distance(NPC.Center, player.Center) > 1700f)
			{
				NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * 40f;
				if (Main.rand.NextBool(5)) SpawnTeleportVisuals();
			}
		}

		switch (CurrentState)
		{
			case State.Intro: DoIntro(player); break;
			case State.Phase1: DoPhase1(player); break;
			case State.Hiding: DoHiding(player); break;
			case State.Phase2: DoPhase2(player); break;
			case State.Phase3: DoPhase3(player); break;
			case State.Finale: DoFinale(player); break;
			case State.Transitioning: DoTransitioning(player); break;
		}

		if (NPC.velocity.Length() > 32f)
			NPC.velocity = Vector2.Normalize(NPC.velocity) * 32f;

		if (_scalePulse > 1f) _scalePulse = MathHelper.Lerp(_scalePulse, 1f, 0.12f);
		if (_hitFlash > 0f) _hitFlash = Math.Max(0f, _hitFlash - 0.08f);
		NPC.scale = BaseScale * _scalePulse;

		for (int i = NPC.oldPos.Length - 1; i > 0; i--) NPC.oldPos[i] = NPC.oldPos[i - 1];
		NPC.oldPos[0] = NPC.position;
	}

	private void ManagePhases()
	{
		float pct = (float)NPC.life / NPC.lifeMax;

		// Finale is terminal and overrides everything once we drop below the threshold.
		if (!_inFinale && pct <= FinaleThreshold && CurrentState != State.Intro)
		{
			EnterFinale();
			return;
		}
		if (_inFinale) return;

		State desired = State.Phase1;
		if (pct <= P3Threshold) desired = State.Phase3;
		else if (pct <= P2Threshold) desired = State.Phase2;

		if (CurrentState != State.Hiding && CurrentState != State.Intro && CurrentState != State.Transitioning && CurrentState != desired)
		{
			RotateTargetMP();
			PlayPhaseTransitionStinger(desired);
			KillDeathray();
			DespawnEnforcers();
			CurrentState = State.Transitioning;
			ResetStepCounters();
		}
	}

	private void RotateTargetMP()
	{
		if (Main.netMode == NetmodeID.SinglePlayer) return;
		System.Collections.Generic.List<int> validTargets = new System.Collections.Generic.List<int>();
		for (int i = 0; i < Main.maxPlayers; i++)
		{
			if (Main.player[i].active && !Main.player[i].dead)
			{
				validTargets.Add(i);
			}
		}
		if (validTargets.Count > 0)
		{
			_currentPhaseTarget = validTargets[Main.rand.Next(validTargets.Count)];
			NPC.target = _currentPhaseTarget;
		}
	}

	private void ResetStepCounters()
	{
		Timer = 0; Counter1 = 0; Counter2 = 0;
		SubTimer = 0; SubMode = 0; TgX = 0; TgY = 0;
		_seqFiring = -1;
	}

	private void PlayPhaseTransitionStinger(State next)
	{
		if (next == State.Phase2 && !_enteredP2) { _enteredP2 = true; SoundEngine.PlaySound(AnimateBossSounds.Phase2Transition, NPC.Center); }
		else if (next == State.Phase3 && !_enteredP3) { _enteredP3 = true; SoundEngine.PlaySound(AnimateBossSounds.Phase3Transition, NPC.Center); }
	}

	// ====================================================================
	// Enforcer management
	// ====================================================================
	private bool TryGetEnforcer(int who, out RareAnimateEnforcer e)
	{
		e = null;
		if (who < 0 || who >= Main.maxNPCs) return false;
		NPC n = Main.npc[who];
		if (!n.active || n.ModNPC is not RareAnimateEnforcer re) return false;
		e = re;
		return true;
	}

	private bool TryGetRed(out RareAnimateEnforcer e) { bool ok = TryGetEnforcer(_redWho, out e); if (!ok) _redWho = -1; return ok; }
	private bool TryGetGreen(out RareAnimateEnforcer e) { bool ok = TryGetEnforcer(_greenWho, out e); if (!ok) _greenWho = -1; return ok; }

	private void EnsureEnforcers(Vector2 redPos, Vector2 greenPos)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) return;
		if (!TryGetRed(out _)) _redWho = SpawnEnforcer(redPos, RareAnimateEnforcer.VariantRed);
		if (!TryGetGreen(out _)) _greenWho = SpawnEnforcer(greenPos, RareAnimateEnforcer.VariantGreen);
	}

	private int SpawnEnforcer(Vector2 pos, int variant)
	{
		int who = NPC.NewNPC(NPC.GetSource_FromAI(), (int)pos.X, (int)pos.Y, ModContent.NPCType<RareAnimateEnforcer>());
		if (who >= 0 && who < Main.maxNPCs)
		{
			NPC n = Main.npc[who];
			n.target = NPC.target;
			n.ai[1] = pos.X;  // seed idle target so it doesn't warp to origin on first tick
			n.ai[2] = pos.Y;
			if (n.ModNPC is RareAnimateEnforcer e) e.SetVariant(variant);
			if (Main.netMode == NetmodeID.Server) NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, who);
		}
		return who;
	}

	private void DespawnEnforcers()
	{
		if (TryGetRed(out var r)) r.Cmd_Despawn();
		if (TryGetGreen(out var g)) g.Cmd_Despawn();
		_redWho = -1; _greenWho = -1;
	}

	// Smoothly orbit both enforcers on opposite sides of the player (skips an enforcer that's
	// mid-dash so it can complete its own action, then naturally rejoins the orbit afterward).
	private void OrbitEnforcers(Player player, float angle, float radius, out Vector2 redPos, out Vector2 greenPos)
	{
		greenPos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
		redPos = player.Center + new Vector2((float)Math.Cos(angle + Math.PI), (float)Math.Sin(angle + Math.PI)) * radius;
		if (TryGetGreen(out var g) && !g.ActionInProgress) g.Cmd_IdleAir(greenPos);
		if (TryGetRed(out var r) && !r.ActionInProgress) r.Cmd_IdleAir(redPos);
	}

	// ====================================================================
	// Visual helpers (blue palette)
	// ====================================================================
	private void SpawnTeleportVisuals()
	{
		SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
		for (int i = 0; i < 30; i++)
			Dust.NewDustPerfect(NPC.Center, DustID.IceTorch, Main.rand.NextVector2CircularEdge(5f, 5f), 0, default, 1.5f).noGravity = true;
	}

	private void EmitBlueBurst(int particles, float radius, float scale)
	{
		for (int i = 0; i < particles; i++)
			Dust.NewDustPerfect(NPC.Center, DustID.IceTorch, Main.rand.NextVector2CircularEdge(radius, radius), 0, default, scale).noGravity = true;
		for (int i = 0; i < Math.Max(1, particles / 3); i++)
			Dust.NewDustPerfect(NPC.Center, DustID.BlueTorch, Main.rand.NextVector2Circular(radius * 0.6f, radius * 0.6f), 0, default, scale * 0.8f).noGravity = true;
	}

	private void EmitSmallPuff(int count = 14)
	{
		for (int i = 0; i < count; i++)
			Dust.NewDustPerfect(NPC.Center, DustID.IceTorch, Main.rand.NextVector2Circular(2.5f, 2.5f), 0, default, 1.2f).noGravity = true;
	}

	private static Vector2 PredictPlayer(Player player, float ticks = 26f, float cap = 360f)
	{
		Vector2 lead = player.velocity * ticks;
		if (lead.LengthSquared() > cap * cap) lead = Vector2.Normalize(lead) * cap;
		return player.Center + lead;
	}

	// ====================================================================
	// INTRO — staggered SmoothStep descent, then a synchronized flourish
	// ====================================================================
	private void DoIntro(Player player)
	{
		NPC.dontTakeDamage = true;
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		if (Timer == 0)
		{
			_introBlueStart = player.Center + new Vector2(0f, -950f);
			_introRedStart = player.Center + new Vector2(-760f, -1050f);
			_introGreenStart = player.Center + new Vector2(760f, -1050f);
			NPC.Center = _introBlueStart;
			NPC.velocity = Vector2.Zero;
			NPC.alpha = 60;
			EnsureEnforcers(_introRedStart, _introGreenStart);
			SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
			SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
		}

		Timer++;
		float t = MathHelper.Clamp(Timer / IntroDuration, 0f, 1f);

		Vector2 blueEnd = player.Center + new Vector2(0f, -300f);
		NPC.Center = Vector2.SmoothStep(_introBlueStart, blueEnd, t);
		NPC.rotation += 0.04f + (t * 0.06f);

		// Enforcers descend on a stagger (Green slightly behind Blue, Red behind Green).
		float angle = Timer * OrbitAngularSpeed;
		Vector2 gEnd = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * OrbitRadius;
		Vector2 rEnd = player.Center + new Vector2((float)Math.Cos(angle + Math.PI), (float)Math.Sin(angle + Math.PI)) * OrbitRadius;
		if (TryGetGreen(out var g)) { g.Cmd_Slaved(); g.NPC.Center = Vector2.SmoothStep(_introGreenStart, gEnd, Stagger(t, 0.22f)); }
		if (TryGetRed(out var r)) { r.Cmd_Slaved(); r.NPC.Center = Vector2.SmoothStep(_introRedStart, rEnd, Stagger(t, 0.44f)); }

		// Dramatic buildup effects
		if (Main.rand.NextFloat() < t * 1.5f)
		{
			float dustRadius = MathHelper.Lerp(60f, 20f, t);
			Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(dustRadius, dustRadius), DustID.IceTorch, Main.rand.NextVector2Circular(2f, 2f) * t, 0, default, 1.3f + t).noGravity = true;
		}

		if (Timer % 30 == 0)
		{
			PulseScale(1.05f + t * 0.15f);
			if (t > 0.5f && Main.rand.NextBool(2)) 
				SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.5f + t, Volume = t * 0.6f }, NPC.Center);
		}

		// Full comet burst of 10 comets right before ending
		if (Timer == IntroDuration - 5)
		{
			SoundEngine.PlaySound(SoundID.Item20 with { Pitch = -0.4f }, NPC.Center);
			PulseScale(1.5f);
			EmitSmallPuff(30);
			AnimateFx.ShakeCamera(NPC.Center, 6f, 1500f, 15, "RareAnimateIntroComets");
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int i = -5; i <= 4; i++)
				{
					float offset = i + 0.5f;
					Vector2 vel = new(offset * 2.2f, -14f - Math.Abs(offset) * 0.4f);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<CometProjectile>(), CometDamage, 0f, Main.myPlayer);
				}
			}
		}

		if (Timer >= IntroDuration)
		{
			NPC.alpha = 0;
			NPC.dontTakeDamage = false;
			EmitBlueBurst(70, 9f, 2.0f);
			EmitBlueBurst(30, 16f, 2.6f);
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
			AnimateFx.ShakeCamera(NPC.Center, 9f, 1600f, 22, "RareAnimateSpawn");
			CurrentState = State.Phase1;
			ResetStepCounters();
		}
	}

	private static float Stagger(float t, float delay) => MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((t - delay) / (1f - delay), 0f, 1f));

	// ====================================================================
	// PHASE 1 — The Spatial Puzzle: predictive Heart Traps + a 2-second orbit/dash rhythm
	// ====================================================================
	// Counter1 = dash beats issued, Counter2 = traps placed
	private void DoPhase1(Player player)
	{
		NPC.alpha = 0;
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		if (_justWokeFromHide)
		{
			SpawnTeleportVisuals();
			NPC.Center = player.Center + new Vector2(0f, -360f);
			NPC.velocity = Vector2.Zero;
			SpawnTeleportVisuals();
			_justWokeFromHide = false;
		}

		EnsureEnforcers(player.Center + new Vector2(-OrbitRadius, 0f), player.Center + new Vector2(OrbitRadius, 0f));

		Timer++;

		// Blue establishes zones of control by circling closely to allow melee retaliation.
		float blueAngle = Timer * 0.01f;
		Vector2 blueHover = player.Center + new Vector2((float)Math.Cos(blueAngle), (float)Math.Sin(blueAngle) * 0.6f) * 300f - new Vector2(0f, 60f);
		NPC.velocity = Vector2.Lerp(NPC.velocity, (blueHover - NPC.Center) * 0.05f, 0.08f);
		NPC.rotation = (NPC.Center - player.Center).ToRotation() + MathHelper.PiOver2;

		// Enforcers orbit on opposite sides.
		float orbitAngle = Timer * OrbitAngularSpeed;
		OrbitEnforcers(player, orbitAngle, OrbitRadius, out _, out _);

		// 2-second rhythm: alternate Red and Green dashing inward (Red, +120t, Green, +120t, ...).
		if (Timer % 120f == 0f)
		{
			bool redTurn = ((int)Counter1 % 2) == 0;
			if (redTurn && TryGetRed(out var r) && !r.ActionInProgress) r.Cmd_TelegraphDash();
			else if (!redTurn && TryGetGreen(out var g) && !g.ActionInProgress) g.Cmd_TelegraphDash();
			Counter1++;
		}

		// Predictive trap placement every ~150 ticks, offset within the rhythm so it overlaps a dash.
		// Telegraphed for 1 second (60 ticks) before placement.
		float trapPhase = Timer % 150f;
		if (trapPhase == 15f)
		{
			Vector2 trapPos = ComputePredictiveTrapPos(player);
			TgX = trapPos.X;
			TgY = trapPos.Y;
			SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.4f, Volume = 0.6f }, trapPos);
		}
		if (trapPhase >= 15f && trapPhase < 75f)
		{
			Vector2 trapPos = new Vector2(TgX, TgY);
			// Telegraph particles pulling inward
			if (Main.rand.NextBool(2))
			{
				float progress = (trapPhase - 15f) / 60f;
				Vector2 offset = Main.rand.NextVector2CircularEdge(40f - progress * 20f, 40f - progress * 20f);
				Dust.NewDustPerfect(trapPos + offset, DustID.IceTorch, -offset * 0.05f, 0, default, 1.2f).noGravity = true;
			}
		}
		if (trapPhase == 75f)
		{
			Vector2 trapPos = new Vector2(TgX, TgY);
			// Visual polish burst
			for (int i = 0; i < 20; i++)
				Dust.NewDustPerfect(trapPos, DustID.IceTorch, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.6f).noGravity = true;
			for (int i = 0; i < 10; i++)
				Dust.NewDustPerfect(trapPos, DustID.BlueTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.3f).noGravity = true;
			
			// Auditory polish for placement
			SoundEngine.PlaySound(SoundID.Item112 with { Pitch = 0.3f, Volume = 0.8f }, trapPos);

			PlaceHeartTrap(trapPos);
			Counter2++;
		}

		// After six traps and a few dash beats, retreat to heal.
		if (Counter2 >= 6f && Timer % 120f == 0f)
			StartHiding(State.Phase1);
	}

	private Vector2 SnapToPlatform(Vector2 pos)
	{
		int tx = (int)(pos.X / 16f);
		int startY = (int)(pos.Y / 16f);
		int bestY = -1;
		int minDistance = 9999;
		int searchRadius = 25;

		for (int y = startY - searchRadius; y <= startY + searchRadius; y++)
		{
			if (WorldGen.InWorld(tx, y))
			{
				Tile tile = Main.tile[tx, y];
				if (tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
				{
					bool spaceAbove = true;
					if (WorldGen.InWorld(tx, y - 1))
					{
						Tile above = Main.tile[tx, y - 1];
						// If the tile directly above is a full solid block, it's not a viable floor surface
						if (above.HasTile && Main.tileSolid[above.TileType] && !Main.tileSolidTop[above.TileType])
						{
							spaceAbove = false;
						}
					}
					
					if (spaceAbove)
					{
						int dist = Math.Abs(y - startY);
						if (dist < minDistance)
						{
							minDistance = dist;
							bestY = y;
						}
					}
				}
			}
		}

		if (bestY != -1)
		{
			// Snap to rest on top of the tile.
			pos.Y = bestY * 16f - 16f;
		}

		return pos;
	}

	// Place the trap a set distance ahead of the player's current path so a flat run can't outpace
	// it — forcing a jump or a turn. Falls back to a ring around the player when they're near-still.
	private Vector2 ComputePredictiveTrapPos(Player player)
	{
		Vector2 pos;
		if (player.velocity.Length() > 2f)
			pos = player.Center + Vector2.Normalize(player.velocity) * MathHelper.Clamp(player.velocity.Length() * 24f, 180f, 520f);
		else
			pos = player.Center + Main.rand.NextVector2CircularEdge(220f, 160f);

		return SnapToPlatform(pos);
	}

	private void PlaceHeartTrap(Vector2 pos)
	{
		SoundEngine.PlaySound(SoundID.Item28 with { Pitch = -0.2f }, pos);
		if (Main.netMode != NetmodeID.MultiplayerClient)
			Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<HeartTrapProjectile>(), TrapDamage, 0f, Main.myPlayer, NPC.whoAmI);
	}

	// ====================================================================
	// PHASE 2 — The Bullet Hell: Blue artillery (vanish/snap) over relentless turret orbit
	// ====================================================================
	// SubMode 0 = visible firing, 1 = vanished. Counter1 = comet bursts fired.
	// Counter1 = comet bursts fired, Counter2 = enforcer shots fired (drives alternation).
	private void DoPhase2(Player player)
	{
		NPC.alpha = 0;
		NPC.dontTakeDamage = false;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		Timer++;

		// --- Enforcers: a slow, FAR, perfectly smooth orbit. They're positioned directly on the
		//     arc each tick (and spawned already on it) so they glide around without ever cutting
		//     across the middle or snapping to the opposite side. ---
		float angle = Timer * Phase2OrbitSpeed;
		Vector2 greenPos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Phase2OrbitRadius;
		Vector2 redPos = player.Center + new Vector2((float)Math.Cos(angle + Math.PI), (float)Math.Sin(angle + Math.PI)) * Phase2OrbitRadius;
		EnsureEnforcers(redPos, greenPos);

		// Handle sequential firing & recoil movement for enforcers
		Vector2 redRecoil = Vector2.Zero;
		Vector2 greenRecoil = Vector2.Zero;

		if (_seqFiring == 0 && TryGetRed(out var redMinion))
		{
			_seqTimer++;
			float recoilProgress = (_seqTimer % 6) / 6f;
			float recoilAmt = (float)Math.Sin(recoilProgress * Math.PI) * 12f;
			redRecoil = -_seqBaseDir * recoilAmt;

			int seqIndex = _seqTimer - 1;
			if (seqIndex % 6 == 0 && seqIndex <= 24)
			{
				int i = (seqIndex / 6) - 2;
				Vector2 projVel = _seqBaseDir.RotatedBy(i * 0.26f) * 5.985f; // 10% slower
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Projectile.NewProjectile(NPC.GetSource_FromAI(), redMinion.NPC.Center, projVel, ModContent.ProjectileType<AnimateShardProjectile>(), ShardDamage, 0f, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item9 with { Pitch = -0.2f, Volume = 0.8f }, redMinion.NPC.Center);
			}

			if (_seqTimer > 25)
			{
				_seqFiring = -1;
			}
		}
		else if (_seqFiring == 1 && TryGetGreen(out var greenMinion))
		{
			_seqTimer++;
			float recoilProgress = (_seqTimer % 6) / 6f;
			float recoilAmt = (float)Math.Sin(recoilProgress * Math.PI) * 12f;
			greenRecoil = -_seqBaseDir * recoilAmt;

			int seqIndex = _seqTimer - 1;
			if (seqIndex % 6 == 0 && seqIndex <= 24)
			{
				int i = (seqIndex / 6) - 2;
				Vector2 projVel = _seqBaseDir.RotatedBy(i * 0.26f) * 6.84f; // 10% slower
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Projectile.NewProjectile(NPC.GetSource_FromAI(), greenMinion.NPC.Center, projVel, ModContent.ProjectileType<UncommonShardProjectile>(), ShardDamage, 0f, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item9 with { Pitch = 0.2f, Volume = 0.8f }, greenMinion.NPC.Center);
			}

			if (_seqTimer > 25)
			{
				_seqFiring = -1;
			}
		}

		if (TryGetRed(out var red)) { red.Cmd_Slaved(); red.NPC.Center = redPos + redRecoil; red.NPC.velocity = Vector2.Zero; }
		if (TryGetGreen(out var green)) { green.Cmd_Slaved(); green.NPC.Center = greenPos + greenRecoil; green.NPC.velocity = Vector2.Zero; }

		// --- Alternating fire: Red, then Green, then Red... one after the other. A short pitched
		//     tell plays 15 ticks before each shot (Red low, Green high). ---
		int slot = (int)(Timer % Phase2FireInterval);
		bool redTurn = ((int)Counter2 % 2) == 0;
		if (slot == Phase2FireInterval - 15)
		{
			if (redTurn && TryGetRed(out var rt)) SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.45f, PitchVariance = 0.1f }, rt.NPC.Center);
			else if (!redTurn && TryGetGreen(out var gt)) SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.45f, PitchVariance = 0.1f }, gt.NPC.Center);
		}
		if (slot == 0 && Timer > 1f)
		{
			_seqFiring = redTurn ? 0 : 1;
			_seqTimer = 0;
			if (redTurn && TryGetRed(out var rf)) _seqBaseDir = Vector2.Normalize(player.Center - rf.NPC.Center);
			else if (!redTurn && TryGetGreen(out var gf)) _seqBaseDir = Vector2.Normalize(player.Center - gf.NPC.Center);
			Counter2++;
		}

		// --- Blue: stays visible the whole phase, hovering above the player and lobbing a comet
		//     burst whenever the cooldown elapses. No more vanishing. ---
		float verticalOffset = 250f + (float)Math.Sin(Timer * 0.04f) * 50f;
		Vector2 hover = player.Center + new Vector2(0f, -verticalOffset);
		NPC.velocity = Vector2.Lerp(NPC.velocity, (hover - NPC.Center) * 0.12f, 0.2f);
		NPC.rotation += 0.05f;

		SubTimer++;
		if (SubTimer >= Phase2CometCooldown)
		{
			FireCometBurst(player);
			SubTimer = 0f;
			Counter1++;
			if (Counter1 >= Phase2CometBursts)
			{
				Counter1 = 0f;
				StartHiding(State.Phase2);
			}
		}
	}

	private void FireCometBurst(Player player)
	{
		SoundEngine.PlaySound(SoundID.Item20 with { Pitch = -0.2f }, NPC.Center);
		PulseScale(1.2f);
		EmitSmallPuff(16);
		AnimateFx.ShakeCamera(NPC.Center, 3f, 1000f, 8, "RareAnimateComets");
		if (Main.netMode == NetmodeID.MultiplayerClient) return;
		for (int i = -4; i <= 4; i++)
		{
			Vector2 vel = new(i * 3.0f, -13f - Math.Abs(i) * 0.5f);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<CometProjectile>(), CometDamage, 0f, Main.myPlayer);
		}
	}

	private void FireFan(Vector2 from, Vector2 at, int projType, float speed)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) return;
		
		// Add natural randomness so the player can't just stand perfectly still
		float randomOffset = Main.rand.NextFloat(-0.1f, 0.1f);
		Vector2 baseDir = Vector2.Normalize(at - from).RotatedBy(randomOffset);
		
		float fastSpeed = speed * 0.87f; // 40% slower than previous 1.45f multiplier
		float spread = 0.15f;
		Projectile.NewProjectile(NPC.GetSource_FromAI(), from, baseDir.RotatedBy(-spread) * fastSpeed, projType, ShardDamage, 0f, Main.myPlayer);
		Projectile.NewProjectile(NPC.GetSource_FromAI(), from, baseDir.RotatedBy(spread) * fastSpeed, projType, ShardDamage, 0f, Main.myPlayer);
	}

	// ====================================================================
	// PHASE 3 — The Execution Test: Box-In (Attack 1) then Jump Rope deathray (Attack 2)
	// ====================================================================
	// Counter1 = attack id (0 = box-in, 1 = jump-rope). SubMode = sub-step. Counter2 = box traps placed.
	private void DoPhase3(Player player)
	{
		NPC.alpha = 0;
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		const float P3TelegraphDuration = 90f;
		const float P3FireDuration = 300f; // 5 seconds sweep
		const float P3TrapDuration = 270f; // 3 traps * 90 ticks

		Timer++;

		if (SubMode == 0f) // Grouping & Telegraph (90 ticks)
		{
			EnsureEnforcers(player.Center + new Vector2(-OrbitRadius, 0f), player.Center + new Vector2(OrbitRadius, 0f));
			MergeEnforcers(NPC.Center);

			// Move directly on top of the player
			Vector2 groupPos = player.Center + new Vector2(0f, -220f);
			NPC.velocity = Vector2.Lerp(NPC.velocity, (groupPos - NPC.Center) * 0.15f, 0.2f);
			TgX = -MathHelper.PiOver2; // Laser starting angle (pointing straight up)
			NPC.rotation = TgX + MathHelper.PiOver2; // Face in the direction of the laser

			SubTimer++;
			if (SubTimer == 1f)
			{
				_sweepDir = Main.rand.NextBool() ? 1 : -1; // Randomize sweep direction!
				SpawnDeathray();
				SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f }, NPC.Center);
			}

			// Drive telegraph beam straight up
			DriveDeathray(NPC.Center, TgX.ToRotationVector2(), RareDeathrayProjectile.DefaultLength, firing: false);

			if (SubTimer % 4f == 0f)
				EmitSmallPuff(8);

			if (SubTimer >= P3TelegraphDuration)
			{
				SubMode = 1f;
				SubTimer = 0f;
				SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 0.9f }, NPC.Center); // deep beam onset
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				AnimateFx.ShakeCamera(NPC.Center, 6f, 1600f, 16, "RareAnimateBeamStart");
			}
		}
		else if (SubMode == 1f) // 360° Deathray Sweep (300 ticks)
		{
			MergeEnforcers(NPC.Center);

			if (SubTimer == 0f)
			{
				_sweepCenter = NPC.Center;
			}
			NPC.Center = _sweepCenter;
			NPC.velocity = Vector2.Zero;

			SubTimer++;

			// Calculate 360 rotation angle
			float progress = SubTimer / P3FireDuration;
			float angle = TgX + _sweepDir * MathHelper.TwoPi * progress;
			Vector2 dir = angle.ToRotationVector2();

			// Drive active laser
			DriveDeathray(NPC.Center, dir, RareDeathrayProjectile.DefaultLength, firing: true);
			NPC.rotation = angle + MathHelper.PiOver2;

			// Screen shake
			if (SubTimer % 6f == 0f)
				AnimateFx.ShakeCamera(player.Center, 4.5f, 1800f, 8, "RareAnimateBeamSweep");

			if (SubTimer >= P3FireDuration)
			{
				KillDeathray();
				SubMode = 2f;
				SubTimer = 0f;
			}
		}
		else if (SubMode == 2f) // Trap-Laying & Enforcer Shooters (270 ticks)
		{
			// Blue stays directly above the player, hovering buoyantly
			Vector2 hoverPos = player.Center + new Vector2(0f, -250f + (float)Math.Sin(Timer * 0.05f) * 20f);
			NPC.velocity = Vector2.Lerp(NPC.velocity, (hoverPos - NPC.Center) * 0.1f, 0.15f);
			NPC.rotation = (player.Center - NPC.Center).ToRotation() + MathHelper.PiOver2; // Look at player

			// Red and Green leave Blue to orbit and shoot
			float orbitAngle = Timer * OrbitAngularSpeed;
			Vector2 greenPos = player.Center + new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * OrbitRadius;
			Vector2 redPos = player.Center + new Vector2((float)Math.Cos(orbitAngle + Math.PI), (float)Math.Sin(orbitAngle + Math.PI)) * OrbitRadius;
			EnsureEnforcers(redPos, greenPos);

			if (TryGetRed(out var r))
			{
				r.Cmd_Slaved();
				r.NPC.Center = Vector2.Lerp(r.NPC.Center, redPos, 0.15f);
				r.NPC.velocity = Vector2.Zero;
			}
			if (TryGetGreen(out var g))
			{
				g.Cmd_Slaved();
				g.NPC.Center = Vector2.Lerp(g.NPC.Center, greenPos, 0.15f);
				g.NPC.velocity = Vector2.Zero;
			}

			SubTimer++;

			// Enforcer projectile fire (alternate every 60 ticks)
			int fireTimer = (int)(SubTimer % 60f);
			if (fireTimer == 0)
			{
				bool redTurn = ((int)(SubTimer / 60f) % 2) == 0;
				if (redTurn && TryGetRed(out var rf))
					FireFan(rf.NPC.Center, player.Center, ModContent.ProjectileType<AnimateShardProjectile>(), 6.65f);
				else if (!redTurn && TryGetGreen(out var gf))
					FireFan(gf.NPC.Center, player.Center, ModContent.ProjectileType<UncommonShardProjectile>(), 7.6f);
			}

			// Blue lays a few traps: 3 traps total, 90 ticks cycle
			int trapPhase = (int)(SubTimer % 90f);
			if (trapPhase == 15)
			{
				Vector2 trapPos = ComputePredictiveTrapPos(player);
				TgX = trapPos.X;
				TgY = trapPos.Y;
				SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.4f, Volume = 0.6f }, trapPos);
			}
			else if (trapPhase > 15 && trapPhase < 75)
			{
				Vector2 trapPos = new Vector2(TgX, TgY);
				if (Main.rand.NextBool(2))
				{
					float progress = (trapPhase - 15) / 60f;
					Vector2 offset = Main.rand.NextVector2CircularEdge(40f - progress * 20f, 40f - progress * 20f);
					Dust.NewDustPerfect(trapPos + offset, DustID.IceTorch, -offset * 0.05f, 0, default, 1.2f).noGravity = true;
				}
				// Falling telegraph sparks from Blue to the trap position
				if (Main.rand.NextBool(3))
				{
					float progress = Main.rand.NextFloat();
					Vector2 particlePos = Vector2.Lerp(NPC.Center, trapPos, progress);
					Dust.NewDustPerfect(particlePos, DustID.PinkCrystalShard, Vector2.UnitY * 3f, 0, default, 1.0f).noGravity = true;
				}
			}
			else if (trapPhase == 75)
			{
				PlaceHeartTrap(new Vector2(TgX, TgY));
			}

			if (SubTimer >= P3TrapDuration)
			{
				SubMode = 0f;
				SubTimer = 0f;
				Timer = 0f;
				StartHiding(State.Phase3);
			}
		}
	}

	private void MergeEnforcers(Vector2 center)
	{
		if (TryGetRed(out var r)) 
		{ 
			r.Cmd_Slaved(); 
			Vector2 offset = new Vector2(-22f, 14f);
			r.NPC.Center = center + offset; 
			r.NPC.velocity = Vector2.Zero; 
			r.NPC.rotation = (-offset).ToRotation() + MathHelper.PiOver2;
		}
		if (TryGetGreen(out var g)) 
		{ 
			g.Cmd_Slaved(); 
			Vector2 offset = new Vector2(22f, 14f);
			g.NPC.Center = center + offset; 
			g.NPC.velocity = Vector2.Zero; 
			g.NPC.rotation = (-offset).ToRotation() + MathHelper.PiOver2;
		}
	}

	private void SpawnDeathray()
	{
		if (_deathrayWho >= 0 && _deathrayWho < Main.maxProjectiles && Main.projectile[_deathrayWho].active && Main.projectile[_deathrayWho].type == ModContent.ProjectileType<RareDeathrayProjectile>())
			return;
		if (Main.netMode == NetmodeID.MultiplayerClient) return;
		int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<RareDeathrayProjectile>(), DeathrayDamage, 0f, Main.myPlayer, NPC.whoAmI);
		_deathrayWho = p;
	}

	private void DriveDeathray(Vector2 center, Vector2 dir, float length, bool firing)
	{
		if (_deathrayWho < 0 || _deathrayWho >= Main.maxProjectiles) return;
		Projectile p = Main.projectile[_deathrayWho];
		if (!p.active || p.type != ModContent.ProjectileType<RareDeathrayProjectile>()) { _deathrayWho = -1; return; }
		p.Center = center;
		p.velocity = dir;
		p.ai[1] = firing ? Math.Max(length, 1f) : -Math.Max(length, 1f);
		p.timeLeft = 3;
		if (Main.netMode != NetmodeID.SinglePlayer) p.netUpdate = true;
	}

	private void KillDeathray()
	{
		if (_deathrayWho >= 0 && _deathrayWho < Main.maxProjectiles)
		{
			Projectile p = Main.projectile[_deathrayWho];
			if (p.active && p.type == ModContent.ProjectileType<RareDeathrayProjectile>()) p.Kill();
		}
		_deathrayWho = -1;
	}

	// ====================================================================
	// FINALE — the Adrenaline Check: coordination collapses, everyone chain-dashes
	// ====================================================================
	private void EnterFinale()
	{
		_inFinale = true;
		CurrentState = State.Finale;
		ResetStepCounters();
		KillDeathray();
		NPC.dontTakeDamage = false;
		EnsureEnforcers(NPC.Center + new Vector2(-200f, -120f), NPC.Center + new Vector2(200f, -120f));
		if (TryGetRed(out var r)) r.Cmd_Frenzy();
		if (TryGetGreen(out var g)) g.Cmd_Frenzy();
		SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f }, NPC.Center);
		SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
		EmitBlueBurst(80, 10f, 2.4f);
		AnimateFx.ShakeCamera(NPC.Center, 10f, 1800f, 24, "RareAnimateFinale");
	}

	// Blue runs his own relentless chain-dash, independent of the (now frenzied) enforcers.
	// SubMode 0 = short telegraph, 1 = dash, 2 = micro-recover.
	private void DoFinale(Player player)
	{
		NPC.alpha = 0;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.dontTakeDamage = false;
		NPC.damage = 81; // Maximum damage during Finale

		EnsureEnforcers(NPC.Center + new Vector2(-200f, -120f), NPC.Center + new Vector2(200f, -120f));
		if (TryGetRed(out var r)) r.Cmd_Frenzy();
		if (TryGetGreen(out var g)) g.Cmd_Frenzy();

		const float telegraph = 35f;
		const float dash = 30f;
		const float recover = 30f;
		float speed = 26f;

		if (SubMode == 0f)
		{
			NPC.velocity = Vector2.Zero;
			SubTimer++;
			if (SubTimer == 1f) SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.3f, PitchVariance = 0.2f }, NPC.Center);
			TgX = player.Center.X; TgY = player.Center.Y;
			if (Main.rand.NextBool(2))
				Dust.NewDustPerfect(NPC.Center, DustID.IceTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.2f).noGravity = true;
			if (SubTimer >= telegraph)
			{
				Vector2 dir = Vector2.Normalize(new Vector2(TgX, TgY) - NPC.Center);
				if (dir == Vector2.Zero) dir = Vector2.UnitX;
				NPC.velocity = dir * speed;
				TgX = dir.X; TgY = dir.Y;
				SubMode = 1f; SubTimer = 0f;
				PulseScale(1.35f);
				SpawnTeleportVisuals();
				SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f, PitchVariance = 0.2f }, NPC.Center);
			}
		}
		else if (SubMode == 1f)
		{
			SubTimer++;
			float ratio = MathHelper.Clamp(1f - SubTimer / dash, 0.2f, 1f);
			NPC.velocity = new Vector2(TgX, TgY) * speed * ratio;
			NPC.rotation += 0.4f;
			if (Main.rand.NextBool())
				Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.IceTorch).velocity = NPC.velocity * -0.4f;
			if (SubTimer > dash) { SubMode = 2f; SubTimer = 0f; AnimateFx.ShakeCamera(NPC.Center, 2.5f, 800f, 5, "RareAnimateFinaleDash"); }
		}
		else
		{
			SubTimer++;
			NPC.velocity = Vector2.Zero;
			if (SubTimer > recover) { SubMode = 0f; SubTimer = 0f; }
		}
	}

	// ====================================================================
	// TRANSITIONING — theatrical, invulnerable beat between phases
	// ====================================================================
	private void DoTransitioning(Player player)
	{
		NPC.dontTakeDamage = true;
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		Vector2 hover = player.Center + new Vector2(0f, -320f);
		NPC.velocity = (hover - NPC.Center) * 0.05f;
		if (NPC.velocity.Length() > 8f) NPC.velocity = Vector2.Normalize(NPC.velocity) * 8f;
		NPC.rotation = (NPC.Center - player.Center).ToRotation() + MathHelper.PiOver2;

		Timer++;
		if (Timer % 10f == 0f) EmitBlueBurst(15, 6f, 1.2f);

		if (Timer >= 120f)
		{
			NPC.dontTakeDamage = false;
			EmitBlueBurst(45, 7f, 1.6f);
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
			AnimateFx.ShakeCamera(NPC.Center, 5f, 1200f, 16, "RareAnimateTransition");

			float pct = (float)NPC.life / NPC.lifeMax;
			State next = State.Phase1;
			if (pct <= P3Threshold) next = State.Phase3;
			else if (pct <= P2Threshold) next = State.Phase2;
			CurrentState = next;
			ResetStepCounters();
		}
	}

	// ====================================================================
	// HIDING — the family's hide-and-heal retreat (enforcers despawn, return on wake)
	// ====================================================================
	private void StartHiding(State returnState)
	{
		if (CurrentState == State.Finale) return; // Fight to the death

		if (NPC.life > _lastHidingHpThreshold)
		{
			// Anti-stall: skip the retreat if Blue hasn't lost 5% since his last hide.
			ResetStepCounters();
			return;
		}
		_lastHidingHpThreshold = NPC.life - (int)(NPC.lifeMax * 0.05f);

		KillDeathray();
		DespawnEnforcers();
		CurrentState = State.Hiding;
		Timer = -30; // 0.5s windup
		_previousState = returnState;
		Counter1 = 0; Counter2 = 0;
		SubTimer = 0; SubMode = 0;
	}

	private void DoHiding(Player player)
	{
		if (Timer < 0) // windup + fade out
		{
			NPC.velocity *= 0.8f;
			NPC.alpha = (int)MathHelper.Clamp(255f * ((30f + Timer) / 30f), 0f, 255f);
			if (Timer == -30) SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
			if (Main.rand.NextBool(2)) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IceTorch);
			Timer++;
			if (Timer == 0)
			{
				ExecuteHideTeleport(player);
				NPC.alpha = 150;
			}
			return;
		}

		if (Timer < 1000)
		{
			NPC.velocity *= 0.9f;
			NPC.dontTakeDamage = false;
			NPC.alpha = 130 + (int)(40f * Math.Sin(Timer * 0.05f));

			if (Main.rand.NextBool(4))
			{
				Vector2 spawn = NPC.Center + Main.rand.NextVector2Circular(28f, 28f);
				Dust d = Dust.NewDustPerfect(spawn, DustID.IceTorch, new Vector2(0f, -Main.rand.NextFloat(0.5f, 1.5f)), 0, default, 1.1f);
				d.noGravity = true;
			}

			// Balanced heal: a FULL uninterrupted ~10s hide restores ~10% of max life.
			// Hitting Blue cuts the heal short entirely, forcing the player to hunt him down.
			float phaseBonus = _previousState switch { State.Phase2 => 1.10f, State.Phase3 => 1.20f, _ => 1.0f };
			float perTick = NPC.lifeMax * 0.10f / 600f; // spread ~10% across the 600-tick auto-wake window

			SubTimer += perTick * phaseBonus;
			if (SubTimer >= 1f)
			{
				int heal = (int)SubTimer;
				SubTimer -= heal;
				if (NPC.life < NPC.lifeMax)
				{
					NPC.life = Math.Min(NPC.lifeMax, NPC.life + heal);
					NPC.HealEffect(heal, true);
				}
			}
			Timer++;
		}

		// Wake after 10 idle seconds, or the instant Blue is struck / touched.
		if (Timer >= 600 && Timer < 1000) { WakeFromHiding(); return; }
		if (Timer >= 0 && Timer < 1000 && (NPC.Hitbox.Intersects(player.Hitbox) || NPC.justHit)) { WakeFromHiding(); return; }
	}

	private void ExecuteHideTeleport(Player player)
	{
		SpawnTeleportVisuals();
		float dir = Main.rand.NextBool() ? -1f : 1f;
		float dist = Main.rand.NextFloat(700f, 900f);
		Vector2 target = player.Center + new Vector2(dist * dir, -300f);

		// Drop onto the first solid surface below, if any (purely cosmetic — Blue ignores tiles).
		for (int i = 0; i < 70; i++)
		{
			int tx = (int)(target.X / 16f);
			int ty = (int)(target.Y / 16f);
			if (WorldGen.InWorld(tx, ty) && Main.tile[tx, ty].HasTile && Main.tileSolid[Main.tile[tx, ty].TileType] && !Main.tileSolidTop[Main.tile[tx, ty].TileType])
			{
				target.Y = ty * 16f - NPC.height / 2f - 2f;
				break;
			}
			target.Y += 16f;
		}

		NPC.Center = target;
		NPC.velocity = Vector2.Zero;
		SpawnTeleportVisuals();
	}

	private void WakeFromHiding()
	{
		NPC.alpha = 0;
		NPC.dontTakeDamage = false;
		SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
		SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
		SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
		EmitBlueBurst(50, 8f, 2.0f);
		AnimateFx.ShakeCamera(NPC.Center, 7f, 1500f, 18, "RareAnimateWake");

		float pct = (float)NPC.life / NPC.lifeMax;
		State next = State.Phase1;
		if (pct <= FinaleThreshold) { EnterFinale(); return; }
		if (pct <= P3Threshold) next = State.Phase3;
		else if (pct <= P2Threshold) next = State.Phase2;
		PlayPhaseTransitionStinger(next);

		CurrentState = next;
		ResetStepCounters();
		_justWokeFromHide = true;
	}

	// ====================================================================
	// Death / hit feedback + drawing
	// ====================================================================
	public override void OnKill()
	{
		base.OnKill();
		DespawnEnforcers();
		KillDeathray();
	}

	public override void HitEffect(NPC.HitInfo hit)
	{
		if (Main.netMode == NetmodeID.Server) return;

		if (NPC.life <= 0)
		{
			SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);
			SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
			SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
			EmitBlueBurst(130, 11f, 2.6f);
			EmitBlueBurst(60, 19f, 3.0f);
			for (int i = 0; i < 24; i++)
				Dust.NewDustPerfect(NPC.Center, DustID.BlueTorch, Main.rand.NextVector2CircularEdge(6f, 6f), 0, default, 2.0f).noGravity = true;
			AnimateFx.ShakeCamera(NPC.Center, 14f, 2000f, 30, "RareAnimateDeath");
			return;
		}

		_hitFlash = 1f;
		PulseScale(1.08f);
		int count = 4 + (int)Math.Min(20, hit.Damage / 8);
		for (int i = 0; i < count; i++)
			Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.IceTorch, Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.2f).noGravity = true;
		if (hit.Damage >= 30)
			AnimateFx.ShakeCamera(NPC.Center, 2.5f, 900f, 6, "RareAnimateHit");
	}

	public override Color? GetAlpha(Color drawColor)
	{
		if (_hitFlash > 0f)
		{
			Color c = Color.Lerp(drawColor, Color.White, _hitFlash * 0.7f);
			c.A = (byte)(255 - NPC.alpha);
			return c;
		}
		return null;
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		Color blueTint = new(70, 140, 255);

		// Soft pulsing aura so Blue pierces the dark (glowmask-style underlay).
		if (NPC.alpha < 255)
		{
			Texture2D glow = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
			Vector2 glowOrigin = glow.Size() / 2f;
			float pulse = 1f + 0.12f * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
			float alphaMul = 1f - NPC.alpha / 255f;
			Color glowColor = blueTint * (0.55f * alphaMul);
			glowColor.A = 0;
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			spriteBatch.Draw(glow, NPC.Center - screenPos, null, glowColor, NPC.rotation, glowOrigin, NPC.scale * 0.9f * pulse, SpriteEffects.None, 0f);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
		}

		Texture2D texture = TextureAssets.Npc[NPC.type].Value;
		Vector2 origin = NPC.frame.Size() / 2f;
		float hpPct = (float)NPC.life / NPC.lifeMax;
		float pulseRate = MathHelper.Lerp(0.05f, 0.25f, 1f - hpPct);
		float drawScale = NPC.scale * (1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * pulseRate * 60f) * 0.08f);

		// Speed-gated afterimage trail (matches the family; only shows at real dash speeds).
		Vector2 lastDrawn = NPC.Center;
		for (int i = 1; i < NPC.oldPos.Length; i++)
		{
			if (NPC.oldPos[i] == Vector2.Zero) continue;
			Vector2 oldCenter = NPC.oldPos[i] + NPC.Size / 2f;
			if (Vector2.Distance(NPC.Center, oldCenter) > 300f) break;
			if (Vector2.Distance(lastDrawn, oldCenter) < 2f) continue;
			Vector2 oldDraw = oldCenter - screenPos + new Vector2(0f, NPC.gfxOffY);
			Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length);
			spriteBatch.Draw(texture, oldDraw, NPC.frame, color, NPC.rotation, origin, drawScale, SpriteEffects.None, 0f);
			lastDrawn = oldCenter;
		}

		spriteBatch.Draw(texture, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY), NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, drawScale, SpriteEffects.None, 0f);
		return false;
	}
}
