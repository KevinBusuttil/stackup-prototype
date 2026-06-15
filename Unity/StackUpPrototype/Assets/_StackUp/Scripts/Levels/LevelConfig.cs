namespace StackUp
{
    /// <summary>
    /// Design-time description of a level. Drives <see cref="LevelBootstrap"/> so
    /// all 8 campaign levels (and endless) come from data rather than bespoke
    /// scenes. See CLAUDE_CODE_SPEC.md Sections 6 / 24 (M3).
    /// </summary>
    [System.Serializable]
    public class LevelConfig
    {
        public string Name = "Level";
        public string Blurb = "";

        public bool UseStacking;
        public bool UseVerification;
        public bool Endless;

        public int MaxConcurrent = 1;
        public int OrderCount = 1;        // campaign: total orders to clear
        public int DockCount = 1;
        public float SlaSeconds = 0f;     // 0 = no SLA timer

        // Which SKUs (by id) are available, and how big orders can get.
        public string[] SkuPool = { "BOX-A" };
        public int MaxLinesPerOrder = 1;
        public int MaxQtyPerLine = 1;

        // Adds a mis-stocked decoy slot so wrong-pick penalties are reachable.
        public bool IncludeDecoy;
    }
}
