// Architecture scaffold only. Fill behavior in the implementation pass.
namespace ElementalHearts.Core;

/// <summary>Compact packet discriminators for all mod networking. Payloads should carry typed IDs, not string identities.</summary>
public enum MessageType : byte
{
	HeartConsumed = 0,
	HeartsCleared = 1,
	HeartDeactivated = 2,
	BossHeartDropped = 3,
	ClaimIdleShards = 4,
	SyncIdleShardTime = 5,
	SyncWorldState = 6,
}
