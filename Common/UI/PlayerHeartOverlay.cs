using System;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Common.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ElementalHearts.Common.UI;

/// <summary>
/// Recolours the hearts in the player's life panel to match the highest heart tier they've
/// consumed in the current world. The ladder interleaves the two vanilla heart states so the
/// bar reads as one continuous progression:
/// <para>Regular → Common → Uncommon → Rare → Life&#160;Fruit → Epic → Legendary → Mythic.</para>
/// "Regular" (red) and "Life Fruit" (gold) are vanilla's own two heart textures, so those
/// rungs draw normally; each mod rung swaps a bespoke sprite in for every heart in the bar.
/// Exotic has no art of its own yet and shares Legendary's heart; Mythic — the radiant summit,
/// reached only via the Zenith heart — gets its own. The highest rung between the player's mod
/// tier and their vanilla life-fruit state wins, so eating a Life Fruit outranks a Rare heart
/// but not an Epic one, exactly as the ladder is ordered.
/// </summary>
public sealed class PlayerHeartOverlay : ModResourceOverlay
{
	private Asset<Texture2D> _common, _uncommon, _rare, _epic, _legendary, _mythic;

	public override void Load()
	{
		if (Main.dedServ)
			return;

		_common = Request("CommonPlayerHeart");
		_uncommon = Request("UncommonPlayerHeart");
		_rare = Request("RarePlayerHeart");
		_epic = Request("EpicPlayerHeart");
		_legendary = Request("LegendaryPlayerHeart");
		_mythic = Request("MythicPlayerHeart");
	}

	private static Asset<Texture2D> Request(string name) =>
		ModContent.Request<Texture2D>($"ElementalHearts/Assets/Textures/PlayerHearts/{name}", AssetRequestMode.ImmediateLoad);

	public override bool PreDrawResource(ResourceOverlayDrawContext context)
	{
		// Only the life-heart fills are themed. Mana stars, the horizontal bars, and the heart
		// frame panels all use other textures, so they fall through to vanilla untouched.
		if (!IsHeartFill(context.texture))
			return true;

		Asset<Texture2D>? sprite = ResolveSprite();
		if (sprite == null)
			return true; // Regular or Life Fruit rung — let vanilla draw its own heart.

		// The context was authored for the vanilla heart texture: its source rectangle can be a
		// partial vertical slice (the Fancy bar clips the fill to show a half-full heart), and its
		// origin/scale are in that texture's pixels. Remap everything from vanilla-texture space
		// into the replacement's space so a differently-sized sprite lands on the exact same
		// on-screen rectangle, clips identically, and keeps the low-health "beat" in context.scale.
		Texture2D vanilla = context.texture.Value;
		Texture2D mine = sprite.Value;
		float rx = mine.Width / (float)vanilla.Width;
		float ry = mine.Height / (float)vanilla.Height;

		Rectangle s = context.source ?? new Rectangle(0, 0, vanilla.Width, vanilla.Height);
		context.texture = sprite;
		context.source = new Rectangle(
			(int)Math.Round(s.X * rx), (int)Math.Round(s.Y * ry),
			(int)Math.Round(s.Width * rx), (int)Math.Round(s.Height * ry));
		context.origin = new Vector2(context.origin.X * rx, context.origin.Y * ry);
		context.scale = new Vector2(context.scale.X / rx, context.scale.Y / ry);
		context.Draw();
		return false;
	}

	/// <summary>
	/// Whether this draw is a life-heart fill (the coloured interior), across every vanilla
	/// life display. The Classic set draws the canonical <see cref="TextureAssets.Heart"/> /
	/// <see cref="TextureAssets.Heart2"/>; the Fancy set (the 1.4 default) draws its own
	/// "Heart_Fill" / "Heart_Fill_B" textures — matched by name so the frame panels
	/// ("Heart_Left/Middle/Right/...") and mana/bars are left alone.
	/// </summary>
	private static bool IsHeartFill(Asset<Texture2D> texture)
	{
		if (texture == null)
			return false;
		if (texture == TextureAssets.Heart || texture == TextureAssets.Heart2)
			return true;
		string name = texture.Name;
		return name != null && name.Contains("Heart_Fill");
	}

	/// <summary>
	/// The replacement sprite for the local player's current rung, or null when that rung is
	/// one of the two vanilla states (Regular / Life Fruit) that need no override.
	/// </summary>
	private Asset<Texture2D>? ResolveSprite()
	{
		Player player = Main.LocalPlayer;
		if (player == null)
			return null;

		int modRung = player.GetModPlayer<HeartConsumptionPlayer>().HighestTier switch
		{
			HeartTier.Common => 1,
			HeartTier.Uncommon => 2,
			HeartTier.Rare => 3,
			HeartTier.Epic => 5,
			HeartTier.Legendary => 6,
			// Exotic has no bespoke art yet, so it shares Legendary's orange heart.
			HeartTier.Exotic => 6,
			// Mythic is the summit — the radiant heart, only reachable via the Zenith heart.
			HeartTier.Mythic => 7,
			_ => 0,
		};

		// Eating a Life Fruit (vanilla's gold hearts) sits between Rare and Epic on the ladder.
		int vanillaRung = player.ConsumedLifeFruit > 0 ? 4 : 0;

		return (modRung > vanillaRung ? modRung : vanillaRung) switch
		{
			1 => _common,
			2 => _uncommon,
			3 => _rare,
			5 => _epic,
			6 => _legendary,
			7 => _mythic,
			_ => null, // 0 (Regular) or 4 (Life Fruit): vanilla draws it.
		};
	}
}
