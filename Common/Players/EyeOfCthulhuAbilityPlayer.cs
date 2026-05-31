using ElementalHearts.Content.Buffs;
using ElementalHearts.Content.Projectiles.Minions;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ElementalHearts.Common.Players;

public class EyeOfCthulhuAbilityPlayer : ModPlayer
{
	public bool Enabled { get; set; }

	public override void SaveData(TagCompound tag)
	{
		if (Enabled)
			tag["Enabled"] = true;
	}

	public override void LoadData(TagCompound tag)
	{
		Enabled = tag.ContainsKey("Enabled");
	}

	public override void PostUpdateEquips()
	{
		if (Enabled)
		{
			Player.AddBuff(ModContent.BuffType<ServantOfCthulhuBuff>(), 2);

			int minionType = ModContent.ProjectileType<ServantOfCthulhuMinion>();
			if (Player.ownedProjectileCounts[minionType] < 1)
			{
				int damage = GetMinionDamage();
				// Spawn the minion at the player's center
				Projectile.NewProjectile(Player.GetSource_Accessory(new Item(ModContent.ItemType<Content.Items.Vanilla.Exotic.EyeOfCthulhuHeart>())), Player.Center, Microsoft.Xna.Framework.Vector2.Zero, minionType, damage, 2f, Player.whoAmI);
			}
			else
			{
				// Update damage if it changes due to progression
				int damage = GetMinionDamage();
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];
					if (proj.active && proj.owner == Player.whoAmI && proj.type == minionType)
					{
						proj.damage = damage;
					}
				}
			}
		}
	}

	public int GetMinionDamage()
	{
		int damage = 10; // Base damage

		if (NPC.downedBoss3) // Skeletron
			damage += 10;
		if (Main.hardMode) // Wall of Flesh
			damage += 20;
		if (NPC.downedMechBossAny)
			damage += 20;
		if (NPC.downedPlantBoss)
			damage += 30;
		if (NPC.downedGolemBoss)
			damage += 30;
		if (NPC.downedMoonlord)
			damage += 50;

		return damage;
	}
}
