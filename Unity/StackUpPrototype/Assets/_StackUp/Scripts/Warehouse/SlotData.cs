using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Runtime data for a single rack slot, registered by the WarehouseGrid.
    /// </summary>
    public class SlotData
    {
        public string SlotId;
        public Vector3 WorldPosition;
        public string ZoneId;
        public Dictionary<string, int> StockBySku = new Dictionary<string, int>();
        public int Capacity;
    }
}
