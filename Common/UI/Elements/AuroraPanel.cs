using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ElementalHearts.Common.UI.Elements
{
    public class AuroraPanel : UIPanel
    {
        private float _hoverIntensity = 0f;
        private float _scale = 0f;
        private bool _hasInitialized = false;

        public string TooltipText { get; set; } = "";
        
        // Colors for the aurora effect
        public Color AuroraColor1 { get; set; } = new Color(50, 0, 100);
        public Color AuroraColor2 { get; set; } = new Color(0, 50, 150);
        public Color AuroraColor3 { get; set; } = new Color(150, 0, 100);
        public float BorderRadius { get; set; } = 12f;

        public override void OnInitialize()
        {
            base.OnInitialize();
            _hasInitialized = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Smooth scale-in animation when first appearing
            if (_scale < 1f)
            {
                _scale = MathHelper.Lerp(_scale, 1f, 0.15f);
                if (Math.Abs(_scale - 1f) < 0.01f)
                    _scale = 1f;
            }

            // Hover glow interpolation
            if (IsMouseHovering)
            {
                _hoverIntensity = MathHelper.Lerp(_hoverIntensity, 1f, 0.1f);
                
                if (!string.IsNullOrEmpty(TooltipText))
                {
                    Main.instance.MouseText(TooltipText);
                }
            }
            else
            {
                _hoverIntensity = MathHelper.Lerp(_hoverIntensity, 0f, 0.1f);
            }
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        private static Effect _cachedAuroraEffect;

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            
            float width = dimensions.Width;
            float height = dimensions.Height;
            float scaledWidth = width * _scale;
            float scaledHeight = height * _scale;
            float x = dimensions.X + (width - scaledWidth) / 2f;
            float y = dimensions.Y + (height - scaledHeight) / 2f;
            
            Rectangle drawRect = new Rectangle((int)x, (int)y, (int)scaledWidth, (int)scaledHeight);

            // Fetch the compiled shader directly from raw bytes
            if (_cachedAuroraEffect == null)
            {
                try
                {
                    byte[] shaderBytes = ModContent.GetInstance<ElementalHearts>().GetFileBytes("Assets/Effects/AuroraGradient.fxc");
                    if (shaderBytes != null && shaderBytes.Length > 0)
                    {
                        _cachedAuroraEffect = new Effect(Main.graphics.GraphicsDevice, shaderBytes);
                    }
                }
                catch
                {
                    // Fallback or ignore if not found
                }
            }

            if (_cachedAuroraEffect != null)
            {
                // Set shader parameters
                _cachedAuroraEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                _cachedAuroraEffect.Parameters["uResolution"]?.SetValue(new Vector2(scaledWidth, scaledHeight));
                _cachedAuroraEffect.Parameters["uHoverGlow"]?.SetValue(_hoverIntensity);
                _cachedAuroraEffect.Parameters["uBorderRadius"]?.SetValue(BorderRadius);
                _cachedAuroraEffect.Parameters["uColor1"]?.SetValue(AuroraColor1.ToVector4());
                _cachedAuroraEffect.Parameters["uColor2"]?.SetValue(AuroraColor2.ToVector4());
                _cachedAuroraEffect.Parameters["uColor3"]?.SetValue(AuroraColor3.ToVector4());

                // Start immediate mode for custom shader
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, _cachedAuroraEffect, Main.UIScaleMatrix);

                // Draw a blank white texture the size of the panel to let the pixel shader work
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, drawRect, Color.White * 0.8f);

                // Restore default spritebatch mode
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            }
        }
    }
}
