using ElementalHearts.Common.LifeShards;
using ElementalHearts.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

[AutoloadBossHead]
public sealed class UncommonAnimate : AnimateBoss
{
	public override int ProgressionTier => 1;
	public override LifeShardTier Tier => LifeShardTier.Uncommon;
	public override SoundStyle? AmbientEmissionSound => AnimateBossSounds.UncommonEmission;

	public override string Texture => "ElementalHearts/Content/NPCs/Bosses/Animate/UncommonAnimate";
	public override string BossHeadTexture => "ElementalHearts/Content/Items/BossSpawns/UncommonMenacingHeart";

	// State machine
	private enum State
	{
		Intro,
		Phase1_SkyShoot,
		Hiding,
		Phase2_CoopSpiral,
		Phase3_CoopDashes,
		Transitioning
	}

	private State CurrentState
	{
		get => (State)NPC.ai[0];
		set => NPC.ai[0] = (float)value;
	}

	private ref float Timer => ref NPC.ai[1];
	private ref float Counter1 => ref NPC.ai[2];
	private ref float Counter2 => ref NPC.ai[3];

	private float SubTimer { get => NPC.localAI[0]; set => NPC.localAI[0] = value; }
	private float SubMode { get => NPC.localAI[1]; set => NPC.localAI[1] = value; }
	private float TgX { get => NPC.localAI[2]; set => NPC.localAI[2] = value; }
	private float TgY { get => NPC.localAI[3]; set => NPC.localAI[3] = value; }

	// Class fields (don't sync in MP, but persist on the server-controlled NPC instance)
	private int _redMinionWho = -1;
	private int _lastHidingHpThreshold;
	private int _lastPhase3Move = -1;
	private bool _justWokeFromHide;
	// MP target lock — set once, kept for the whole phase, only rotated at phase transitions.
	// Prevents the boss (and Red, who mirrors it) from snapping between players mid-attack.
	private int _currentPhaseTarget = -1;
	// Cooldown between Phase 2 safety teleports — prevents back-to-back warps if the orbit
	// keeps getting knocked off after a dash or piercing weapon.
	public int teleportCooldown;
	// Phase 2 safety teleport state — 0.5s telegraph at destination, then warp.
	private float _p2TeleportActive;
	private float _p2TeleportTimer;
	private float _p2TeleportTargetX;
	private float _p2TeleportTargetY;
	// Phase 3 dash: captured at launch so SubMode 3 can decay velocity from 2x base → 0 over 120 ticks.
	private float _dashDirX;
	private float _dashDirY;
	private float _dashPeakSpeed;
	private const float Phase3DashDuration = 120f;          // 2-second dash for all Phase 3 moves
	// Boss-driven Red laser aim point (used by PreDraw when Red is in Cmd_SetSlaved and
	// can't draw his own telegraph). Boss writes these each tick during track/lock.
	private float _redLaserAimX;
	private float _redLaserAimY;
	private bool _redLaserShown;
	// Pincer (Move 1): snapshot positions at teleport, dash direction at lock.
	private float _pincerStartY;
	private float _pincerGreenX;
	private float _pincerRedX;
	private float _pincerGreenDashDirX;
	private float _pincerRedDashDirX;
	// Ground sweep: snapshot of locked Y values after the 90-tick tracking window
	private bool _groundSweepLocked;
	private float _groundSweepGreenLockedY;
	private float _groundSweepRedLockedY;
	// Phase 2 telegraph mode: 0 = no laser, 1 = active. Boss orchestrates both telegraphs
	// so that Red can keep orbiting without freezing.
	private float _greenTelegraphActive;
	private float _greenTelegraphTimer;
	private float _redTelegraphActive;
	private float _redTelegraphTimer;
	private const float TelegraphDurationP2 = 30f;
	
	private int[] _p3Bag = new int[] { 1, 2, 3 };
	private int _p3BagIndex = 3;

	// Polish state — purely visual, decays in AI()
	private const float BaseScale = 2.0f;
	private float _scalePulse = 1f;     // multiplied into NPC.scale; decays toward 1 each tick
	private float _hitFlash;            // 0..1 white flash on damage, decays each tick

	// One-shot guards so each phase-transition stinger plays exactly once per fight
	private bool _enteredP2;
	private bool _enteredP3;

	private void PlayPhaseTransitionStinger(State newState)
	{
		if (newState == State.Phase2_CoopSpiral && !_enteredP2)
		{
			_enteredP2 = true;
			SoundEngine.PlaySound(AnimateBossSounds.Phase2Transition, NPC.Center);
		}
		else if (newState == State.Phase3_CoopDashes && !_enteredP3)
		{
			_enteredP3 = true;
			SoundEngine.PlaySound(AnimateBossSounds.Phase3Transition, NPC.Center);
		}
	}

	public override void SetStaticDefaults()
	{
		NPCID.Sets.TrailCacheLength[NPC.type] = 8;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		NPC.width = 22;
		NPC.height = 22;
		NPC.scale = 2.0f;
		NPC.lifeMax = 5000;
		NPC.damage = 55; // contact — pre-Skeletron tier (Skeletron 30, Queen Bee 30; ours is the harder boss)
		NPC.defense = 22;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.behindTiles = false;

		if (!Main.dedServ)
		{
			string musicPath = "Music/UncommonAnimateTheme";
			if (MusicLoader.MusicExists(Mod, musicPath))
			{
				Music = MusicLoader.GetMusicSlot(Mod, musicPath);
			}
		}
	}

	public override void AI()
	{
		// Lock onto a single player for the whole phase. Only rotates targets when
		// ManagePhases() triggers a phase transition, exactly like CommonAnimate —
		// so the boss can't be pulled apart between two players mid-attack.
		if (_currentPhaseTarget < 0 || _currentPhaseTarget == 255 || !Main.player[_currentPhaseTarget].active || Main.player[_currentPhaseTarget].dead)
		{
			NPC.TargetClosest();
			_currentPhaseTarget = NPC.target;
		}
		NPC.target = _currentPhaseTarget;

		Player player = Main.player[NPC.target];

		if (player.dead)
		{
			NPC.velocity.Y -= 0.04f;
			NPC.EncourageDespawn(10);
			if (TryGetRed(out var red)) red.Cmd_SetDespawn();
			return;
		}

		// Force Red to mirror the boss's target so the two never desync onto different players.
		if (TryGetRed(out var redSync))
			redSync.NPC.target = NPC.target;

		if (teleportCooldown > 0) teleportCooldown--;

		if (_lastHidingHpThreshold == 0)
			_lastHidingHpThreshold = NPC.lifeMax;

		Lighting.AddLight(NPC.Center, 0.2f, 0.9f, 0.3f);

		ManagePhases();

		// Per-phase defense bump — slightly tougher in later phases to compensate for player gear scaling.
		NPC.defense = 22 + CurrentState switch
		{
			State.Phase2_CoopSpiral => 1,
			State.Phase3_CoopDashes => 2,
			_ => 0,
		};

		// Anti-despawn warp/dash if too far
		if (CurrentState != State.Intro && CurrentState != State.Hiding && CurrentState != State.Transitioning)
		{
			if (Vector2.Distance(NPC.Center, player.Center) > 1600f)
			{
				NPC.velocity = Vector2.Normalize(player.Center - NPC.Center) * 40f;
				NPC.noTileCollide = true;
				if (Main.rand.NextBool(5)) SpawnTeleportVisuals();
			}
		}

		switch (CurrentState)
		{
			case State.Intro: DoIntro(player); break;
			case State.Phase1_SkyShoot: DoPhase1(player); break;
			case State.Hiding: DoHiding(player); break;
			case State.Phase2_CoopSpiral: DoPhase2(player); break;
			case State.Phase3_CoopDashes: DoPhase3(player); break;
			case State.Transitioning: DoTransitioning(player); break;
		}

		// Clamp speed
		if (NPC.velocity.Length() > 30f)
			NPC.velocity = Vector2.Normalize(NPC.velocity) * 30f;

		// Decay polish state
		if (_scalePulse > 1f) _scalePulse = MathHelper.Lerp(_scalePulse, 1f, 0.12f);
		if (_hitFlash > 0f) _hitFlash = Math.Max(0f, _hitFlash - 0.08f);
		NPC.scale = BaseScale * _scalePulse;

		for (int i = NPC.oldPos.Length - 1; i > 0; i--)
		{
			NPC.oldPos[i] = NPC.oldPos[i - 1];
		}
		NPC.oldPos[0] = NPC.position;
	}

	private void PulseScale(float amount) { if (amount > _scalePulse) _scalePulse = amount; }

	// Wipe oldPos[] so the segmented trail in PreDraw doesn't streak across the gap after a teleport.
	private void ResetTrail()
	{
		for (int i = 0; i < NPC.oldPos.Length; i++)
			NPC.oldPos[i] = NPC.position;
	}

	private void ManagePhases()
	{
		float healthPct = (float)NPC.life / NPC.lifeMax;

		State desired = State.Phase1_SkyShoot;
		if (healthPct <= 0.35f) desired = State.Phase3_CoopDashes;
		else if (healthPct <= 0.70f) desired = State.Phase2_CoopSpiral;

		if (CurrentState != State.Hiding && CurrentState != State.Intro && CurrentState != State.Transitioning && CurrentState != desired)
		{
			// Rotate target at phase boundaries in MP so the boss doesn't fixate on one player.
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				for (int i = 1; i < Main.maxPlayers; i++)
				{
					int nextPlayer = (_currentPhaseTarget + i) % Main.maxPlayers;
					if (Main.player[nextPlayer].active && !Main.player[nextPlayer].dead)
					{
						_currentPhaseTarget = nextPlayer;
						NPC.target = _currentPhaseTarget;
						break;
					}
				}
			}

			// Stinger plays at the moment the threshold is crossed — gives the player a
			// 2-second dramatic buildup before the new phase actually starts.
			PlayPhaseTransitionStinger(desired);

			CurrentState = State.Transitioning;
			Timer = 0; Counter1 = 0; Counter2 = 0;
			SubTimer = 0; SubMode = 0; TgX = 0; TgY = 0;

			if (TryGetRed(out var red))
				red.Cmd_SetDespawn();
			_redMinionWho = -1;
		}
	}

	private void DoTransitioning(Player player)
	{
		NPC.dontTakeDamage = true; // Invulnerable during theatrical transition
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		Vector2 hover = player.Center + new Vector2(0, -300f);
		NPC.velocity = (hover - NPC.Center) * 0.05f;
		// Cap transition speed — without this, ending a Phase 3 dash far from player gives a
		// massive 30 px/tick fly-up that drags an 8-frame trail across the screen.
		if (NPC.velocity.Length() > 8f)
			NPC.velocity = Vector2.Normalize(NPC.velocity) * 8f;
		NPC.rotation = (NPC.Center - player.Center).ToRotation() + MathHelper.PiOver2;

		Timer++;

		if (Timer % 10 == 0)
			EmitGreenBurst(15, 6f, 1.2f);

		if (Timer >= 120)
		{
			NPC.dontTakeDamage = false;
			DoPhaseTransitionBurst();
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

			float healthPct = (float)NPC.life / NPC.lifeMax;
			State next = State.Phase1_SkyShoot;
			if (healthPct <= 0.35f) next = State.Phase3_CoopDashes;
			else if (healthPct <= 0.70f) next = State.Phase2_CoopSpiral;

			CurrentState = next;
			Timer = 0; Counter1 = 0; Counter2 = 0;
			SubTimer = 0; SubMode = 0; TgX = 0; TgY = 0;
		}
	}

	private void SpawnTeleportVisuals()
	{
		SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
		for (int i = 0; i < 30; i++)
		{
			Dust.NewDustPerfect(NPC.Center, DustID.GreenTorch, Main.rand.NextVector2CircularEdge(5f, 5f), 0, default, 1.5f).noGravity = true;
		}
	}

	// --- Polish helpers ---
	private void EmitGreenBurst(int particles, float radius, float scale)
	{
		for (int i = 0; i < particles; i++)
		{
			Vector2 vel = Main.rand.NextVector2CircularEdge(radius, radius);
			var d = Dust.NewDustPerfect(NPC.Center, DustID.GreenTorch, vel, 0, default, scale);
			d.noGravity = true;
		}
		int shardCount = Math.Max(1, particles / 3);
		for (int i = 0; i < shardCount; i++)
		{
			Vector2 vel = Main.rand.NextVector2Circular(radius * 0.6f, radius * 0.6f);
			var d = Dust.NewDustPerfect(NPC.Center, DustID.GrassBlades, vel, 0, default, scale * 0.8f);
			d.noGravity = true;
		}
	}

	private void DoBigSpawnBurst()
	{
		SoundEngine.PlaySound(SoundID.Item62, NPC.Center);            // Life-crystal pickup (thematic)
		SoundEngine.PlaySound(SoundID.Item74, NPC.Center);            // Magic harp shimmer
		EmitGreenBurst(60, 8f, 1.8f);
		EmitGreenBurst(25, 14f, 2.4f);
		ShakeCamera(8f, 1500f, 20, "UncommonAnimateSpawn");
	}

	private void DoPhaseTransitionBurst()
	{
		SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
		EmitGreenBurst(45, 7f, 1.6f);
		ShakeCamera(5f, 1200f, 16, "UncommonAnimatePhaseTransition");
	}

	private void EmitSmallPuff(int count = 12)
	{
		for (int i = 0; i < count; i++)
		{
			var d = Dust.NewDustPerfect(NPC.Center, DustID.GreenTorch, Main.rand.NextVector2Circular(2.5f, 2.5f), 0, default, 1.2f);
			d.noGravity = true;
		}
	}

	private void ShakeCamera(float strength, float range, int frames, string id)
	{
		if (Main.LocalPlayer?.active != true) return;
		if (!Main.LocalPlayer.WithinRange(NPC.Center, range)) return;
		Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, Main.rand.NextVector2Unit(), strength, 6f, frames, range, id));
	}

	private bool TryGetRed(out RedAnimateMinion red)
	{
		red = null;
		if (_redMinionWho < 0 || _redMinionWho >= Main.maxNPCs) return false;
		NPC n = Main.npc[_redMinionWho];
		if (!n.active || n.ModNPC is not RedAnimateMinion rm) { _redMinionWho = -1; return false; }
		red = rm;
		return true;
	}

	private void EnsureRedExists(Vector2 spawnPos)
	{
		if (TryGetRed(out _)) return;
		if (Main.netMode == NetmodeID.MultiplayerClient) return;
		int who = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<RedAnimateMinion>());
		if (who >= 0 && who < Main.maxNPCs)
		{
			_redMinionWho = who;
			// Initialize target to spawn position so Red doesn't warp to world origin on his first tick
			// (default ai[2]/ai[3] are 0, which DoIdleAir would treat as a target far from spawn and warp to).
			Main.npc[who].ai[2] = spawnPos.X;
			Main.npc[who].ai[3] = spawnPos.Y;
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, who);
		}
	}

	// Slave Red to the boss-relative position by reflecting boss's position across the player.
	// Used during APPROACH, TELEGRAPH, and IDLE_BETWEEN — wherever Red shouldn't be doing his
	// own AI lerping. Boss writes Red's Center directly; Red's CmdSlaved keeps him still.
	private void SyncRedMirrored(Player player)
	{
		if (!TryGetRed(out var red)) return;
		red.Cmd_SetSlaved();
		red.NPC.Center = 2f * player.Center - NPC.Center;
		red.NPC.velocity = Vector2.Zero;
	}

	// Same as SyncRedMirrored but with an explicit override position — used by M1 Pincer
	// where Red's Y is computed from a snapshot of player.Y at teleport time, not from the
	// live player position (so the safe band remains where the player teleported in).
	private void SyncRedExplicit(Vector2 redPos)
	{
		if (!TryGetRed(out var red)) return;
		red.Cmd_SetSlaved();
		red.NPC.Center = redPos;
		red.NPC.velocity = Vector2.Zero;
	}

	// Capture launch direction and peak (2x base) so SubMode 3 can scale velocity each tick.
	private void LaunchPhase3Dash(Vector2 unitDir, float baseSpeed)
	{
		_dashDirX = unitDir.X;
		_dashDirY = unitDir.Y;
		_dashPeakSpeed = baseSpeed * 2f;
		NPC.velocity = unitDir * _dashPeakSpeed;
	}

	// Per-tick velocity for a Phase 3 dash: linear decay from peak to 0 over Phase3DashDuration.
	// Average is baseSpeed × 1, so travel distance ≈ baseSpeed × duration (same as a constant 1× dash).
	// Also drives a CONSTANT visible roll throughout the dash — independent of decaying velocity —
	// so the boss never appears to "stop spinning" as he coasts to a stop.
	private void TickPhase3DashVelocity()
	{
		float ratio = MathHelper.Clamp(1f - SubTimer / Phase3DashDuration, 0f, 1f);
		NPC.velocity = new Vector2(_dashDirX, _dashDirY) * _dashPeakSpeed * ratio;

		// Roll rate decays with the same ratio as velocity — at end of dash both reach 0
		// simultaneously, so the boss smoothly stops spinning as he coasts to a stop.
		float rollDir = _dashDirX != 0f ? Math.Sign(_dashDirX) : 1f;
		NPC.rotation += rollDir * 0.35f * ratio;
	}

	// Smooth lerp approach into a Phase 3 setup position. Boss is visible and vulnerable
	// during the approach (player gets free hits while hearts reposition). Speed-capped at
	// 8 px/tick so they can never ram the player at dash speeds. Returns true on the last tick.
	private bool ExecuteSetupApproach(Vector2 destination, float duration)
	{
		if (SubTimer == 0f)
		{
			SoundEngine.PlaySound(SoundID.Item28 with { PitchVariance = 0.2f }, NPC.Center);
		}
		SubTimer++;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		NPC.alpha = 0;

		Vector2 delta = destination - NPC.Center;
		Vector2 desiredVel = delta * 0.06f;
		if (desiredVel.Length() > 8f) desiredVel = Vector2.Normalize(desiredVel) * 8f;
		NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVel, 0.2f);
		NPC.rotation += 0.05f;

		if (Main.rand.NextBool(8))
		{
			var d = Dust.NewDustPerfect(NPC.Center, DustID.GreenTorch, NPC.velocity * -0.3f, 0, default, 1.0f);
			d.noGravity = true;
		}

		return SubTimer >= duration;
	}

	// ====================================================================
	// INTRO
	// ====================================================================
	private void DoIntro(Player player)
	{
		if (Timer == 0)
		{
			NPC.Center = player.Center + new Vector2(0, -260);
			NPC.velocity = Vector2.Zero;
			NPC.alpha = 255;       // Start invisible — we'll fade in during the intro
			ResetTrail();
			DoBigSpawnBurst();
		}

		Timer++;
		// Fade in over the first ~45 ticks
		NPC.alpha = (int)MathHelper.Clamp(255f * (1f - Timer / 45f), 0f, 255f);

		// Sparkle dust trickling out during the intro for ambient life
		if (Main.rand.NextBool(3))
		{
			var d = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.GreenTorch, Main.rand.NextVector2Circular(1f, 1f), 0, default, 1.3f);
			d.noGravity = true;
		}

		if (Timer > 60)
		{
			NPC.alpha = 0;
			DoPhaseTransitionBurst();
			SoundEngine.PlaySound(SoundID.Roar, NPC.Center);   // Roar to mark "fight begins"
			CurrentState = State.Phase1_SkyShoot;
			Timer = 0; Counter1 = 0; Counter2 = 0;
			SubTimer = 0; SubMode = 0;
		}
	}

	// ====================================================================
	// PHASE 1: Green sky-shoots with predicted aim, Red runs P1 roll on ground.
	// 4-shot packs (2 direct + 2 predicted), 3s breather between packs.
	// After 3 packs, attempt hide-and-heal.
	// ====================================================================
	// ai[2] = shotsInCurrentPack (0..4)
	// ai[3] = packsCompleted (0..3)
	// SubTimer = tick within current shot's 180-tick cycle, OR breather progress
	// SubMode = 0 shooting / 1 breather
	private void DoPhase1(Player player)
	{
		NPC.alpha = 0;
		// During the breather Green "chills" — gravity + tile collision on so he can land.
		bool isShooting = SubMode == 0f;
		NPC.noGravity = isShooting;
		NPC.noTileCollide = isShooting;

		// Red rolls only during the active shooting portion. He poofs away during the breather
		// so the player can clearly see the melee window on Green.
		if (SubMode == 0f)
		{
			EnsureRedExists(player.Center + new Vector2(Main.rand.NextBool() ? -900f : 900f, 0f));
			if (TryGetRed(out var red))
				red.Cmd_SetP1Roll();
		}

		// If we just woke from hiding, teleport above player rather than approaching naturally
		if (_justWokeFromHide)
		{
			SpawnTeleportVisuals();
			NPC.Center = player.Center + new Vector2(0, -260f);
			NPC.velocity = Vector2.Zero;
			ResetTrail();
			SpawnTeleportVisuals();
			_justWokeFromHide = false;
		}

		// Smooth hover above player with a slight sine drift
		Timer++;
		float lateral = (float)Math.Sin(Timer * 0.02f) * 220f;
		Vector2 hover = player.Center + new Vector2(lateral, -260f);
		if (SubMode == 0f)
		{
			// Lerp toward the hover velocity so the shot recoil below persists for a few
			// frames and bleeds off smoothly instead of being wiped on the next tick.
			Vector2 desiredVel = (hover - NPC.Center) * 0.04f;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVel, 0.08f);
		}
		
		// Smoothly rotate to face directly away from the player.
		// + PiOver2 because the heart sprite's natural "up" (curves on top) is the facing
		// direction; ToRotation() assumes facing-right, so we rotate 90° to match.
		float targetRot = (NPC.Center - player.Center).ToRotation() + MathHelper.PiOver2;
		float diff = MathHelper.WrapAngle(targetRot - NPC.rotation);
		NPC.rotation = MathHelper.WrapAngle(NPC.rotation + diff * 0.18f);

		if (SubMode == 0f) // Shooting
		{
			SubTimer++;
			const float cycle = 80f;           // 1.5x faster (from 120)
			const float telegraphStart = 50f;  // last 30 ticks of the cycle are telegraph

			if (SubTimer < telegraphStart)
			{
				// Idle drift, occasional dust
				if (Main.rand.NextBool(20))
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GreenTorch);
			}
			else if (SubTimer < cycle)
			{
				// Telegraph phase. Compute target at start.
				if (SubTimer == telegraphStart)
				{
					bool predicted = Counter1 >= 2;
					Vector2 target = predicted ? PredictPlayerPos(player, NPC.Center, 9f) : player.Center;
					TgX = target.X;
					TgY = target.Y;
					SoundEngine.PlaySound(SoundID.Item15 with { PitchVariance = 0.2f }, NPC.Center);
				}
				if (Main.rand.NextBool(2))
				{
					Vector2 spawnPos = NPC.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
					Dust d = Dust.NewDustPerfect(spawnPos, DustID.GreenTorch);
					d.velocity = (NPC.Center - spawnPos) * 0.08f;
					d.noGravity = true;
				}
			}

			if (SubTimer >= cycle)
			{
				// Fire
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 target = new(TgX, TgY);
					Vector2 vel = Vector2.Normalize(target - NPC.Center) * 9f;
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<UncommonShardProjectile>(), 30, 0, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item8 with { PitchVariance = 0.2f }, NPC.Center);
				PulseScale(1.18f);
				// Recoil — full overwrite (not +=) so the kick is visible even if hover lerp
				// was already pushing toward the player. Hover Lerp then absorbs it smoothly.
				Vector2 fireDir = Vector2.Normalize(new Vector2(TgX, TgY) - NPC.Center);
				NPC.velocity = -fireDir * 8f;
				Counter1++;
				SubTimer = 0f;

				if (Counter1 >= 4f)
				{
					// Enter breather — exhale cue + Red rolls at player
					SubMode = 1f;
					SubTimer = 0f;
					Counter1 = 0f;
					Counter2++;
					if (TryGetRed(out var redOut))
						redOut.Cmd_SetP2P3Roll();
					
					SoundEngine.PlaySound(SoundID.Item25 with { PitchVariance = 0.1f }, NPC.Center);   // soft chime — "phew"
					EmitSmallPuff(18);
				}
			}
		}
		else // Breather
		{
			SubTimer++;
			// Slow down for melee window — smoothly damp horizontal velocity, let gravity pull down
			NPC.velocity.X *= 0.95f; 
			if (NPC.velocity.Y == 0) NPC.velocity.Y += (float)Math.Sin(Timer * 0.05f) * 0.15f; // Bob if landed
			if (Main.rand.NextBool(10))
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GreenTorch);

			if (SubTimer >= 180f)
			{
				// Reset Red before starting the new cycle
				if (TryGetRed(out var rReset)) rReset.Cmd_SetDespawn();
				_redMinionWho = -1;

				// Resume shooting; after 3 packs, attempt hide
				if (Counter2 >= 3f)
				{
					Counter2 = 0f;
					StartHiding(State.Phase1_SkyShoot);
					return;
				}
				SubMode = 0f;
				SubTimer = 0f;
				Counter1 = 0f;
				SoundEngine.PlaySound(SoundID.Item28 with { PitchVariance = 0.2f }, NPC.Center);   // charge cue — "back to it"
				EmitSmallPuff(14);
			}
		}
	}

	// Subtle prediction — mostly aim AT the player, with a slight lead toward where they're
	// heading. 15-tick (0.25s) lookahead, capped at 200 px so fast mounts can't yank the
	// reticle off-screen. The dash still primarily threatens the player's current spot.
	private static Vector2 PredictPlayer(Player player)
	{
		Vector2 lead = player.velocity * 15f;
		if (lead.LengthSquared() > 200f * 200f)
			lead = Vector2.Normalize(lead) * 200f;
		return player.Center + lead;
	}

	private static Vector2 PredictPlayerPos(Player player, Vector2 shooterPos, float projSpeed)
	{
		float dist = Vector2.Distance(shooterPos, player.Center);
		float t = MathHelper.Clamp(dist / projSpeed, 0f, 60f); // cap lookahead to 1s
		return player.Center + player.velocity * t;
	}

	// ====================================================================
	// HIDING (same mechanic as CommonAnimate, but on wake → teleport above player)
	// ====================================================================
	private float PreviousState { get => NPC.localAI[1]; set => NPC.localAI[1] = value; }

	private void StartHiding(State returnState)
	{
		if (NPC.life > _lastHidingHpThreshold)
		{
			// Anti-stall: skip hide if we haven't lost at least 5% HP since the last hide
			Timer = 0; Counter1 = 0; Counter2 = 0;
			SubTimer = 0; SubMode = 0;
			return;
		}

		_lastHidingHpThreshold = NPC.life - (int)(NPC.lifeMax * 0.05f);

		// Despawn Red — keeps the "hunt for the hidden boss" tension intact
		if (TryGetRed(out var red))
			red.Cmd_SetDespawn();
		_redMinionWho = -1;

		CurrentState = State.Hiding;
		Timer = -30; // 0.5s windup
		PreviousState = (float)returnState;
		Counter1 = 0; Counter2 = 0;
		SubTimer = 0; SubMode = 0;
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
			float hideDir = Main.rand.NextBool() ? -1f : 1f;
			float distance = Main.rand.NextFloat(1280f, 1600f);
			Vector2 tryPos = Main.player[NPC.target].Center + new Vector2(distance * hideDir, -400f);

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
							int cx = tileX + x; int cy = tileY - y;
							if (WorldGen.InWorld(cx, cy) && Main.tile[cx, cy].HasTile && Main.tileSolid[Main.tile[cx, cy].TileType] && !Main.tileSolidTop[Main.tile[cx, cy].TileType])
							{
								isPerfect = false; break;
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
								hasBasicAir = false; break;
							}
						}
					}

					if (isPerfect)
					{
						bestPos = new Vector2(tileX * 16f + 8f, tileY * 16f - (NPC.height / 2f) - 2f);
						foundPerfectSpot = true; break;
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
		ResetTrail();
		NPC.noGravity = false;
		NPC.noTileCollide = false;
		SpawnTeleportVisuals();
	}

	private void DoHiding(Player player)
	{
		if (Timer < 0) // Pre-hide windup
		{
			NPC.velocity *= 0.8f;
			NPC.alpha = (int)MathHelper.Clamp(255f * ((30f + Timer) / 30f), 0f, 255f); // Fade out

			if (Timer == -30) SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
			if (Main.rand.NextBool(2)) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GreenTorch);

			Timer++;
			if (Timer == 0)
			{
				ExecuteHideTeleport();
				NPC.alpha = 150;
			}
			return;
		}

		if (Timer < 1000)
		{
			NPC.velocity.X *= 0.9f;
			// Slow breathing alpha — visually communicates "asleep / vulnerable"
			NPC.alpha = 130 + (int)(40f * Math.Sin(Timer * 0.05f));

			// Healing sparkle — small green motes drifting upward from the boss
			if (Main.rand.NextBool(4))
			{
				Vector2 spawn = NPC.Center + Main.rand.NextVector2Circular(28f, 28f);
				var d = Dust.NewDustPerfect(spawn, DustID.GreenTorch, new Vector2(0f, -Main.rand.NextFloat(0.5f, 1.5f)), 0, default, 1.1f);
				d.noGravity = true;
				d.fadeIn = 1.0f;
			}

			float phaseBonus = 1.10f; // Phase 1
			if (PreviousState == (float)State.Phase2_CoopSpiral) phaseBonus = 1.20f;
			else if (PreviousState == (float)State.Phase3_CoopDashes) phaseBonus = 1.30f;

			float hpPct = (float)NPC.life / NPC.lifeMax;
			float progressiveMultiplier = MathHelper.Lerp(1.5f, 1.0f, hpPct); // Up to 1.5x faster at 0 HP
			float baseHealRate = 2f * (20f * NPC.lifeMax / 1200f * phaseBonus) / 60f; // 2x as fast as Common Animate

			SubTimer += baseHealRate * progressiveMultiplier;
			if (SubTimer >= 1f)
			{
				int heal = (int)SubTimer;
				SubTimer -= heal;
				if (NPC.life < NPC.lifeMax)
				{
					NPC.life += heal;
					if (NPC.life > NPC.lifeMax) NPC.life = NPC.lifeMax;
					NPC.HealEffect(heal, true);
				}
			}

			Timer++;
		}

		// Auto-wake after 10 idle seconds (no hits)
		if (Timer >= 600 && Timer < 1000)
		{
			WakeFromHiding();
			return;
		}

		// Interrupt from hit/contact
		if (Timer >= 0 && Timer < 1000 && (NPC.Hitbox.Intersects(player.Hitbox) || NPC.justHit))
		{
			WakeFromHiding();
			return;
		}
	}

	private void WakeFromHiding()
	{
		NPC.alpha = 0;
		// Impact polish — being struck out of a heal is a big satisfying moment
		SoundEngine.PlaySound(SoundID.Item62, NPC.Center);        // Life-crystal shatter
		SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
		SoundEngine.PlaySound(SoundID.Item14, NPC.Center);        // Impact bang
		EmitGreenBurst(50, 8f, 2.0f);
		EmitGreenBurst(22, 13f, 2.6f);
		// Sparks
		for (int i = 0; i < 20; i++)
			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GreenTorch);
		for (int i = 0; i < 10; i++)
		{
			var d = Dust.NewDustPerfect(NPC.Center, DustID.GrassBlades, Main.rand.NextVector2Circular(7f, 7f), 0, default, 1.6f);
			d.noGravity = true;
		}
		ShakeCamera(7f, 1500f, 18, "UncommonAnimateWake");

		float hpPct = (float)NPC.life / NPC.lifeMax;
		State next = State.Phase1_SkyShoot;
		if (hpPct <= 0.35f) next = State.Phase3_CoopDashes;
		else if (hpPct <= 0.70f) next = State.Phase2_CoopSpiral;

		// In case the player drained him into a new phase while hidden, fire the stinger here too
		// (the guard flags ensure it never double-plays).
		PlayPhaseTransitionStinger(next);

		CurrentState = next;
		Timer = 0; Counter1 = 0; Counter2 = 0;
		SubTimer = 0; SubMode = 0; TgX = 0; TgY = 0;
		_justWokeFromHide = true;
	}

	// ====================================================================
	// PHASE 2: Co-op spiral, alternating telegraphed shots
	// ai[1] = Timer (orbit driver + shot pacing, 60-tick cycle per shot)
	// ai[2] = shotsThisRound (0..12)
	// ai[3] = roundsCompleted (0..2)
	// SubMode = 0 orbit/shoot loop, 1 breather (3s, red despawned)
	// ====================================================================
	private const float OrbitRadius = 360f;
	private const float OrbitAngularSpeed = 0.015f; // radians per tick (~57° per second)

	private void DoPhase2(Player player)
	{
		// Handle in-progress safety teleport first — 0.5s dust telegraph at the destination,
		// then warp. Boss is invincible during the warp so the player can't get a free hit on
		// a sitting target, mirroring CommonAnimate's invisible-teleport pattern.
		if (_p2TeleportActive == 1f)
		{
			NPC.velocity = Vector2.Zero;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.alpha = 255;
			NPC.dontTakeDamage = true;
			_p2TeleportTimer++;

			Vector2 telePos = new(_p2TeleportTargetX, _p2TeleportTargetY);
			for (int i = 0; i < 2; i++)
			{
				Dust d = Dust.NewDustDirect(telePos, NPC.width, NPC.height, DustID.GreenTorch);
				d.velocity = Main.rand.NextVector2Circular(3f, 3f);
				d.noGravity = true;
			}
			// A few pink motes too to match the boss's signature teleport poof palette
			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(telePos, NPC.width, NPC.height, DustID.PinkCrystalShard);
				d.velocity = Main.rand.NextVector2Circular(3f, 3f);
				d.noGravity = true;
			}

			if (_p2TeleportTimer >= 30f)
			{
				NPC.Center = telePos;
				ResetTrail();
				NPC.alpha = 0;
				NPC.dontTakeDamage = false;
				SpawnTeleportVisuals();
				_p2TeleportActive = 0f;
				_p2TeleportTimer = 0f;
				teleportCooldown = 180;
			}
			return;
		}

		NPC.alpha = 0;
		// During the breather (SubMode 1) Green "chills" — gravity + tile collision on
		bool isOrbiting = SubMode == 0f;
		NPC.noGravity = isOrbiting;
		NPC.noTileCollide = isOrbiting;

		Timer++;

		// Orbit position
		float angle = Timer * OrbitAngularSpeed;
		Vector2 greenPos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * OrbitRadius;
		Vector2 redPos = player.Center + new Vector2((float)Math.Cos(angle + (float)Math.PI), (float)Math.Sin(angle + (float)Math.PI)) * OrbitRadius;

		if (isOrbiting)
		{
			// Telegraphed safety teleport if a dash or piercing weapon flung us way off-orbit.
			// Threshold widened from 360 → 640 so small bumps don't trigger it, and the 3-second
			// cooldown prevents back-to-back warps.
			if (Vector2.Distance(NPC.Center, greenPos) > 640f && teleportCooldown == 0)
			{
				_p2TeleportActive = 1f;
				_p2TeleportTimer = 0f;
				_p2TeleportTargetX = greenPos.X;
				_p2TeleportTargetY = greenPos.Y;
				SpawnTeleportVisuals();
				NPC.alpha = 255;
				NPC.dontTakeDamage = true;
				NPC.velocity = Vector2.Zero;
				return;
			}
			// Lerp toward orbit velocity instead of overwriting so projectile recoil persists
			// for a few frames and decays naturally before snapping back to orbit.
			Vector2 desiredVel = (greenPos - NPC.Center) * 0.12f;
			NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVel, 0.15f);
			NPC.rotation += 0.05f;

			if (Main.rand.NextBool(20))
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GreenTorch);
		}

		if (SubMode == 0f) // Orbit + alternate shoot
		{
			EnsureRedExists(redPos);
			// Red ALWAYS stays in CmdIdleAir following the orbit. Boss handles both telegraphs
			// itself so Red can keep moving and so we never overwrite his TargetX/Y mid-action.
			if (TryGetRed(out var red))
				red.Cmd_SetIdleAir(redPos);

			SubTimer++;
			float pacing = 60f;
			bool greenShoots = (((int)Counter1) % 2) == 1;

			// Start a telegraph at the beginning of each 60-tick slot
			if (SubTimer == 1f)
			{
				if (greenShoots)
				{
					_greenTelegraphActive = 1f;
					_greenTelegraphTimer = 0f;
				}
				else
				{
					_redTelegraphActive = 1f;
					_redTelegraphTimer = 0f;
				}
				SoundEngine.PlaySound(SoundID.Item15 with { PitchVariance = 0.2f }, TryGetRed(out var rs) && !greenShoots ? rs.NPC.Center : NPC.Center);
			}

			// Green telegraph + fire
			if (_greenTelegraphActive == 1f)
			{
				_greenTelegraphTimer++;
				// Tracks player live for the duration
				TgX = player.Center.X; TgY = player.Center.Y;

				if (Main.rand.NextBool(2))
				{
					Vector2 spawnPos = NPC.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
					Dust d = Dust.NewDustPerfect(spawnPos, DustID.GreenTorch);
					d.velocity = (NPC.Center - spawnPos) * 0.08f;
					d.noGravity = true;
				}

				if (_greenTelegraphTimer >= TelegraphDurationP2)
				{
					Vector2 projVel = Vector2.Normalize(player.Center - NPC.Center) * 9f;
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projVel, ModContent.ProjectileType<UncommonShardProjectile>(), 30, 0, Main.myPlayer);
					}
					SoundEngine.PlaySound(SoundID.Item8 with { PitchVariance = 0.2f }, NPC.Center);
					PulseScale(1.16f);
					// Recoil — kicked back along the firing axis. Orbit lerp smoothly pulls us back.
					NPC.velocity = -projVel.SafeNormalize(Vector2.Zero) * 8f;
					_greenTelegraphActive = 0f;
					_greenTelegraphTimer = 0f;
				}
			}

			// Red telegraph + fire (boss-orchestrated; Red keeps orbiting)
			if (_redTelegraphActive == 1f)
			{
				_redTelegraphTimer++;
				if (_redTelegraphTimer >= TelegraphDurationP2)
				{
					if (TryGetRed(out var rShoot))
					{
						Vector2 projVel = Vector2.Normalize(player.Center - rShoot.NPC.Center) * 9f;
						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							Projectile.NewProjectile(NPC.GetSource_FromAI(), rShoot.NPC.Center, projVel, ModContent.ProjectileType<AnimateShardProjectile>(), 30, 0, Main.myPlayer);
						}
						SoundEngine.PlaySound(SoundID.Item8 with { PitchVariance = 0.2f }, rShoot.NPC.Center);
						// Recoil Red the same way Green is recoiled — the boss orbits Red around the
						// player via Cmd_SetIdleAir each tick, so the lerp pulls him back smoothly.
						rShoot.NPC.velocity = -projVel.SafeNormalize(Vector2.Zero) * 8f;
					}
					_redTelegraphActive = 0f;
					_redTelegraphTimer = 0f;
				}
			}

			if (SubTimer >= pacing)
			{
				SubTimer = 0f;
				Counter1++;
				if (Counter1 >= 12f)
				{
					// End of round: red rolls at player, green chills for 3s
					if (TryGetRed(out var rRoll))
						rRoll.Cmd_SetP2P3Roll();
					_redMinionWho = -1; // Detach reference momentarily so orbit logic doesn't conflict
					_redTelegraphActive = 0f;
					_greenTelegraphActive = 0f;
					Counter1 = 0f;
					SubMode = 1f;
					SubTimer = 0f;
					SoundEngine.PlaySound(SoundID.Item25 with { PitchVariance = 0.1f }, NPC.Center);
					EmitSmallPuff(20);
				}
			}
		}
		else // Breather (red gone, green idles 3s for melee window)
		{
			NPC.velocity *= 0.96f;
			NPC.velocity.Y += (float)Math.Sin(Timer * 0.05f) * 0.15f;
			SubTimer++;
			if (SubTimer >= 180f)
			{
				SubTimer = 0f;
				Counter2++;
				if (Counter2 >= 2f)
				{
					// After 2 rounds → heal attempt. If skipped (not 5% lost), reset and continue looping.
					Counter2 = 0f;
					StartHiding(State.Phase2_CoopSpiral);
					return;
				}
				SubMode = 0f;
				
				// Reset Red before starting the new orbit cycle
				if (TryGetRed(out var rReset)) rReset.Cmd_SetDespawn();
				_redMinionWho = -1;
				
				SoundEngine.PlaySound(SoundID.Item28 with { PitchVariance = 0.2f }, NPC.Center);
				EmitSmallPuff(16);
			}
		}
	}

	// ====================================================================
	// PHASE 3: Co-op dashes — 3 moves, random order, no immediate repeats.
	// After 3 moves attempt hide+heal.
	// ai[2] = currentMove (1=dual slice, 2=predictive, 3=ground sweep)
	// ai[3] = movesCompleted
	// SubMode = sub-step (0=pick, 1=approach, 2=telegraph track+lock, 3=dash, 5=chill after 3 moves, 6=idle between moves)
	// SubTimer = sub-step timer
	// ====================================================================
	private void DoPhase3(Player player)
	{
		NPC.alpha = 0;

		// SubMode 5 = chill (only entered after 3 moves). Gravity ON so Green falls onto blocks/platforms.
		if (SubMode == 5f)
		{
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.velocity.X *= 0.95f;
			if (NPC.velocity.Y == 0) NPC.velocity.Y += (float)Math.Sin(Timer * 0.05f) * 0.15f; // Add bobbing even here if on ground, well, only if hovering. Let's just do it
			SubTimer++;
			if (SubTimer >= 180f) // 3-second chill window — melee players get a real damage opportunity
			{
				Counter2 = 0f;
				// Reset Red before hiding or returning
				if (TryGetRed(out var rReset)) rReset.Cmd_SetDespawn();
				_redMinionWho = -1;

				StartHiding(State.Phase3_CoopDashes);
			}
			return;
		}

		// SubMode 6 = inter-move IDLE. Hearts drift naturally for a moment instead of
		// instantly snapping into the next move's setup — keeps the fight breathing.
		if (SubMode == 6f)
		{
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.alpha = 0;

			SubTimer++;
			// Gentle bob: drift toward player at conversational speed + small vertical sine.
			Vector2 toPlayer = player.Center - NPC.Center;
			Vector2 driftVel = (toPlayer.Length() > 1f ? Vector2.Normalize(toPlayer) : Vector2.Zero) * 2.0f
				+ new Vector2(0, (float)Math.Sin(SubTimer * 0.15f) * 0.8f);
			NPC.velocity = Vector2.Lerp(NPC.velocity, driftVel, 0.08f);
			NPC.rotation += 0.04f;

			// Red is slaved — perfectly mirrored across the player every tick.
			SyncRedMirrored(player);

			if (SubTimer >= 45f) // ~0.75s breather
			{
				SubMode = 0f;
				SubTimer = 0f;
			}
			return;
		}

		// Active moves are airborne
		NPC.noGravity = true;
		NPC.noTileCollide = true;

		if (SubMode == 0f) // PICK MOVE
		{
			if (_p3BagIndex >= 3)
			{
				do {
					for (int i = 0; i < 3; i++)
					{
						int swapIndex = Main.rand.Next(3);
						int temp = _p3Bag[i];
						_p3Bag[i] = _p3Bag[swapIndex];
						_p3Bag[swapIndex] = temp;
					}
				} while (_p3Bag[0] == _lastPhase3Move);
				_p3BagIndex = 0;
			}
			
			int move = _p3Bag[_p3BagIndex++];
			_lastPhase3Move = move;
			
			Counter1 = move;
			SubMode = 1f;
			SubTimer = 0f;
			Timer = 0f;

			// Ensure red exists at a sensible position (further off for dodging room)
			EnsureRedExists(player.Center + new Vector2(Main.rand.NextBool() ? -700 : 700, -200));

			// Cue: alerting the player a new move is being chosen
			SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
			EmitSmallPuff(14);
			return;
		}

		switch ((int)Counter1)
		{
			case 1: DoP3MovePincer(player); break;
			case 2: DoP3MovePredictive(player); break;
			case 3: DoP3MoveGroundSweep(player); break;
		}
	}

	// MOVE 1 — DUAL SLICE (Pincer):
	//   Setup: both hearts teleport in at the player's current Y, 30 blocks apart on X
	//          (15 blocks left for red, 15 blocks right for green), player sandwiched in middle.
	//   Aim:   over 2s, green drifts UP 3 blocks and red drifts DOWN 3 blocks. Lasers are flat-
	//          horizontal toward the player's X direction and track the heart's current Y.
	//   Lock:  0.5s frozen — final positions and dash directions snapshot.
	//   Dash:  pure horizontal at locked Y; each heart dashes toward wherever the player is
	//          on the X axis at the moment of launch (sign of player.X − heart.X).
	//   Dodge: stay at your original Y. Green passes 3 blocks above, red 3 below — the band
	//          right where you teleported in is the safe zone. Jumping or falling = hit.
	private void DoP3MovePincer(Player player)
	{
		const float halfSep = 37.5f * 16f;   // 37.5 blocks each side → 75 blocks total apart
		const float vertOffset = 3f * 16f;   // 3 blocks of vertical separation by lock time
		const float teleportDur = 30f;       // 0.5s teleport telegraph
		const float aimDur = 120f;           // 2s aiming (vertical drift)
		const float lockDur = 30f;           // 0.5s final lock

		if (SubMode == 1f) // TELEPORT IN — dust telegraph at destination, then snap
		{
			if (SubTimer == 0f)
			{
				_pincerStartY = player.Center.Y;
				_pincerGreenX = player.Center.X + halfSep;
				_pincerRedX = player.Center.X - halfSep;

				if (TryGetRed(out var redTel))
					redTel.Cmd_SetTelegraphTeleport(new Vector2(_pincerRedX, _pincerStartY), teleportDur);
			}

			SubTimer++;
			NPC.velocity = Vector2.Zero;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.alpha = 255;
			NPC.dontTakeDamage = true;

			if (SubTimer == 1f)
				SoundEngine.PlaySound(SoundID.Item8 with { PitchVariance = 0.2f }, NPC.Center);

			Vector2 dest = new(_pincerGreenX, _pincerStartY);
			for (int i = 0; i < 2; i++)
			{
				Dust d = Dust.NewDustDirect(dest, NPC.width, NPC.height, DustID.GreenTorch);
				d.velocity = Main.rand.NextVector2Circular(3f, 3f);
				d.noGravity = true;
			}

			if (SubTimer >= teleportDur)
			{
				NPC.Center = dest;
				ResetTrail();
				NPC.alpha = 0;
				NPC.dontTakeDamage = false;
				SpawnTeleportVisuals();
				SubMode = 2f;
				SubTimer = 0f;
			}
		}
		else if (SubMode == 2f) // AIM (vertical drift over 2s) + LOCK (0.5s)
		{
			SubTimer++;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.velocity = Vector2.Zero;

			bool isAiming = SubTimer <= aimDur;

			// Vertical position: lerp from start Y to ±vertOffset over aimDur ticks, then hold.
			float aimProgress = MathHelper.Clamp(SubTimer / aimDur, 0f, 1f);
			float greenY = MathHelper.Lerp(_pincerStartY, _pincerStartY - vertOffset, aimProgress);
			float redY = MathHelper.Lerp(_pincerStartY, _pincerStartY + vertOffset, aimProgress);

			NPC.Center = new Vector2(_pincerGreenX, greenY);
			// Red is a pure puppet — boss writes his position; CmdSlaved keeps him still.
			SyncRedExplicit(new Vector2(_pincerRedX, redY));

			// Dash direction: track live while aiming, snapshot at lock.
			if (isAiming)
			{
				float gDir = Math.Sign(player.Center.X - NPC.Center.X);
				if (gDir == 0f) gDir = -1f;
				_pincerGreenDashDirX = gDir;

				if (TryGetRed(out var redDir))
				{
					float rDir = Math.Sign(player.Center.X - redDir.NPC.Center.X);
					if (rDir == 0f) rDir = 1f;
					_pincerRedDashDirX = rDir;
				}
			}
			// else: directions frozen — laser already shows the locked-in direction

			// Green laser endpoint — purely horizontal at boss's current Y, pointing toward player's X.
			TgX = NPC.Center.X + _pincerGreenDashDirX * 3000f;
			TgY = NPC.Center.Y;

			// Drive boss-orchestrated red laser (PreDraw uses _redTelegraphActive / _redTelegraphTimer).
			_redTelegraphActive = 1f;
			_redTelegraphTimer = Math.Min(SubTimer, aimDur); // fade in over aim phase, hold during lock

			if (Main.rand.NextBool(2))
			{
				Vector2 spawnPos = NPC.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
				Dust d = Dust.NewDustPerfect(spawnPos, DustID.GreenTorch);
				d.velocity = (NPC.Center - spawnPos) * 0.08f;
				d.noGravity = true;
			}

			if (SubTimer >= aimDur + lockDur)
			{
				// LAUNCH — flat horizontal dashes at locked Y; direction toward player.
				LaunchPhase3Dash(new Vector2(_pincerGreenDashDirX, 0f), 10f);

				if (TryGetRed(out var rDash))
				{
					rDash.Cmd_SetDashImmediate(new Vector2(_pincerRedDashDirX * 20f, 0f), Phase3DashDuration);
				}

				_redTelegraphActive = 0f;
				_redTelegraphTimer = 0f;

				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				SpawnTeleportVisuals();
				PulseScale(1.30f);
				ShakeCamera(3.5f, 1100f, 10, "UncommonAnimateDash");
				EmitGreenBurst(30, 8f, 1.5f);
				ShakeCamera(4f, 1000f, 10, "UncommonAnimateDash");
				SubMode = 3f;
				SubTimer = 0f;
			}
		}
		else if (SubMode == 3f) // DASH — flat horizontal, decay 2x → 0 over Phase3DashDuration
		{
			TickPhase3DashVelocity();
			// No rotation update — sprite stays visually flat for the horizontal pincer sweep.
			SubTimer++;
			if (Main.rand.NextBool())
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GreenTorch);
				d.velocity = NPC.velocity * -0.5f;
			}
			if (SubTimer > Phase3DashDuration)
				FinishPhase3Move();
		}
	}

	// MOVE 2 — PREDICTIVE:
	//   Setup: green/red align on opposite sides of player at radius
	//   Telegraph: both telegraph dash toward predicted player position (60-tick lock-in)
	//   Action: both dash
	//   Chill: green idles 3s for melee
	private void DoP3MovePredictive(Player player)
	{
		const float radius = 320f;
		// Use TgX as a stable per-move angle seed. Set on the first tick (Counter1 was just assigned).
		if (SubMode == 1f && SubTimer == 0f && TgX == 0f && TgY == 0f)
		{
			// Bias toward horizontal angles (the dash reads better when boss starts on a side)
			float a = Main.rand.NextFloat(-0.7f, 0.7f);             // -40°..+40° from horizontal
			if (Main.rand.NextBool()) a = MathHelper.Pi - a;        // mirror to left side half the time
			TgX = (float)Math.Cos(a);
			TgY = (float)Math.Sin(a);
		}
		Vector2 greenPos = player.Center + new Vector2(TgX, TgY) * radius;
		Vector2 redPos = player.Center - (greenPos - player.Center);

		const float trackDur = 60f; // 1s tracking — laser follows the predicted spot
		const float lockDur = 30f;  // 0.5s lock

		if (SubMode == 1f) // APPROACH — smooth lerp into orbit-radius position
		{
			if (ExecuteSetupApproach(greenPos, 60f))
			{
				// Initial predict (will refresh each tick during tracking)
				Vector2 initPredict = PredictPlayer(player);
				TgX = initPredict.X;
				TgY = initPredict.Y;
				_redLaserAimX = initPredict.X;
				_redLaserAimY = initPredict.Y;
				_redLaserShown = true;
				SubMode = 2f;
				SubTimer = 0f;
			}
			// Red mirrors boss across the player throughout approach.
			SyncRedMirrored(player);
		}
		else if (SubMode == 2f) // TELEGRAPH — track predicted spot live for trackDur, then 0.5s lock
		{
			NPC.velocity = Vector2.Zero;
			SubTimer++;

			if (SubTimer <= trackDur)
			{
				// Recompute the prediction each tick — laser visibly homes in as player turns/decelerates
				Vector2 predict = PredictPlayer(player);
				TgX = predict.X;
				TgY = predict.Y;
				_redLaserAimX = predict.X;
				_redLaserAimY = predict.Y;
			}
			// else: lock — TgX/TgY frozen (and _redLaserAimX/Y too)

			// Red is slaved + mirrored every tick. Boss draws Red's laser via PreDraw.
			SyncRedMirrored(player);

			if (Main.rand.NextBool(2))
			{
				Vector2 spawnPos = NPC.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
				Dust d = Dust.NewDustPerfect(spawnPos, DustID.GreenTorch);
				d.velocity = (NPC.Center - spawnPos) * 0.08f;
				d.noGravity = true;
			}

			if (SubTimer >= trackDur + lockDur)
			{
				// Boss launches its decaying dash; Red launches a mirrored one — peak velocity
				// is the symmetric reflection of boss's launch direction (mirror about player).
				Vector2 greenDir = Vector2.Normalize(new Vector2(TgX, TgY) - NPC.Center);
				LaunchPhase3Dash(greenDir, 11f); // peak 22 px/tick → decays to 0

				if (TryGetRed(out var rDash))
				{
					Vector2 redTarget = 2f * player.Center - new Vector2(TgX, TgY);
					Vector2 redDir = Vector2.Normalize(redTarget - rDash.NPC.Center);
					rDash.Cmd_SetDashImmediate(redDir * 22f, Phase3DashDuration);
				}
				_redLaserShown = false;

				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				SpawnTeleportVisuals();
				PulseScale(1.30f);
				ShakeCamera(3.5f, 1100f, 10, "UncommonAnimateDash");
				EmitGreenBurst(30, 8f, 1.5f); // Shockwave
				ShakeCamera(4f, 1000f, 10, "UncommonAnimateDash");
				SubMode = 3f;
				SubTimer = 0f;
			}
		}
		else if (SubMode == 3f) // DASH — decay 2x → 0 over Phase3DashDuration
		{
			TickPhase3DashVelocity(); // also handles roll rotation
			SubTimer++;
			if (Main.rand.NextBool())
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GreenTorch);
				d.velocity = NPC.velocity * -0.5f;
			}
			if (SubTimer > Phase3DashDuration)
				FinishPhase3Move();
		}
	}

	// MOVE 3 — TRACKING SWEEP:
	//   Setup: green/red fly into position on opposite sides of player at player's current Y.
	//   Telegraph: both *continuously* match player's Y (they shadow the player vertically) until launch.
	//   Action: at the launch instant Y is locked → both dash horizontally at that height.
	//   Dodge: REACT — the moment the dash launches, move vertically (jump or fall) out of the line.
	private void DoP3MoveGroundSweep(Player player)
	{
		const float dx = 28f * 16f; // 28 blocks each side → 56 blocks apart, so the player has real reaction time to jump out of the sweep line
		Vector2 greenAnchor = new(player.Center.X + dx, player.Center.Y);
		Vector2 redAnchor = new(player.Center.X - dx, player.Center.Y);

		const float trackDur = 90f; // 1.5s of Y-tracking
		const float lockDur = 30f;  // 0.5s lock

		if (SubMode == 1f) // APPROACH — lerp into position; Y tracks player throughout
		{
			_groundSweepLocked = false;

			if (ExecuteSetupApproach(greenAnchor, 60f))
			{
				SubMode = 2f;
				SubTimer = 0f;
				// Boss draws both red and green lasers via PreDraw during the telegraph
				_redTelegraphActive = 1f;
				_redTelegraphTimer = 0f;
			}
			// Red mirrors boss across the player — always exactly opposite.
			SyncRedMirrored(player);
		}
		else if (SubMode == 2f) // TELEGRAPH — trackDur of Y-tracking, then 0.5s Y-locked hold
		{
			NPC.noGravity = true;
			NPC.noTileCollide = true;

			SubTimer++;
			_redTelegraphTimer = SubTimer;

			if (SubTimer <= trackDur)
			{
				NPC.velocity = (greenAnchor - NPC.Center) * 0.25f;
				TgY = NPC.Center.Y;
			}
			else
			{
				if (!_groundSweepLocked)
				{
					_groundSweepLocked = true;
					_groundSweepGreenLockedY = NPC.Center.Y;
					_groundSweepRedLockedY = 2f * player.Center.Y - NPC.Center.Y; // mirrored
				}
				Vector2 lockedAnchor = new(greenAnchor.X, _groundSweepGreenLockedY);
				NPC.velocity = (lockedAnchor - NPC.Center) * 0.25f;
				TgY = _groundSweepGreenLockedY;
			}

			TgX = player.Center.X - dx * 2f;
			// Red is slaved + mirrored every tick.
			SyncRedMirrored(player);

			if (Main.rand.NextBool(2))
			{
				Vector2 spawnPos = NPC.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
				Dust d = Dust.NewDustPerfect(spawnPos, DustID.GreenTorch);
				d.velocity = (NPC.Center - spawnPos) * 0.08f;
				d.noGravity = true;
			}

			if (SubTimer >= trackDur + lockDur)
			{
				// LAUNCH — snap to locked Y and dash horizontally with the decay system.
				NPC.position = new Vector2(NPC.position.X, _groundSweepGreenLockedY - NPC.height / 2f);
				LaunchPhase3Dash(new Vector2(-1f, 0f), 12f); // peak -24 px/tick → decays to 0

				if (TryGetRed(out var rDash))
				{
					rDash.NPC.position = new Vector2(rDash.NPC.position.X, _groundSweepRedLockedY - rDash.NPC.height / 2f);
					rDash.Cmd_SetDashImmediate(new Vector2(+22f, 0f), Phase3DashDuration); // peak +22 px/tick → decays to 0
				}

				SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				SpawnTeleportVisuals();
				PulseScale(1.30f);
				ShakeCamera(4f, 1100f, 12, "UncommonAnimateGroundSweep");
				EmitGreenBurst(30, 8f, 1.5f);

				_redTelegraphActive = 0f;
				_redTelegraphTimer = 0f;
				SubMode = 3f;
				SubTimer = 0f;
			}
		}
		else if (SubMode == 3f) // DASH — decay 2x → 0 over Phase3DashDuration
		{
			TickPhase3DashVelocity(); // also handles roll rotation
			SubTimer++;
			if (Main.rand.NextBool())
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GreenTorch);
				d.velocity = NPC.velocity * -0.5f;
			}
			if (SubTimer > Phase3DashDuration)
				FinishPhase3Move();
		}
	}

	private void FinishPhase3Move()
	{
		EmitSmallPuff(12);
		Counter2++;
		if (Counter2 >= 3f)
		{
			// 3 moves done — enter the chill window so melee players get a damage opportunity.
			// Despawn Red cleanly here so he can't keep rolling/dashing while green is alone.
			SubMode = 5f;
			SubTimer = 0f;
			Timer = 0f;
			SoundEngine.PlaySound(SoundID.Item25, NPC.Center);
			EmitSmallPuff(16);

			if (TryGetRed(out var rDespawn))
				rDespawn.Cmd_SetDespawn();
			_redMinionWho = -1;

			return;
		}
		// Brief inter-move idle (SubMode 6) so hearts drift naturally before the next setup.
		Counter1 = 0; SubMode = 6; SubTimer = 0; Timer = 0;
		TgX = 0; TgY = 0;
	}

	// ====================================================================
	// PRE-DRAW: laser telegraphs (green from boss, red from minion in Phase 2)
	// ====================================================================
	public override Color? GetAlpha(Color drawColor)
	{
		// Punch a brief white flash on hit
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
		Color greenTint = new(40, 230, 100);
		Color redTint = new(255, 60, 40);

		// === AURA GLOW underlay — soft pulsing green halo behind the sprite ===
		if (NPC.alpha < 255)
		{
			Texture2D glow = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
			Vector2 glowOrigin = glow.Size() / 2f;
			float pulse = 1f + 0.12f * (float)Math.Sin(Main.GameUpdateCount * 0.10f);
			float alphaMul = 1f - NPC.alpha / 255f;
			Color glowColor = greenTint * (0.55f * alphaMul);
			glowColor.A = 0;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			spriteBatch.Draw(glow, NPC.Center - screenPos, null, glowColor, NPC.rotation, glowOrigin, NPC.scale * 0.85f * pulse, SpriteEffects.None, 0f);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
		}

		// Green: Phase 1 sky-shoot telegraph
		if (CurrentState == State.Phase1_SkyShoot && SubMode == 0f && SubTimer >= 50f)
		{
			float progress = MathHelper.Clamp((SubTimer - 50f) / 30f, 0f, 1f);
			DrawLaserBeam(spriteBatch, screenPos, NPC.Center, new Vector2(TgX, TgY), greenTint, progress, 2f);
		}
		// Green: Phase 2 telegraph
		if (CurrentState == State.Phase2_CoopSpiral && _greenTelegraphActive == 1f)
		{
			float progress = MathHelper.Clamp(_greenTelegraphTimer / TelegraphDurationP2, 0f, 1f);
			DrawLaserBeam(spriteBatch, screenPos, NPC.Center, new Vector2(TgX, TgY), greenTint, progress, 2f);
		}
		// Red: Phase 2 telegraph (boss-orchestrated, beam drawn from Red's live position)
		if (CurrentState == State.Phase2_CoopSpiral && _redTelegraphActive == 1f && TryGetRed(out var redTel))
		{
			float progress = MathHelper.Clamp(_redTelegraphTimer / TelegraphDurationP2, 0f, 1f);
			Player p = Main.player[NPC.target];
			DrawLaserBeam(spriteBatch, screenPos, redTel.NPC.Center, p.Center, redTint, progress, 2f);
		}
		// Green: Phase 3 dash/shoot telegraph
		if (CurrentState == State.Phase3_CoopDashes && SubMode == 2f)
		{
			// Use the *charge* duration (not the lock extension) so the beam visibly hits
			// full intensity when the position is decided, then stays at 1.0 through the lock.
			float dur = (int)Counter1 switch { 1 => 120f, 2 => 60f, 3 => 90f, _ => 60f };
			float progress = MathHelper.Clamp(SubTimer / dur, 0f, 1f);
			float thickness = 3f; // All three Phase 3 moves are dashes now — use heavy laser for all
			DrawLaserBeam(spriteBatch, screenPos, NPC.Center, new Vector2(TgX, TgY), greenTint, progress, thickness);
		}
		// Red: Phase 3 Move 3 (Tracking Sweep) telegraph — boss-orchestrated since Red stays in IdleAir.
		// Beam points from Red's live position horizontally toward the player's column at his current Y,
		// so the laser visibly tracks vertically as Red shadows the player.
		if (CurrentState == State.Phase3_CoopDashes && (int)Counter1 == 3 && SubMode == 2f && _redTelegraphActive == 1f && TryGetRed(out var redM3))
		{
			float progress = MathHelper.Clamp(_redTelegraphTimer / 90f, 0f, 1f);
			Vector2 redEnd = redM3.NPC.Center + new Vector2(1f, 0f); // unit vector right — DrawLaserBeam extends 3000 px in this direction
			DrawLaserBeam(spriteBatch, screenPos, redM3.NPC.Center, redEnd, redTint, progress, 3f);
		}
		// Red: Phase 3 Move 1 (Pincer) telegraph — flat horizontal toward the locked dash direction.
		if (CurrentState == State.Phase3_CoopDashes && (int)Counter1 == 1 && SubMode == 2f && _redTelegraphActive == 1f && TryGetRed(out var redM1))
		{
			float progress = MathHelper.Clamp(_redTelegraphTimer / 120f, 0f, 1f);
			Vector2 redEnd = redM1.NPC.Center + new Vector2(_pincerRedDashDirX, 0f);
			DrawLaserBeam(spriteBatch, screenPos, redM1.NPC.Center, redEnd, redTint, progress, 3f);
		}
		// Red: Phase 3 Move 2 (Predictive) — boss-orchestrated, points at the symmetric
		// reflection of the green aim point (player ± offset).
		if (CurrentState == State.Phase3_CoopDashes && (int)Counter1 == 2 && _redLaserShown && (SubMode == 1f || SubMode == 2f) && TryGetRed(out var redM2))
		{
			Player p = Main.player[NPC.target];
			float trackDurM2 = 60f;
			float progress = SubMode == 2f
				? MathHelper.Clamp(SubTimer / trackDurM2, 0f, 1f)
				: 0.5f;
			Vector2 redEnd = 2f * p.Center - new Vector2(_redLaserAimX, _redLaserAimY);
			DrawLaserBeam(spriteBatch, screenPos, redM2.NPC.Center, redEnd, redTint, progress, 2f);
		}

		Texture2D texture = TextureAssets.Npc[NPC.type].Value;
		Vector2 origin = NPC.frame.Size() / 2f;
		float hpPct = (float)NPC.life / NPC.lifeMax;
		float pulseRate = MathHelper.Lerp(0.05f, 0.25f, 1f - hpPct);
		float scalePulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi * pulseRate * 60f) * 0.08f;
		float drawScale = NPC.scale * scalePulse;

		// Trails — speed-gated so they only appear at actual dash speeds, never during slow
		// positioning movement (Transitioning fly-in, Phase 2 orbit, Phase 1 hover, etc).
		// Also skip uninitialized slots and bail on huge gaps so a teleport doesn't streak.
		Vector2 lastDrawnPos = NPC.Center;
		for (int i = 1; i < NPC.oldPos.Length; i++)
		{
			if (NPC.oldPos[i] == Vector2.Zero) continue;
			Vector2 oldCenter = NPC.oldPos[i] + NPC.Size / 2f;
			if (Vector2.Distance(NPC.Center, oldCenter) > 300f) break;
			
			// Prevent blobs by skipping segments that haven't moved from the previous drawn position
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

	private static void DrawLaserBeam(SpriteBatch spriteBatch, Vector2 screenPos, Vector2 startWorld, Vector2 endWorld, Color baseTint, float aimProgress, float baseThickness)
	{
		Color baseColor = baseTint * aimProgress;
		Vector2 startPos = startWorld - screenPos;
		Vector2 endPos = endWorld - screenPos;

		Texture2D magicPixel = TextureAssets.MagicPixel.Value;
		Texture2D glowTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98").Value;
		Vector2 glowOrigin = new(32f, 32f);

		float angle = (endPos - startPos).ToRotation();
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

	// Use a non-default immunity-cooldown slot so contact damage from green is on its OWN
	// timer, independent from red's (slot 2). Result: in M1 Pincer, if the player gets caught
	// in the middle, both hearts can land their hit on the same tick — nothing gets free
	// i-frame coverage from the other.
	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		cooldownSlot = ImmunityCooldownID.Bosses;
		return true;
	}

	public override void OnKill()
	{
		base.OnKill();
		if (TryGetRed(out var red))
			red.Cmd_SetDespawn();
	}

	// HitEffect runs on every client (including in MP) for hits and for the death frame.
	public override void HitEffect(NPC.HitInfo hit)
	{
		if (Main.netMode == NetmodeID.Server) return;

		if (NPC.life <= 0)
		{
			// === DEATH SPECTACLE ===
			SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);
			SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
			SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
			EmitGreenBurst(120, 10f, 2.6f);
			EmitGreenBurst(60, 18f, 3.0f);
			// Shrapnel
			for (int i = 0; i < 30; i++)
			{
				var d = Dust.NewDustPerfect(NPC.Center, DustID.GrassBlades, Main.rand.NextVector2CircularEdge(6f, 6f), 0, default, 2.0f);
				d.noGravity = true;
			}
			// Lingering smoke / aura
			for (int i = 0; i < 18; i++)
			{
				var d = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), DustID.GreenTorch, Main.rand.NextVector2Circular(3f, 3f), 100, default, 2.4f);
				d.noGravity = true;
				d.fadeIn = 1.2f;
			}
			ShakeCamera(14f, 2000f, 30, "UncommonAnimateDeath");
			return;
		}

		// === REGULAR HIT FEEDBACK ===
		_hitFlash = 1f;
		PulseScale(1.10f);

		int count = 4 + (int)Math.Min(20, hit.Damage / 8);
		for (int i = 0; i < count; i++)
		{
			Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
			var d = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(16f, 16f), DustID.GreenTorch, vel, 0, default, 1.2f);
			d.noGravity = true;
		}
		// Sparks for chunky hits
		if (hit.Damage >= 30)
		{
			for (int i = 0; i < 4; i++)
			{
				var d = Dust.NewDustPerfect(NPC.Center, DustID.GrassBlades, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.4f);
				d.noGravity = true;
			}
			ShakeCamera(2.5f, 900f, 6, "UncommonAnimateHit");
		}
	}
}
