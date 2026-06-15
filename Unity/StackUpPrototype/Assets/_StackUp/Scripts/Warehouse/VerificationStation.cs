using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Verification station. Checks the selected order's collected contents
    /// against its lines; passes or fails (creating rework). See
    /// CLAUDE_CODE_SPEC.md Sections 13.7 / 15.
    /// </summary>
    public class VerificationStation : MonoBehaviour, IInteractable
    {
        private OrderManager orders;

        public void Init(OrderManager orders) => this.orders = orders;

        private CustomerOrder Target()
        {
            if (orders == null) return null;
            var o = orders.SelectedOrder;
            if (o == null) return null;
            // Only meaningful before it is verified/loaded.
            return (o.State == OrderState.Verified || o.State == OrderState.Loaded) ? null : o;
        }

        public string GetPrompt()
        {
            var o = Target();
            return o != null ? $"Verify order {o.OrderId}" : "Nothing to verify";
        }

        public bool CanInteract(PlayerController player) => Target() != null;

        public void Interact(PlayerController player)
        {
            var o = Target();
            if (o != null) orders.Verify(o);
        }
    }
}
