using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Dock lane. Accepts the active order when it is ready to load and its
    /// DockLaneId matches. See CLAUDE_CODE_SPEC.md Sections 13.8 / 16.
    /// </summary>
    public class DockLane : MonoBehaviour, IInteractable
    {
        public string DockLaneId = "DOCK-1";
        private OrderManager orders;

        public void Init(OrderManager orders) => this.orders = orders;

        public string GetPrompt()
        {
            if (orders == null) return "";
            var order = orders.ActiveOrder;
            if (order == null) return "No order";
            if (!orders.IsReadyToLoad()) return "Order not ready";
            if (order.DockLaneId != DockLaneId) return $"Wrong lane (needs {order.DockLaneId})";
            return $"Load order at {DockLaneId}";
        }

        public bool CanInteract(PlayerController player)
        {
            return orders != null && orders.IsReadyToLoad()
                && orders.ActiveOrder.DockLaneId == DockLaneId;
        }

        public void Interact(PlayerController player)
        {
            if (CanInteract(player)) orders.CompleteActiveOrder(player);
        }
    }
}
