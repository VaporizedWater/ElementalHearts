using ElementalHearts.Content.Buffs;
using ElementalHearts.Content.Projectiles.Minions;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.NPCs;

public class DestroyerMinionGlobalNPC : GlobalNPC
{
	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		if (projectile.type == ModContent.ProjectileType<ServantOfCthulhuMinion>() && npc.HasBuff(ModContent.BuffType<DestroyerTargetDebuff>()))
		{
			modifiers.FinalDamage *= 2f;
		}
	}

	public override void DrawEffects(NPC npc, ref Microsoft.Xna.Framework.Color drawColor)
	{
		if (npc.HasBuff(ModContent.BuffType<DestroyerTargetDebuff>()))
		{
			// Red discoloration
			drawColor.R = 255;
			drawColor.G = (byte)System.Math.Max(0, drawColor.G - 50);
			drawColor.B = (byte)System.Math.Max(0, drawColor.B - 50);

			if (Main.rand.NextBool(5))
			{
				Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, Terraria.ID.DustID.RedTorch, 0f, -2f, 100, default, 1f);
				d.noGravity = true;
			}
		}
	}
}
