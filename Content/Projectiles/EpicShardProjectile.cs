using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ElementalHearts.Content.Projectiles;

public sealed class EpicShardProjectile : SmallBossShardProjectile
{
	public override string Texture => "ElementalHearts/Content/Projectiles/EpicSmallBossProjectile";

	protected override Vector3 LightColor => new(0.6f, 0.2f, 0.8f);

	protected override int TrailDust => DustID.PurpleTorch;
}
