using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Registry of all rack slots in the level. Scans <see cref="SlotMarker"/>s,
    /// stores stock by SKU, and supports lookup / nearest-slot queries.
    /// See CLAUDE_CODE_SPEC.md Section 12.
    /// </summary>
    public class WarehouseGrid : MonoBehaviour
    {
        [SerializeField] private bool drawDebug = true;

        private readonly Dictionary<string, SlotData> slots = new Dictionary<string, SlotData>();
        public IReadOnlyDictionary<string, SlotData> Slots => slots;

        /// <summary>Scans every <see cref="SlotMarker"/> in the scene and registers it. Call once the level is built.</summary>
        public void Build()
        {
            slots.Clear();
            foreach (var marker in FindObjectsByType<SlotMarker>(FindObjectsInactive.Exclude))
            {
                if (string.IsNullOrEmpty(marker.SlotId))
                {
                    Debug.LogWarning($"SlotMarker on '{marker.name}' has no SlotId; skipped.", marker);
                    continue;
                }
                if (slots.ContainsKey(marker.SlotId))
                {
                    Debug.LogWarning($"Duplicate SlotId '{marker.SlotId}'; skipped.", marker);
                    continue;
                }
                slots[marker.SlotId] = marker.CreateSlotData();
            }
        }

        public SlotData GetSlot(string slotId)
        {
            return slotId != null && slots.TryGetValue(slotId, out var s) ? s : null;
        }

        public int GetStock(string slotId, string skuId)
        {
            var s = GetSlot(slotId);
            return s != null && skuId != null && s.StockBySku.TryGetValue(skuId, out int q) ? q : 0;
        }

        /// <summary>Removes up to <paramref name="quantity"/> of a SKU from a slot. Returns true if any was taken.</summary>
        public bool TryTakeStock(string slotId, string skuId, int quantity, out int taken)
        {
            taken = 0;
            var s = GetSlot(slotId);
            if (s == null || skuId == null || !s.StockBySku.TryGetValue(skuId, out int have) || have <= 0) return false;
            taken = quantity < have ? quantity : have;
            int left = have - taken;
            if (left > 0) s.StockBySku[skuId] = left;
            else s.StockBySku.Remove(skuId);
            return taken > 0;
        }

        /// <summary>Puts stock back into a slot (e.g. when a pick could not be carried).</summary>
        public void AddStock(string slotId, string skuId, int quantity)
        {
            var s = GetSlot(slotId);
            if (s == null || string.IsNullOrEmpty(skuId) || quantity <= 0) return;
            int existing = s.StockBySku.TryGetValue(skuId, out int q) ? q : 0;
            s.StockBySku[skuId] = existing + quantity;
        }

        public List<SlotData> FindSlotsWithSku(string skuId)
        {
            var result = new List<SlotData>();
            foreach (var s in slots.Values)
                if (skuId != null && s.StockBySku.TryGetValue(skuId, out int q) && q > 0) result.Add(s);
            return result;
        }

        public SlotData FindNearestSlotWithSku(string skuId, Vector3 from)
        {
            SlotData best = null;
            float bestSqr = float.MaxValue;
            foreach (var s in slots.Values)
            {
                if (skuId == null || !s.StockBySku.TryGetValue(skuId, out int q) || q <= 0) continue;
                float d = (s.WorldPosition - from).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = s; }
            }
            return best;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawDebug || slots.Count == 0) return;
            Gizmos.color = Color.cyan;
            foreach (var s in slots.Values)
                Gizmos.DrawWireCube(s.WorldPosition, Vector3.one * 0.3f);
        }
#endif
    }
}
