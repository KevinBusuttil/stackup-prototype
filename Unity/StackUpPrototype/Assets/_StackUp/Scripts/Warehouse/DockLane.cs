using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Dock lane. Loads the selected order when it is ready; loading at the wrong
    /// lane is blocked and penalised. See CLAUDE_CODE_SPEC.md Sections 13.8 / 16 / 28.
    /// </summary>
    public class DockLane : MonoBehaviour, IInteractable
    {
        public string DockLaneId = "DOCK-1";
        private OrderManager orders;

        public void Init(OrderManager orders) => this.orders = orders;

        public string GetPrompt()
        {
            if (orders == null) return "";
            var order = orders.SelectedOrder;
            if (order == null || !orders.IsReadyToLoad(order)) return "No order ready";
            return order.DockLaneId == DockLaneId
                ? $"Load order at {DockLaneId}"
                : $"Wrong lane (needs {order.DockLaneId})";
        }

        public bool CanInteract(PlayerController player)
        {
            // Ready orders are interactable at any lane so a wrong-lane attempt can be penalised.
            return orders != null && orders.IsReadyToLoad(orders.SelectedOrder);
        }

        public void Interact(PlayerController player)
        {
            if (orders == null) return;
            orders.TryLoad(orders.SelectedOrder, DockLaneId, out _);
        }
    }
}
