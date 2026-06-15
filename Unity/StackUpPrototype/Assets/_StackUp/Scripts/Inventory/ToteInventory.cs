using System.Collections.Generic;

namespace StackUp
{
    /// <summary>
    /// Simple SKU-count container carried by the player. Plain class (no Unity
    /// dependency) so it is easy to test. See CLAUDE_CODE_SPEC.md Section 14.1.
    /// </summary>
    public class ToteInventory
    {
        private readonly Dictionary<string, int> contents = new Dictionary<string, int>();

        public int MaxUnits { get; set; } = 20;
        public IReadOnlyDictionary<string, int> Contents => contents;

        public int UnitCount
        {
            get
            {
                int total = 0;
                foreach (var kv in contents) total += kv.Value;
                return total;
            }
        }

        public bool IsFull => UnitCount >= MaxUnits;

        public int GetQuantity(string skuId)
        {
            return skuId != null && contents.TryGetValue(skuId, out int q) ? q : 0;
        }

        /// <summary>Adds up to <paramref name="quantity"/> units, respecting MaxUnits. Returns the amount actually added.</summary>
        public int Add(string skuId, int quantity)
        {
            if (string.IsNullOrEmpty(skuId) || quantity <= 0) return 0;
            int space = MaxUnits - UnitCount;
            if (space <= 0) return 0;
            int toAdd = quantity < space ? quantity : space;
            contents[skuId] = GetQuantity(skuId) + toAdd;
            return toAdd;
        }

        /// <summary>Removes up to <paramref name="quantity"/> units. Returns the amount actually removed.</summary>
        public int Remove(string skuId, int quantity)
        {
            if (string.IsNullOrEmpty(skuId) || quantity <= 0) return 0;
            int have = GetQuantity(skuId);
            int toRemove = quantity < have ? quantity : have;
            if (toRemove <= 0) return 0;
            int left = have - toRemove;
            if (left > 0) contents[skuId] = left;
            else contents.Remove(skuId);
            return toRemove;
        }

        public void Clear() => contents.Clear();
    }
}
