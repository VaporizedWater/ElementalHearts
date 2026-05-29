namespace ElementalHearts.Common.Network;

public enum MessageType : byte
{
	HeartConsumed = 0,
	HeartsCleared = 1,
	/// <summary>
	/// A toggleable heart (currently only buff-granting Potion Hearts) was re-used to
	/// disable its world-wide effect. Carries the heart's item type so it can be
	/// resolved on each receiver.
	/// </summary>
	HeartDeactivated = 2,
}
