using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using ElementalHearts.Content.Items.Hearts;
using ElementalHearts.Common.Configs;
using ElementalHearts.Common.Systems;
using ElementalHearts.Common.Players;
using ElementalHearts.Common.Hearts;
using ElementalHearts.Content.Items.LifeShards;

namespace ElementalHearts.Common.UI.Checklist;

public class MilestonesUI
{
	public static void RebuildMilestonesList(UIList list, System.Action rebuildAction)
	{
		list.Clear();



		var player = Main.LocalPlayer.GetModPlayer<HeartConsumptionPlayer>();
		bool shared = ElementalHeartsServerConfig.Instance.WorldGen.SharedProgression;
		int totalConsumed = shared ? HeartConsumptionWorld.Consumed.Count : player.WorldHpApplied.Count;
		int elementalHp = player.ActiveHpBonus;

		int totalBossHearts = 0;
		bool hasMythic = false;

		var allHearts = ModContent.GetContent<ElementalHeartItem>().ToList();
		foreach (var heart in allHearts)
		{
			bool isConsumed = shared ? HeartConsumptionWorld.IsConsumed(heart.ConsumptionId) : player.IsConsumedLocally(heart.ConsumptionId);
			if (isConsumed)
			{
				if (heart is BossHeartItem) totalBossHearts++;
				if (heart.Tier == HeartTier.Mythic) hasMythic = true;
			}
		}
		
		int abilitiesActive = 0;
		int totalAbilitiesUnlocked = 0;
		foreach (var heart in allHearts)
		{
			bool isUnlocked = shared ? HeartConsumptionWorld.IsUnlocked(heart.ConsumptionId) : player.IsUnlockedLocally(heart.ConsumptionId);
			if (heart.IsActiveAbility && isUnlocked)
			{
				totalAbilitiesUnlocked++;
				if (heart.IsAbilityEnabled) abilitiesActive++;
			}
		}

		AddMilestone(list, "m_first", "First Taste of Power", "Consume your very first Elemental Heart.", totalConsumed >= 1, $"{System.Math.Min(totalConsumed, 1)} / 1", 1, player, rebuildAction);
		AddMilestone(list, "m_health", "A Healthy Start", "Reach +20 Elemental HP.", elementalHp >= 20, $"+{System.Math.Min(elementalHp, 20)} / 20 HP", 2, player, rebuildAction);
		AddMilestone(list, "m_collect_25", "Heart Collector", "Consume 25 unique Elemental Hearts.", totalConsumed >= 25, $"{System.Math.Min(totalConsumed, 25)} / 25", 5, player, rebuildAction);
		AddMilestone(list, "m_collect_50", "Heart Hoarder", "Consume 50 unique Elemental Hearts.", totalConsumed >= 50, $"{System.Math.Min(totalConsumed, 50)} / 50", 10, player, rebuildAction);
		AddMilestone(list, "m_boss_10", "Boss Conqueror", "Consume 10 different Boss Hearts.", totalBossHearts >= 10, $"{System.Math.Min(totalBossHearts, 10)} / 10", 15, player, rebuildAction);
		AddMilestone(list, "m_mastery", "Elemental Mastery", "Unlock 5 active abilities.", totalAbilitiesUnlocked >= 5, $"{System.Math.Min(totalAbilitiesUnlocked, 5)} / 5", 20, player, rebuildAction);
		AddMilestone(list, "m_mythic", "The Ultimate Power", "Consume a Mythic tier heart.", hasMythic, hasMythic ? "Complete!" : "Incomplete", 25, player, rebuildAction);
	}

	private static void AddMilestone(UIList list, string id, string name, string description, bool isComplete, string progressText, int rewardGoldCoins, HeartConsumptionPlayer player, System.Action rebuildAction)
	{
		Common.UI.Elements.AuroraPanel panel = new Common.UI.Elements.AuroraPanel();
		panel.Width.Set(0, 1f);
		panel.Height.Set(60, 0f);
		panel.IsInteractive = false;
		
		if (isComplete)
		{
			panel.AuroraColor1 = new Color(20, 50, 20);
			panel.AuroraColor2 = new Color(15, 35, 15);
			panel.AuroraColor3 = new Color(25, 60, 25);
			panel.BorderColor = new Color(40, 90, 40); // Softened green border
		}
		else
		{
			panel.AuroraColor1 = new Color(15, 20, 35);
			panel.AuroraColor2 = new Color(20, 26, 48);
			panel.AuroraColor3 = new Color(10, 15, 25);
			panel.BorderColor = new Color(89, 116, 213);
		}

		UIText nameText = new UIText(name, 1.1f);
		nameText.Top.Set(2, 0f);
		nameText.Left.Set(10, 0f);
		nameText.TextColor = isComplete ? Color.Gold : Color.White;
		panel.Append(nameText);

		UIText descText = new UIText(description, 0.85f);
		descText.Top.Set(28, 0f);
		descText.Left.Set(10, 0f);
		descText.TextColor = isComplete ? new Color(200, 200, 200) : new Color(150, 150, 150);
		panel.Append(descText);

		bool isClaimed = player.IsMilestoneClaimedLocally(id);

		if (isComplete && !isClaimed)
		{
			Common.UI.Elements.UIAnimatedButton claimBtn = new Common.UI.Elements.UIAnimatedButton($"Claim {rewardGoldCoins} Gold", 0.85f);
			claimBtn.HAlign = 1f;
			claimBtn.VAlign = 0.5f;
			claimBtn.Left.Set(-15, 0f);
			claimBtn.BaseColor = new Color(150, 110, 20);
			claimBtn.HoverColor = new Color(255, 215, 0);
			claimBtn.BorderColor = Color.White;
			
			claimBtn.OnLeftClick += (evt, element) => {
				Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.CoinPickup, Main.LocalPlayer.Center);
				player.ClaimMilestoneLocally(id);
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("MilestoneReward"), Terraria.ID.ItemID.GoldCoin, rewardGoldCoins);
				rebuildAction();
			};
			panel.Append(claimBtn);
		}
		else
		{
			UIText progText = new UIText(isClaimed ? "Claimed!" : progressText, 0.9f);
			progText.VAlign = 0.5f;
			progText.HAlign = 1f;
			progText.Left.Set(-15, 0f);
			progText.TextColor = isClaimed ? new Color(150, 255, 150) : new Color(200, 200, 200);
			panel.Append(progText);
		}

		list.Add(panel);
	}
}
