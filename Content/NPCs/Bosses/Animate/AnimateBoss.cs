using Microsoft.Xna.Framework;
using ElementalHearts.Common.LifeShards;
using ElementalHearts.Common.Systems;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

public abstract class AnimateBoss : ModNPC
{
	public abstract int ProgressionTier { get; }
	public abstract LifeShardTier Tier { get; }

	public virtual SoundStyle? AmbientEmissionSound => null;
	private SlotId _ambientSoundSlot;

	public override bool PreAI()
	{
		if (AmbientEmissionSound.HasValue)
		{
			if (!SoundEngine.TryGetActiveSound(_ambientSoundSlot, out var activeSound))
			{
				_ambientSoundSlot = SoundEngine.PlaySound(AmbientEmissionSound.Value, NPC.Center, sound =>
				{
					if (!NPC.active) return false;
					sound.Position = NPC.Center;
					return true;
				});
			}
		}
		return true;
	}

	public override void SetStaticDefaults()
	{
		Main.npcFrameCount[NPC.type] = 1;
		NPCID.Sets.BossBestiaryPriority.Add(NPC.type);
	}

	public override void SetDefaults()
	{
		NPC.width = 40;
		NPC.height = 40;
		NPC.damage = 20 * (ProgressionTier + 1);
		NPC.defense = 5 * ProgressionTier;
		NPC.lifeMax = 1000 * (ProgressionTier + 1);
		NPC.HitSound = AnimateBossSounds.BossHit;
		NPC.DeathSound = SoundID.NPCDeath1;
		NPC.knockBackResist = 0f;
		NPC.aiStyle = -1; // Custom AI
		NPC.boss = true;
		NPC.npcSlots = 10f;
		NPC.value = Item.buyPrice(gold: ProgressionTier + 1);
		
		if (!Main.dedServ)
		{
			Music = MusicID.Boss1;
		}
	}

	public override void AI()
	{
		// Basic placeholder flying AI: pick the closest player each frame and ease toward a
		// point just above them.
		NPC.TargetClosest();
		Player player = Main.player[NPC.target];
		if (player.dead)
		{
			NPC.velocity.Y -= 0.04f;
			NPC.EncourageDespawn(10);
			return;
		}

		Vector2 toTarget = (player.Center - new Vector2(0f, 100f)) - NPC.Center;
		if (toTarget.Length() > 50f)
		{
			Vector2 desired = Vector2.Normalize(toTarget) * (2f + (ProgressionTier * 0.5f));
			NPC.velocity = ((NPC.velocity * 40f) + desired) / 41f;
		}

		NPC.rotation = NPC.velocity.X * 0.05f;
	}

	public override bool CanHitPlayer(Player target, ref int cooldownSlot)
	{
		Rectangle customHitbox = NPC.Hitbox;
		customHitbox.Inflate(-customHitbox.Width / 4, -customHitbox.Height / 4);
		if (!customHitbox.Intersects(target.Hitbox)) return false;

		return true;
	}

	public override void OnKill()
	{
		AnimateProgressionSystem.UnlockTier(ProgressionTier + 1);
	}

	public override void ModifyNPCLoot(NPCLoot npcLoot)
	{
		npcLoot.Add(Terraria.GameContent.ItemDropRules.ItemDropRule.Common(ItemID.LifeCrystal, 1));
	}
}
