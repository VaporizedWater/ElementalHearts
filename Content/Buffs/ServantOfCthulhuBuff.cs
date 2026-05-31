using ElementalHearts.Content.Projectiles.Minions;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Content.Buffs;

public class ServantOfCthulhuBuff : ModBuff
{
	public override string Texture => "ElementalHearts/Content/Items/Vanilla/Exotic/EyeOfCthulhuHeart";

	public override void SetStaticDefaults()
	{
		Main.buffNoTimeDisplay[Type] = true;
		Main.buffNoSave[Type] = true;
	}

	public override bool PreDraw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, int buffIndex, ref Terraria.DataStructures.BuffDrawParams drawParams)
	{
		Microsoft.Xna.Framework.Graphics.Texture2D texture = Terraria.GameContent.TextureAssets.Buff[Type].Value;
		
		// The default buff box is 32x32. drawParams.Position is the top-left of that box.
		Microsoft.Xna.Framework.Vector2 center = drawParams.Position + new Microsoft.Xna.Framework.Vector2(16f, 16f);
		
		// Draw the item texture centered and scaled down so it fits nicely inside the buff box
		spriteBatch.Draw(
			texture, 
			center, 
			null, 
			drawParams.DrawColor, 
			0f, 
			texture.Size() / 2f, 
			1.1f, // Increased scale so it fills the buff box better
			Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 
			0f
		);
		
		return false; // Tell tModLoader we handled the drawing
	}

	public override void Update(Player player, ref int buffIndex)
	{
		if (player.ownedProjectileCounts[ModContent.ProjectileType<ServantOfCthulhuMinion>()] > 0)
		{
			player.buffTime[buffIndex] = 18000;
		}
		else
		{
			player.DelBuff(buffIndex);
			buffIndex--;
		}
	}
}
