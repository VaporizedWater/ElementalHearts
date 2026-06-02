using ElementalHearts.Content.Projectiles.Minions;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Players;

public class DestroyerAbilityPlayer : ToggleAbilityPlayer
{
	public override void PostUpdateEquips()
	{
		if (Enabled && !Player.GetModPlayer<EyeOfCthulhuAbilityPlayer>().Enabled)
		{
			Enabled = false;
		}

		if (Enabled)
		{
			int minionType = ModContent.ProjectileType<DestroyerProbeMinion>();
			if (Player.ownedProjectileCounts[minionType] < 1)
			{
				Projectile.NewProjectile(Player.GetSource_Accessory(new Item(ModContent.ItemType<Content.Items.Vanilla.Exotic.TheDestroyerHeart>())), Player.Center, Microsoft.Xna.Framework.Vector2.Zero, minionType, 0, 0f, Player.whoAmI);
			}
		}
	}
}
