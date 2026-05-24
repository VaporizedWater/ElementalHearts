using Terraria.Audio;

namespace ElementalHearts.Content.NPCs.Bosses.Animate;

/// <summary>
/// Shared custom sound effects used across the Animate boss family.
/// </summary>
public static class AnimateBossSounds
{
	public static readonly SoundStyle BossHit = new("ElementalHearts/Sounds/BossHit") { PitchVariance = 0.15f };
	public static readonly SoundStyle Phase2Transition = new("ElementalHearts/Sounds/Phase2Transition");
	public static readonly SoundStyle Phase3Transition = new("ElementalHearts/Sounds/Phase3Transition");
	
	public static readonly SoundStyle CommonEmission = new("ElementalHearts/Sounds/CommonAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
	public static readonly SoundStyle UncommonEmission = new("ElementalHearts/Sounds/UncommonAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
	public static readonly SoundStyle RareEmission = new("ElementalHearts/Sounds/RareAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
	public static readonly SoundStyle EpicEmission = new("ElementalHearts/Sounds/EpicAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
	public static readonly SoundStyle LegendaryEmission = new("ElementalHearts/Sounds/LegendaryAnimateEmission") { IsLooped = true, Volume = 0.5f, Type = SoundType.Ambient };
}
