using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;

namespace ElementalHearts.Common.Systems;

public class ShockwaveScreenShaderData : ScreenShaderData
{
	private float _radius;
	private float _maxRadius;
	private float _expansionSpeed;
	private float _baseIntensity;

	public ShockwaveScreenShaderData(ReLogic.Content.Asset<Effect> shader, string passName) : base(shader, passName) { }

	public void StartShockwave(Vector2 position, float maxRadius, float expansionSpeed, float intensity)
	{
		UseTargetPosition(position);
		_radius = 0f;
		_maxRadius = maxRadius;
		_expansionSpeed = expansionSpeed;
		_baseIntensity = intensity;
		UseIntensity(intensity);
		UseProgress(0f);
	}

	public override void Update(GameTime gameTime)
	{
		_radius += _expansionSpeed;
		UseProgress(_radius);

		float progressNormalized = MathHelper.Clamp(_radius / _maxRadius, 0f, 1f);
		UseIntensity(_baseIntensity * (1f - progressNormalized));

		if (_radius >= _maxRadius)
		{
			Terraria.Graphics.Effects.Filters.Scene.Deactivate("ElementalHearts:Shockwave");
		}

		base.Update(gameTime);
	}

	public override void Apply()
	{
		// Pass the screen position and resolution to our shader
		Shader.Parameters["uScreenPosition"]?.SetValue(Main.screenPosition);
		Shader.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
		
		base.Apply();
	}
}
