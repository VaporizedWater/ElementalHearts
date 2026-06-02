using ElementalHearts.Content.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.NPCs;

public class TwinsCloneGlobalNPC : GlobalNPC
{
	public override void AI(NPC npc)
	{
		if (npc.boss || npc.friendly || npc.damage <= 0) return;
		
		int cloneType = ModContent.ProjectileType<TwinsCloneProjectile>();
		for (int i = 0; i < Main.maxProjectiles; i++)
		{
			Projectile proj = Main.projectile[i];
			if (proj.active && proj.type == cloneType)
			{
				float dist = npc.DistanceSQ(proj.Center);
				if (dist < 1000f * 1000f) // 1000 pixels range
				{
					Microsoft.Xna.Framework.Vector2 dir = proj.Center - npc.Center;
					if (dir.LengthSquared() > 0)
					{
						dir.Normalize();
						npc.velocity.X = (npc.velocity.X * 10f + dir.X * 4f) / 11f;
						if (!npc.noGravity)
						{
							if (npc.collideX && dir.Y < 0) npc.velocity.Y = -6f;
						}
						else
						{
							npc.velocity.Y = (npc.velocity.Y * 10f + dir.Y * 4f) / 11f;
						}
					}
					return;
				}
			}
		}
	}
}
