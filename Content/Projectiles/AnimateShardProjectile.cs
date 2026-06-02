using Microsoft.Xna.Framework;
using Terraria.ID;

namespace ElementalHearts.Content.Projectiles;

public sealed class AnimateShardProjectile : SmallBossShardProjectile
{
	public override string Texture => "ElementalHearts/Content/Projectiles/CommonSmallBossProjectile";

	protected override Vector3 LightColor => new(0.8f, 0.2f, 0.5f);

	protected override int TrailDust => DustID.PinkCrystalShard;

	// 25% larger for better visibility.
	protected override int HitboxSize => 20;
	protected override float DrawScale => 1.25f;
}
