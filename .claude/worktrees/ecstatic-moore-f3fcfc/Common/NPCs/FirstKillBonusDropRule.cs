using ElementalHearts.Common.Systems;
using Terraria.GameContent.ItemDropRules;

namespace ElementalHearts.Common.NPCs;

/// <summary>
/// Condition that checks if a boss is being killed for the first time.
/// </summary>
public sealed class FirstKillCondition : IItemDropRuleCondition, IProvideItemConditionDescription
{
	private readonly int _npcType;
	private readonly bool _isFirstKill;

	public FirstKillCondition(int npcType, bool isFirstKill)
	{
		_npcType = npcType;
		_isFirstKill = isFirstKill;
	}

	public bool CanDrop(DropAttemptInfo info) => _isFirstKill == BossFirstKillWorld.IsFirstKill(_npcType);

	public bool CanShowItemDropInUI() => true;

	public string GetConditionDescription() => _isFirstKill ? "First kill of the boss" : "Boss defeated before";
}
