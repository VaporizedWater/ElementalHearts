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

	/// <summary>
	/// A boss heart was just dropped. Carries the heart's item type and drop position so
	/// every client can play the cosmetic drop-moment effect (see
	/// <see cref="Systems.BossHeartDropFx"/>). Server → clients only.
	/// </summary>
	BossHeartDropped = 3,

	/// <summary>
	/// Sent by a client to request claiming pending idle game shards.
	/// </summary>
	ClaimIdleShards = 4,

	/// <summary>
	/// Sent by the server to sync the LastClaimTimeTicks to all clients.
	/// </summary>
	SyncIdleShardTime = 5,

	/// <summary>
	/// Sent by a client to request cashing out pending idle shards as coins (the Piggy Bank
	/// Heart's "Sell" action). Server-authoritative, mirroring <see cref="ClaimIdleShards"/>.
	/// </summary>
	SellIdleShards = 6,

	/// <summary>
	/// A player began a Deerclops-Heart parry. Carries the parrying player's whoAmI so every other
	/// client can light up the same pink glow (see <see cref="Players.ParryAbilityPlayer"/>). Relayed
	/// by the server to all clients except the originator.
	/// </summary>
	ParryStarted = 7,
}
