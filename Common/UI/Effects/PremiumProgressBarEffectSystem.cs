using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.UI.Effects
{
    public class PremiumProgressBarEffectSystem : ModSystem
    {
        private static Effect _progressBarEffect;
        private static bool _effectLoaded = false;

        public override void Unload()
        {
            _progressBarEffect = null;
            _effectLoaded = false;
        }

        private static void LoadEffect()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server) return;
            if (_progressBarEffect != null)
            {
                _effectLoaded = true;
                return;
            }

            try
            {
                byte[] shaderBytes = ModContent.GetInstance<ElementalHearts>().GetFileBytes("Assets/Effects/ProgressBar.fxc");
                if (shaderBytes != null && shaderBytes.Length > 0)
                {
                    _progressBarEffect = new Effect(Main.graphics.GraphicsDevice, shaderBytes);
                    _effectLoaded = true;
                }
            }
            catch
            {
                _effectLoaded = false;
            }
        }

        public static void Draw(SpriteBatch spriteBatch, Rectangle bounds, float fillPercent, float borderThickness, Color backgroundColor, Color borderColor, Color fillColor1, Color fillColor2, Color pulseColor, bool isCapped)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            if (!_effectLoaded && _progressBarEffect == null)
            {
                LoadEffect();
            }

            if (!_effectLoaded || _progressBarEffect == null)
            {
                // Fallback: draw a basic flat rectangle
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, bounds, fillColor1 * fillPercent);
                return;
            }

            float cornerRadius = bounds.Height / 2f;

            // Set shader parameters
            _progressBarEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _progressBarEffect.Parameters["uResolution"]?.SetValue(new Vector2(bounds.Width, bounds.Height));
            _progressBarEffect.Parameters["uBorderThickness"]?.SetValue(borderThickness);
            _progressBarEffect.Parameters["uBorderRadius"]?.SetValue(cornerRadius);
            _progressBarEffect.Parameters["uFillPercent"]?.SetValue(fillPercent);
            _progressBarEffect.Parameters["uBackgroundColor"]?.SetValue(backgroundColor.ToVector4());
            _progressBarEffect.Parameters["uBorderColor"]?.SetValue(borderColor.ToVector4());
            _progressBarEffect.Parameters["uFillColor1"]?.SetValue(fillColor1.ToVector4());
            _progressBarEffect.Parameters["uFillColor2"]?.SetValue(fillColor2.ToVector4());
            _progressBarEffect.Parameters["uPulseColor"]?.SetValue(pulseColor.ToVector4());
            _progressBarEffect.Parameters["uIsCapped"]?.SetValue(isCapped ? 1f : 0f);

            RasterizerState rasterizer = spriteBatch.GraphicsDevice.RasterizerState;

            // Start immediate mode for custom shader
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizer, _progressBarEffect, Main.UIScaleMatrix);

            // Draw a completely opaque blank texture, letting the shader determine the pixels natively
            spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, bounds, Color.White * 1.0f);

            // Restore default spritebatch mode
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizer, null, Main.UIScaleMatrix);
        }
    }
}
