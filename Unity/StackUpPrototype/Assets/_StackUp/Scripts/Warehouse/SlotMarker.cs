using System;
using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Marks a rack slot in the scene and carries its identity + initial stock.
    /// Scanned by <see cref="WarehouseGrid"/>. See CLAUDE_CODE_SPEC.md Section 12.
    /// </summary>
    public class SlotMarker : MonoBehaviour
    {
        [Serializable]
        public struct StockEntry
        {
            public string SkuId;
            public int Quantity;
        }

        public string SlotId;
        public string ZoneId = "ZONE-A";
        public int Capacity = 50;
        public List<StockEntry> InitialStock = new List<StockEntry>();

        public SlotData CreateSlotData()
        {
            var data = new SlotData
            {
                SlotId = SlotId,
                WorldPosition = transform.position,
                ZoneId = ZoneId,
                Capacity = Capacity
            };
            foreach (var e in InitialStock)
            {
                if (string.IsNullOrEmpty(e.SkuId) || e.Quantity <= 0) continue;
                int existing = data.StockBySku.TryGetValue(e.SkuId, out int q) ? q : 0;
                data.StockBySku[e.SkuId] = existing + e.Quantity;
            }
            return data;
        }
    }
}
