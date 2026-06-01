using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace ElementalHearts.Common.UI;

public class UIToggleSwitch : UIElement
{
    private static Texture2D _circleTexture;
    public bool IsOn;
    public event Action<bool> OnStateChanged;

    /// <summary>0 = off, 1 = on. Eased toward <see cref="IsOn"/> each frame so the thumb glides
    /// and the track cross-fades instead of snapping — a flipped switch should feel physical.</summary>
    private float _slide;

    public UIToggleSwitch(bool initialState)
    {
        IsOn = initialState;
        _slide = initialState ? 1f : 0f;
        Width.Set(44, 0f);
        Height.Set(24, 0f);

        OnMouseOver += (evt, element) => Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);

        OnLeftClick += (evt, element) => {
            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuTick);
            IsOn = !IsOn;
            OnStateChanged?.Invoke(IsOn);
        };
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // Glide toward the target state; 0.25 lerp settles in a few frames without feeling laggy.
        _slide = MathHelper.Lerp(_slide, IsOn ? 1f : 0f, 0.25f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (_circleTexture == null)
            GenerateCircleTexture();

        CalculatedStyle dim = GetDimensions();
        Vector2 pos = dim.Position();

        // Track cross-fades grey→teal as the switch slides on, so colour and motion agree.
        Color trackColor = Color.Lerp(new Color(210, 210, 210), new Color(25, 160, 150), _slide);

        // Scale 24px height from 64px base texture
        float scale = 24f / 64f;

        // Left circle
        spriteBatch.Draw(_circleTexture, pos, null, trackColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        // Right circle
        spriteBatch.Draw(_circleTexture, pos + new Vector2(20, 0), null, trackColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        // Middle rect
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)pos.X + 12, (int)pos.Y, 20, 24), trackColor);

        // Thumb circle — glides between the off (2px) and on (22px) rest positions.
        float thumbScale = 20f / 64f;
        float thumbX = MathHelper.Lerp(2f, 22f, _slide);
        spriteBatch.Draw(_circleTexture, pos + new Vector2(thumbX, 2f), null, Color.White, 0f, Vector2.Zero, thumbScale, SpriteEffects.None, 0f);
    }

    private static void GenerateCircleTexture()
    {
        int size = 64;
        _circleTexture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
        Color[] data = new Color[size * size];
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = MathHelper.Clamp(radius - dist + 0.5f, 0f, 1f);
                data[y * size + x] = Color.White * alpha;
            }
        }
        _circleTexture.SetData(data);
    }
}
