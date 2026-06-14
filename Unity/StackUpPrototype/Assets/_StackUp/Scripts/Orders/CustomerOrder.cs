using System.Collections.Generic;

namespace StackUp
{
    [System.Serializable]
    public class OrderLine
    {
        public string SkuId;
        public int Quantity;
    }

    public enum OrderState
    {
        Pending,
        Picking,
        Picked,
        VerificationFailed,
        Verified,
        Loaded,
        Failed
    }

    /// <summary>
    /// A customer order the player must pick, verify, and load.
    /// </summary>
    [System.Serializable]
    public class CustomerOrder
    {
        public string OrderId;
        public int Priority;
        public List<OrderLine> Lines = new List<OrderLine>();
        public float DueTimeSeconds;
        public string DockLaneId;
        public OrderState State = OrderState.Pending;
    }
}
