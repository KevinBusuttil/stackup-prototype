using System;
using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Generates and tracks the active order, derives its state from what the
    /// player has collected, and completes it on load. M1 runs a single order;
    /// the queue / multiple orders arrive in M2. See CLAUDE_CODE_SPEC.md Section 13.4.
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        public CustomerOrder ActiveOrder { get; private set; }

        public event Action<CustomerOrder> OrderGenerated;
        public event Action<CustomerOrder> OrderChanged;     // state or pick progress changed
        public event Action<CustomerOrder> OrderCompleted;

        private Tote playerTote;
        private int orderCounter;

        /// <summary>Binds the player's tote so order progress tracks what was picked.</summary>
        public void BindTote(Tote tote)
        {
            if (playerTote != null) playerTote.Changed -= OnToteChanged;
            playerTote = tote;
            if (playerTote != null) playerTote.Changed += OnToteChanged;
        }

        private void OnDestroy()
        {
            if (playerTote != null) playerTote.Changed -= OnToteChanged;
        }

        public CustomerOrder GenerateOrder(IList<OrderLine> lines, string dockLaneId, float dueSeconds = 0f)
        {
            orderCounter++;
            ActiveOrder = new CustomerOrder
            {
                OrderId = $"ORD-{orderCounter:000}",
                Priority = 1,
                DockLaneId = dockLaneId,
                DueTimeSeconds = dueSeconds,
                State = OrderState.Picking,
                Lines = new List<OrderLine>(lines)
            };
            OrderGenerated?.Invoke(ActiveOrder);
            RecomputeState();
            return ActiveOrder;
        }

        public int RequiredQuantity(string skuId)
        {
            if (ActiveOrder == null || skuId == null) return 0;
            int total = 0;
            foreach (var l in ActiveOrder.Lines) if (l.SkuId == skuId) total += l.Quantity;
            return total;
        }

        public int CollectedQuantity(string skuId)
        {
            return playerTote != null ? playerTote.Inventory.GetQuantity(skuId) : 0;
        }

        public int RemainingToPick(string skuId)
        {
            int r = RequiredQuantity(skuId) - CollectedQuantity(skuId);
            return r > 0 ? r : 0;
        }

        public bool IsReadyToLoad()
        {
            return ActiveOrder != null
                && (ActiveOrder.State == OrderState.Picked || ActiveOrder.State == OrderState.Verified);
        }

        private void OnToteChanged() => RecomputeState();

        private void RecomputeState()
        {
            if (ActiveOrder == null) return;
            if (ActiveOrder.State == OrderState.Loaded || ActiveOrder.State == OrderState.Failed) return;

            bool allPicked = true;
            foreach (var line in ActiveOrder.Lines)
            {
                if (CollectedQuantity(line.SkuId) < line.Quantity) { allPicked = false; break; }
            }

            ActiveOrder.State = allPicked ? OrderState.Picked : OrderState.Picking;
            OrderChanged?.Invoke(ActiveOrder);
        }

        public void CompleteActiveOrder(PlayerController player)
        {
            if (!IsReadyToLoad()) return;

            ActiveOrder.State = OrderState.Loaded;
            if (player != null && player.Tote != null) player.Tote.Clear();

            OrderChanged?.Invoke(ActiveOrder);
            OrderCompleted?.Invoke(ActiveOrder);
        }
    }
}
