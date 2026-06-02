using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ElementalHearts.Content.Projectiles;

public sealed class LegendaryShardProjectile : SmallBossShardProjectile
{
	public override string Texture => "ElementalHearts/Content/Projectiles/LegendarySmallBossProjectile";

	protected override Vector3 LightColor => new(0.8f, 0.7f, 0.2f);

	protected override int TrailDust => DustID.YellowTorch;
}
