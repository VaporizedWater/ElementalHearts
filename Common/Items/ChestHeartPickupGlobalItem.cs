using ElementalHearts.Common.Players;
using Terraria;
using Terraria.ModLoader;

namespace ElementalHearts.Common.Items;

/// <summary>
/// Hooks world-item pickup so that items overflow into the Chest Heart's 10 extra slots
/// when the vanilla inventory is completely full.
///
/// Two hooks are required:
///  • <see cref="ItemSpace"/> — tells the engine that the player HAS room (so the item
///    gets attracted and the pickup sequence begins). Without this, a full vanilla inventory
///    means the item is never grabbed and <see cref="OnPickup"/> never fires.
///  • <see cref="OnPickup"/>  — actually absorbs the item into the extra slots and returns
///    <see langword="false"/> so vanilla doesn't try to place it in the already-full slots.
/// </summary>
public sealed class ChestHeartPickupGlobalItem : GlobalItem
{
    /// <summary>
    /// Called by the engine to decide whether the player has room for this item.
    /// We return <see langword="true"/> when vanilla is full but our extra slots aren't,
    /// which causes the item to start moving toward the player and triggers <see cref="OnPickup"/>.
    /// </summary>
    public override bool ItemSpace(Item pickupItem, Player player)
    {
        // Only affect the local player while the ability is active.
        if (player.whoAmI != Main.myPlayer)
            return false;

        var chestPlayer = player.GetModPlayer<ChestHeartPlayer>();
        if (!chestPlayer.Enabled)
            return false;

        // If vanilla inventory still has room, let vanilla report its own space status.
        if (VanillaHasSpace(player, pickupItem))
            return false; // don't override — let vanilla return true via its own check

        // Vanilla is full. Signal extra-slot space if we have any.
        return ExtraInventoryHasSpaceStatic(chestPlayer, pickupItem);
    }

    /// <summary>
    /// Called when the item has been successfully picked up by the player.
    /// We absorb it into the extra slots and return <see langword="false"/> to prevent
    /// vanilla from trying (and failing) to place it in the full main inventory.
    /// </summary>
    public override bool OnPickup(Item item, Player player)
    {
        // Only run on the local player and only when Chest Heart is active.
        if (player.whoAmI != Main.myPlayer)
            return true;

        var chestPlayer = player.GetModPlayer<ChestHeartPlayer>();
        if (!chestPlayer.Enabled)
            return true;

        // If vanilla still has room, let it handle this pickup as normal.
        if (VanillaHasSpace(player, item))
            return true;

        // Vanilla is full — absorb into extra slots.
        // Return false = "we handled it, don't put it in vanilla inventory".
        // Return true  = extra slots also full, let vanilla drop it.
        bool absorbed = chestPlayer.TryPutInExtraInventory(item);
        return !absorbed;
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <see langword="true"/> if vanilla inventory slots 0–57 can accept
    /// this item (empty slot or matching stack with headroom).
    /// Mirrors the range vanilla's own <c>Player.GetItem</c> examines.
    /// </summary>
    private static bool VanillaHasSpace(Player player, Item item)
    {
        // 0-9 hotbar, 10-49 main, 50-53 coins, 54-57 ammo; 58 is trash (excluded).
        for (int i = 0; i < 58; i++)
        {
            Item slot = player.inventory[i];
            if (slot.IsAir)
                return true;
            if (slot.type == item.type && slot.stack < slot.maxStack)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> if any of the 10 extra slots (indices 10–19)
    /// can accept this item. Public so the IL hook in
    /// <see cref="ChestHeartInventorySystem"/> can call it without duplication.
    /// </summary>
    public static bool ExtraInventoryHasSpaceStatic(ChestHeartPlayer chestPlayer, Item item)
    {
        for (int i = 10; i < 20; i++)
        {
            Item slot = chestPlayer.ExtraInventory[i];
            if (slot.IsAir)
                return true;
            if (slot.type == item.type && slot.stack < slot.maxStack)
                return true;
        }
        return false;
    }
}

