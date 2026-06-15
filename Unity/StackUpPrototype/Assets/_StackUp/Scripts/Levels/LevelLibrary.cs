namespace StackUp
{
    /// <summary>
    /// The 8 campaign levels (Section 6.1) plus the endless config (Section 6.2),
    /// expressed as data. Also the master SKU pool shared across levels.
    /// </summary>
    public static class LevelLibrary
    {
        public const string BoxA = "BOX-A";
        public const string GlassB = "GLASS-B";
        public const string SteelC = "STEEL-C";
        public const string BagD = "BAG-D";

        public static readonly LevelConfig[] Campaign =
        {
            new LevelConfig
            {
                Name = "First Pick", Blurb = "One SKU, one order. Pick and load.",
                UseStacking = false, UseVerification = false,
                MaxConcurrent = 1, OrderCount = 1, DockCount = 1, SlaSeconds = 0f, TargetTimeSeconds = 45f,
                SkuPool = new[] { BoxA }, MaxLinesPerOrder = 1, MaxQtyPerLine = 1
            },
            new LevelConfig
            {
                Name = "More Items", Blurb = "Multiple SKUs and a small order queue.",
                UseStacking = false, UseVerification = false,
                MaxConcurrent = 1, OrderCount = 3, DockCount = 1, SlaSeconds = 0f,
                SkuPool = new[] { BoxA, SteelC, BagD }, MaxLinesPerOrder = 2, MaxQtyPerLine = 2
            },
            new LevelConfig
            {
                Name = "Stacking Intro", Blurb = "Stack items onto a pallet.",
                UseStacking = true, UseVerification = false,
                MaxConcurrent = 1, OrderCount = 2, DockCount = 1, SlaSeconds = 0f,
                SkuPool = new[] { BoxA, GlassB, SteelC }, MaxLinesPerOrder = 2, MaxQtyPerLine = 2
            },
            new LevelConfig
            {
                Name = "Verification", Blurb = "Verify before you load.",
                UseStacking = true, UseVerification = true,
                MaxConcurrent = 1, OrderCount = 2, DockCount = 1, SlaSeconds = 0f, TargetTimeSeconds = 150f,
                SkuPool = new[] { BoxA, GlassB, SteelC }, MaxLinesPerOrder = 2, MaxQtyPerLine = 2,
                IncludeDecoy = true
            },
            new LevelConfig
            {
                Name = "Time Pressure", Blurb = "Multiple orders against the clock.",
                UseStacking = true, UseVerification = true,
                MaxConcurrent = 2, OrderCount = 4, DockCount = 2, SlaSeconds = 60f,
                SkuPool = new[] { BoxA, GlassB, SteelC }, MaxLinesPerOrder = 2, MaxQtyPerLine = 2,
                IncludeDecoy = true
            },
            new LevelConfig
            {
                Name = "Batch Picking", Blurb = "Route efficiently across more orders.",
                UseStacking = true, UseVerification = true,
                MaxConcurrent = 2, OrderCount = 5, DockCount = 2, SlaSeconds = 70f,
                SkuPool = new[] { BoxA, GlassB, SteelC, BagD }, MaxLinesPerOrder = 3, MaxQtyPerLine = 2,
                IncludeDecoy = true
            },
            new LevelConfig
            {
                Name = "Larger Warehouse", Blurb = "More SKUs, longer travel.",
                UseStacking = true, UseVerification = true,
                MaxConcurrent = 2, OrderCount = 5, DockCount = 2, SlaSeconds = 0f,
                SkuPool = new[] { BoxA, GlassB, SteelC, BagD }, MaxLinesPerOrder = 3, MaxQtyPerLine = 3,
                IncludeDecoy = true
            },
            new LevelConfig
            {
                Name = "Controlled Chaos", Blurb = "Concurrent orders, verify, manage docks.",
                UseStacking = true, UseVerification = true,
                MaxConcurrent = 3, OrderCount = 6, DockCount = 2, SlaSeconds = 50f,
                SkuPool = new[] { BoxA, GlassB, SteelC, BagD }, MaxLinesPerOrder = 3, MaxQtyPerLine = 2,
                IncludeDecoy = true
            },
        };

        public static LevelConfig Endless()
        {
            return new LevelConfig
            {
                Name = "Endless", Blurb = "Survive escalating waves.",
                UseStacking = true, UseVerification = true, Endless = true,
                MaxConcurrent = 2, OrderCount = 0, DockCount = 2, SlaSeconds = 60f,
                SkuPool = new[] { BoxA, GlassB, SteelC, BagD }, MaxLinesPerOrder = 3, MaxQtyPerLine = 2,
                IncludeDecoy = true
            };
        }

        public static int CampaignCount => Campaign.Length;

        public static LevelConfig Get(int index)
        {
            if (index < 0 || index >= Campaign.Length) return Campaign[0];
            return Campaign[index];
        }
    }
}
