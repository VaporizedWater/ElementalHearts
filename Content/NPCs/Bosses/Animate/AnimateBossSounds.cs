using Terraria.Audio;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

/// <summary>
/// Shared custom sound effects used across the Animate boss family.
/// </summary>
public static class AnimateBossSounds
{
	public static readonly SoundStyle BossHit = new("ElementalHearts/Assets/Sounds/BossHit") { PitchVariance = 0.15f };
	public static readonly SoundStyle Phase2Transition = new("ElementalHearts/Assets/Sounds/Phase2Transition");
	public static readonly SoundStyle Phase3Transition = new("ElementalHearts/Assets/Sounds/Phase3Transition");
	
	public static readonly SoundStyle CommonEmission = new("ElementalHearts/Assets/Sounds/CommonAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
	public static readonly SoundStyle UncommonEmission = new("ElementalHearts/Assets/Sounds/UncommonAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
	public static readonly SoundStyle RareEmission = new("ElementalHearts/Assets/Sounds/RareAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
	public static readonly SoundStyle EpicEmission = new("ElementalHearts/Assets/Sounds/EpicAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
	public static readonly SoundStyle LegendaryEmission = new("ElementalHearts/Assets/Sounds/LegendaryAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
}
