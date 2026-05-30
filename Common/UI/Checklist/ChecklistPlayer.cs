using Terraria.GameInput;
using Terraria.ModLoader;

namespace ElementalHearts.Common.UI.Checklist;

public class ChecklistPlayer : ModPlayer
{
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (ChecklistUISystem.ToggleChecklistKeybind.JustPressed)
		{
			ChecklistUISystem.ToggleUI();
		}
	}
}
