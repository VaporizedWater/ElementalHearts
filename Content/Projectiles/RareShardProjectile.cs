using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ElementalHearts.Content.Projectiles;

public sealed class RareShardProjectile : SmallBossShardProjectile
{
	public override string Texture => "ElementalHearts/Content/Projectiles/RareSmallBossProjectile";

	protected override Vector3 LightColor => new(0.2f, 0.45f, 0.95f);

	protected override int TrailDust => DustID.IceTorch;
}
