using System.Collections.Generic;

namespace StackUp
{
    /// <summary>A single item placed on the pallet, with the properties stacking rules care about.</summary>
    public struct PalletItem
    {
        public string SkuId;
        public WeightClass Weight;
        public StackClass Stack;
    }

    /// <summary>
    /// Width x Depth grid of stackable columns (Section 14.2). Enforces stacking
    /// rules: height limit and "heavy cannot be placed on fragile".
    /// </summary>
    public class PalletGrid
    {
        public int Width { get; }
        public int Depth { get; }
        public int MaxHeight { get; }

        private readonly List<PalletItem>[,] columns;

        public PalletGrid(int width = 3, int depth = 3, int maxHeight = 4)
        {
            Width = width;
            Depth = depth;
            MaxHeight = maxHeight;
            columns = new List<PalletItem>[width, depth];
            for (int x = 0; x < width; x++)
                for (int z = 0; z < depth; z++)
                    columns[x, z] = new List<PalletItem>();
        }

        public bool InBounds(int x, int z) => x >= 0 && x < Width && z >= 0 && z < Depth;
        public int Height(int x, int z) => InBounds(x, z) ? columns[x, z].Count : MaxHeight;

        public bool CanPlace(int x, int z, PalletItem item, out string reason)
        {
            reason = null;
            if (!InBounds(x, z)) { reason = "out of bounds"; return false; }

            var col = columns[x, z];
            if (col.Count >= MaxHeight) { reason = "stack height limit"; return false; }
            if (col.Count > 0)
            {
                var top = col[col.Count - 1];
                if (item.Weight == WeightClass.Heavy && top.Stack == StackClass.Fragile)
                {
                    reason = "heavy on fragile";
                    return false;
                }
            }
            return true;
        }

        public bool TryPlace(int x, int z, PalletItem item)
        {
            if (!CanPlace(x, z, item, out _)) return false;
            columns[x, z].Add(item);
            return true;
        }

        /// <summary>Finds the lowest legal column for the item (keeps stacks even). Returns false if none.</summary>
        public bool FindFirstValidCell(PalletItem item, out int fx, out int fz)
        {
            fx = fz = -1;
            int best = int.MaxValue;
            for (int x = 0; x < Width; x++)
                for (int z = 0; z < Depth; z++)
                    if (CanPlace(x, z, item, out _) && columns[x, z].Count < best)
                    {
                        best = columns[x, z].Count;
                        fx = x;
                        fz = z;
                    }
            return fx >= 0;
        }

        public int Count(string skuId)
        {
            int c = 0;
            for (int x = 0; x < Width; x++)
                for (int z = 0; z < Depth; z++)
                    foreach (var it in columns[x, z])
                        if (it.SkuId == skuId) c++;
            return c;
        }

        public Dictionary<string, int> Contents()
        {
            var d = new Dictionary<string, int>();
            for (int x = 0; x < Width; x++)
                for (int z = 0; z < Depth; z++)
                    foreach (var it in columns[x, z])
                        d[it.SkuId] = (d.TryGetValue(it.SkuId, out int q) ? q : 0) + 1;
            return d;
        }

        public int TotalItems
        {
            get
            {
                int c = 0;
                for (int x = 0; x < Width; x++)
                    for (int z = 0; z < Depth; z++)
                        c += columns[x, z].Count;
                return c;
            }
        }
    }
}
