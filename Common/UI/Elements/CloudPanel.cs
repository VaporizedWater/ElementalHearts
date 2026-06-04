using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ElementalHearts.Common.UI.Elements
{
    public class CloudPanel : UIPanel
    {
        private static Effect _cachedCloudEffect;
        private bool _effectLoaded = false;
        public float BorderRadius { get; set; } = 12f;
        public float CloudDensity { get; set; } = 0.6f;

        public override void OnInitialize()
        {
            base.OnInitialize();
            LoadEffect();
        }

        private void LoadEffect()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server) return;
            if (_cachedCloudEffect != null)
            {
                _effectLoaded = true;
                return;
            }

            try
            {
                byte[] shaderBytes = ModContent.GetInstance<ElementalHearts>().GetFileBytes("Assets/Effects/CloudBackground.fxc");
                if (shaderBytes != null && shaderBytes.Length > 0)
                {
                    _cachedCloudEffect = new Effect(Main.graphics.GraphicsDevice, shaderBytes);
                    _effectLoaded = true;
                }
            }
            catch
            {
                _effectLoaded = false;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle dimensions = GetDimensions().ToRectangle();
            
            // Re-attempt load if not present
            if (!_effectLoaded && _cachedCloudEffect == null)
            {
                LoadEffect();
            }

            if (!_effectLoaded || _cachedCloudEffect == null)
            {
                // Fallback to default UIPanel rendering
                base.DrawSelf(spriteBatch);
                return;
            }

            Rectangle drawRect = new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height);

            float scaledWidth = drawRect.Width;
            float scaledHeight = drawRect.Height;

            // Fetch current sky colors natively from Terraria!
            Color skyColor = Main.ColorOfTheSkies;
            
            // Adjust the sky color slightly to look good as a UI background (darken slightly)
            skyColor = new Color((int)(skyColor.R * 0.7f), (int)(skyColor.G * 0.7f), (int)(skyColor.B * 0.7f));
            
            // Determine cloud color. White during day, dark gray at night.
            Color cloudColor = Main.dayTime ? new Color(255, 255, 255, 200) : new Color(50, 50, 70, 150);

            // Set shader parameters
            _cachedCloudEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _cachedCloudEffect.Parameters["uResolution"]?.SetValue(new Vector2(scaledWidth, scaledHeight));
            _cachedCloudEffect.Parameters["uBorderRadius"]?.SetValue(BorderRadius);
            _cachedCloudEffect.Parameters["uSkyColor"]?.SetValue(skyColor.ToVector4());
            _cachedCloudEffect.Parameters["uCloudColor"]?.SetValue(cloudColor.ToVector4());
            _cachedCloudEffect.Parameters["uCloudDensity"]?.SetValue(CloudDensity);

            RasterizerState rasterizer = spriteBatch.GraphicsDevice.RasterizerState;

            // Start immediate mode for custom shader
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizer, _cachedCloudEffect, Main.UIScaleMatrix);

            // Draw a completely opaque blank texture, letting the shader determine the alpha natively
            spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, drawRect, Color.White * 1.0f);

            // Restore default spritebatch mode
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizer, null, Main.UIScaleMatrix);
        }
    }
}
