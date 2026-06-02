using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.UI.Effects
{
    public class PremiumProgressBarEffectSystem : ModSystem
    {
        private static Texture2D _texture;
        private static Color[] _colorData;
        private static int _lastWidth = -1;
        private static int _lastHeight = -1;

        public override void Unload()
        {
            if (_texture != null && !_texture.IsDisposed)
            {
                var tex = _texture;
                Main.QueueMainThreadAction(() =>
                {
                    if (!tex.IsDisposed)
                    {
                        tex.Dispose();
                    }
                });
            }
            _texture = null;
            _colorData = null;
        }

        public static void Draw(SpriteBatch spriteBatch, Rectangle bounds, float fillPercent, float borderThickness, Color backgroundColor, Color borderColor, Color fillColor1, Color fillColor2, Color pulseColor, bool isCapped)
        {
            int w = bounds.Width;
            int h = bounds.Height;

            if (w <= 0 || h <= 0) return;

            if (_texture == null || _lastWidth != w || _lastHeight != h || _texture.IsDisposed)
            {
                if (_texture != null && !_texture.IsDisposed) _texture.Dispose();
                _texture = new Texture2D(Main.graphics.GraphicsDevice, w, h);
                _colorData = new Color[w * h];
                _lastWidth = w;
                _lastHeight = h;
            }

            float cornerRadius = h / 2f;
            float bX = (w / 2f) - cornerRadius;
            float fillX = w * fillPercent;
            
            float glowWidth = 30f;
            float time = Main.GlobalTimeWrappedHourly;
            float glowPos = (float)((time * 150f) % (w + glowWidth)) - glowWidth;
            float pulse = (float)(Math.Sin(time * 4f) + 1f) / 2f;

            for (int y = 0; y < h; y++)
            {
                float pY = y - h / 2f;
                float dY = Math.Max(Math.Abs(pY), 0f);
                float dY2 = dY * dY;

                for (int x = 0; x < w; x++)
                {
                    float pX = x - w / 2f;
                    float dX = Math.Max(Math.Abs(pX) - bX, 0f);
                    
                    float length = (float)Math.Sqrt(dX * dX + dY2);
                    float distOuter = length + Math.Min(Math.Max(Math.Abs(pX) - bX, Math.Abs(pY)), 0f) - cornerRadius;
                    float alphaOuter = 1f - MathHelper.Clamp(distOuter + 0.5f, 0f, 1f);

                    if (alphaOuter <= 0f)
                    {
                        _colorData[y * w + x] = Color.Transparent;
                        continue;
                    }

                    float distInner = distOuter + borderThickness;
                    float alphaInner = 1f - MathHelper.Clamp(distInner + 0.5f, 0f, 1f);

                    float alphaFill = 1f - MathHelper.Clamp(x - fillX + 0.5f, 0f, 1f);

                    Color currentFill = Color.Lerp(fillColor1, fillColor2, (float)x / w);
                    if (!isCapped)
                    {
                        float glowFactor = 1f - MathHelper.Clamp(Math.Abs(x - glowPos) / glowWidth, 0f, 1f);
                        currentFill = Color.Lerp(currentFill, Color.White, glowFactor * 0.5f);
                    }
                    else
                    {
                        currentFill = Color.Lerp(pulseColor, Color.White, pulse * 0.5f);
                    }

                    Color innerColor = Color.Lerp(backgroundColor, currentFill, alphaFill);
                    Color rgb = Color.Lerp(borderColor, innerColor, alphaInner);

                    _colorData[y * w + x] = rgb * alphaOuter;
                }
            }

            _texture.SetData(_colorData);
            spriteBatch.Draw(_texture, bounds.Location.ToVector2(), Color.White);
        }
    }
}
