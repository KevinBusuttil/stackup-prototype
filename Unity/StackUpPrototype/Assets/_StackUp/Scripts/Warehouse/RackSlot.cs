using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Interactable rack slot. Picks the active order's still-needed SKU from this
    /// slot into the player's tote. See CLAUDE_CODE_SPEC.md Sections 13.3 / 15.
    /// </summary>
    [RequireComponent(typeof(SlotMarker))]
    public class RackSlot : MonoBehaviour, IInteractable
    {
        private SlotMarker marker;
        private WarehouseGrid grid;
        private OrderManager orders;

        private void Awake() => marker = GetComponent<SlotMarker>();

        public void Init(WarehouseGrid grid, OrderManager orders)
        {
            this.grid = grid;
            this.orders = orders;
        }

        /// <summary>The SKU this slot can usefully provide for the active order right now, or null.</summary>
        private string NeededSkuHere()
        {
            if (orders == null || grid == null || orders.ActiveOrder == null) return null;
            foreach (var line in orders.ActiveOrder.Lines)
            {
                if (orders.RemainingToPick(line.SkuId) > 0 && grid.GetStock(marker.SlotId, line.SkuId) > 0)
                    return line.SkuId;
            }
            return null;
        }

        public string GetPrompt()
        {
            string sku = NeededSkuHere();
            return sku != null ? $"Pick {sku}" : "Nothing to pick here";
        }

        public bool CanInteract(PlayerController player)
        {
            return player != null && player.Tote != null
                && !player.Tote.Inventory.IsFull && NeededSkuHere() != null;
        }

        public void Interact(PlayerController player)
        {
            string sku = NeededSkuHere();
            if (sku == null) return;

            if (grid.TryTakeStock(marker.SlotId, sku, 1, out int taken) && taken > 0)
            {
                int added = player.Tote.Add(sku, taken);
                if (added < taken) grid.AddStock(marker.SlotId, sku, taken - added); // couldn't carry it all
            }
        }
    }
}
