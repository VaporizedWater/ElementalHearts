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
        public bool IsInteractive { get; set; } = false;
        
        // Colors for the aurora effect
        public Color AuroraColor1 { get; set; } = new Color(50, 0, 100);
        public Color AuroraColor2 { get; set; } = new Color(0, 50, 150);
        public Color AuroraColor3 { get; set; } = new Color(150, 0, 100);
        public float BorderRadius { get; set; } = 12f;

        public Color[] AuroraColors { get; set; } = null;
        public bool IsPrismatic { get; set; } = false;
        public float AuroraBrightness { get; set; } = 1f;

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
            if (IsInteractive && IsMouseHovering)
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
            if (IsInteractive)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
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
                Color c1 = AuroraColor1;
                Color c2 = AuroraColor2;
                Color c3 = AuroraColor3;

                if (IsPrismatic)
                {
                    float time = Main.GlobalTimeWrappedHourly * 0.5f;
                    c1 = Main.hslToRgb(time % 1f, 1f, 0.5f);
                    c2 = Main.hslToRgb((time + 0.33f) % 1f, 1f, 0.5f);
                    c3 = Main.hslToRgb((time + 0.66f) % 1f, 1f, 0.5f);
                }
                else if (AuroraColors != null && AuroraColors.Length > 0)
                {
                    if (AuroraColors.Length == 1)
                    {
                        c1 = AuroraColors[0];
                        c2 = new Color((int)(c1.R * 0.6f), (int)(c1.G * 0.6f), (int)(c1.B * 0.6f));
                        c3 = new Color((int)Math.Min(255, c1.R * 1.4f), (int)Math.Min(255, c1.G * 1.4f), (int)Math.Min(255, c1.B * 1.4f));
                    }
                    else if (AuroraColors.Length == 2)
                    {
                        c1 = AuroraColors[0];
                        c2 = Color.Lerp(AuroraColors[0], AuroraColors[1], 0.5f);
                        c3 = AuroraColors[1];
                    }
                    else if (AuroraColors.Length == 3)
                    {
                        c1 = AuroraColors[0];
                        c2 = AuroraColors[1];
                        c3 = AuroraColors[2];
                    }
                    else
                    {
                        // Array of N > 3 colors. We dynamically blend 3 moving points.
                        float t = Main.GlobalTimeWrappedHourly * 0.3f;
                        int n = AuroraColors.Length;
                        
                        Color SampleArray(float pos)
                        {
                            pos = pos % n;
                            if (pos < 0) pos += n;
                            int idx1 = (int)pos;
                            int idx2 = (idx1 + 1) % n;
                            float blend = pos - idx1;
                            return Color.Lerp(AuroraColors[idx1], AuroraColors[idx2], blend);
                        }

                        c1 = SampleArray(t);
                        c2 = SampleArray(t + (n / 3f));
                        c3 = SampleArray(t + (n * 2f / 3f));
                    }
                }

                c1 *= AuroraBrightness * 0.6f; // Increased intensity to 0.6f (20% stronger than original 0.5f)
                c2 *= AuroraBrightness * 0.6f;
                c3 *= AuroraBrightness * 0.6f;

                // Set shader parameters
                _cachedAuroraEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                _cachedAuroraEffect.Parameters["uResolution"]?.SetValue(new Vector2(scaledWidth, scaledHeight));
                _cachedAuroraEffect.Parameters["uHoverGlow"]?.SetValue(_hoverIntensity);
                _cachedAuroraEffect.Parameters["uBorderRadius"]?.SetValue(BorderRadius);
                _cachedAuroraEffect.Parameters["uColor1"]?.SetValue(c1.ToVector4());
                _cachedAuroraEffect.Parameters["uColor2"]?.SetValue(c2.ToVector4());
                _cachedAuroraEffect.Parameters["uColor3"]?.SetValue(c3.ToVector4());

                RasterizerState rasterizer = spriteBatch.GraphicsDevice.RasterizerState;

                // Start immediate mode for custom shader
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizer, _cachedAuroraEffect, Main.UIScaleMatrix);

                // Draw a blank white texture the size of the panel to let the pixel shader work
                // Opacity set to 1.0f (Opaque) as requested
                spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, drawRect, Color.White * 1.0f);

                // Restore default spritebatch mode
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizer, null, Main.UIScaleMatrix);
            }
        }
    }
}
