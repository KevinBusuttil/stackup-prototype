using System;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// MonoBehaviour wrapper around a <see cref="ToteInventory"/>, carried by the
    /// player. Raises <see cref="Changed"/> so the HUD and OrderManager can react.
    /// </summary>
    public class Tote : MonoBehaviour
    {
        public ToteInventory Inventory { get; private set; } = new ToteInventory();
        public event Action Changed;

        public int Add(string skuId, int quantity)
        {
            int added = Inventory.Add(skuId, quantity);
            if (added > 0) Changed?.Invoke();
            return added;
        }

        public int Remove(string skuId, int quantity)
        {
            int removed = Inventory.Remove(skuId, quantity);
            if (removed > 0) Changed?.Invoke();
            return removed;
        }

        public void Clear()
        {
            Inventory.Clear();
            Changed?.Invoke();
        }
    }
}
