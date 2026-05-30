using Terraria.ModLoader;
using ElementalHearts.Common.Configs;

namespace ElementalHearts.Common.UI.Checklist;

public class ChecklistCommand : ModCommand
{
	public override CommandType Type => CommandType.Chat;
	public override string Command => "hearts";
	public override string Description => "Toggles the Elemental Hearts Checklist";

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		ChecklistUISystem.ToggleUI();
	}
}
