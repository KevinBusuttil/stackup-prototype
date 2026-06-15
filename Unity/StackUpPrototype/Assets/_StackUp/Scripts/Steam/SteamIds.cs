namespace StackUp
{
    /// <summary>
    /// String keys for Steam achievements, stats, and leaderboards. Centralised so
    /// the real Steamworks config can mirror them. See CLAUDE_CODE_SPEC.md
    /// Sections 20.2 / 20.3 / 20.4.
    /// </summary>
    public static class SteamIds
    {
        // Achievements
        public const string FirstPick = "FIRST_PICK";
        public const string FirstOrder = "FIRST_ORDER";
        public const string PerfectOrder = "PERFECT_ORDER";
        public const string Combo5 = "COMBO_5";
        public const string Combo10 = "COMBO_10";
        public const string StackMaster = "STACK_MASTER";
        public const string NoRework = "NO_REWORK";
        public const string SpeedRunner = "SPEED_RUNNER";
        public const string Endless10 = "ENDLESS_10";
        public const string Endless25 = "ENDLESS_25";
        public const string DockPerfect = "DOCK_PERFECT";
        public const string WarehousePro = "WAREHOUSE_PRO";

        // Stats
        public const string StatOrders = "total_orders";
        public const string StatPicks = "total_picks";
        public const string StatPerfect = "perfect_orders";
        public const string StatWrongPicks = "wrong_picks";
        public const string StatRework = "rework_jobs";
        public const string StatBestCombo = "best_combo";
        public const string StatPlayTime = "play_time_seconds";
        public const string StatHighestWave = "highest_endless_wave";

        // Leaderboards
        public const string LbEndlessScore = "endless_high_score";
        public const string LbEndlessWave = "endless_highest_wave";

        public static string LevelTimeBoard(int levelIndex) => $"level_{levelIndex + 1}_best_time";
    }
}
