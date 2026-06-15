using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Interactable rack slot. Picks the SKU stocked here into the player's tote.
    /// Picking a SKU no active order needs costs a wrong-pick penalty.
    /// See CLAUDE_CODE_SPEC.md Sections 13.3 / 15 / 28.
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

        private string StockedSku()
        {
            if (grid == null) return null;
            var slot = grid.GetSlot(marker.SlotId);
            if (slot == null) return null;
            foreach (var kv in slot.StockBySku)
                if (kv.Value > 0) return kv.Key;
            return null;
        }

        public string GetPrompt()
        {
            string sku = StockedSku();
            if (sku == null) return "Empty slot";
            return orders != null && orders.AnyActiveOrderNeeds(sku) ? $"Pick {sku}" : $"Pick {sku} (not needed!)";
        }

        public bool CanInteract(PlayerController player)
        {
            return player != null && player.Tote != null
                && !player.Tote.Inventory.IsFull && StockedSku() != null;
        }

        public void Interact(PlayerController player)
        {
            string sku = StockedSku();
            if (sku == null) return;

            bool needed = orders != null && orders.AnyActiveOrderNeeds(sku);
            if (grid.TryTakeStock(marker.SlotId, sku, 1, out int taken) && taken > 0)
            {
                int added = player.Tote.Add(sku, taken);
                if (added < taken) grid.AddStock(marker.SlotId, sku, taken - added);
                if (added > 0 && !needed) orders?.Score?.WrongPick();
            }
        }
    }
}
