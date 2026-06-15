namespace StackUp
{
    /// <summary>
    /// Abstraction over Steamworks so gameplay never references Steam directly.
    /// The prototype ships a <see cref="MockSteamService"/>; a real Steamworks.NET
    /// implementation can be dropped in later without touching gameplay code.
    /// See CLAUDE_CODE_SPEC.md Sections 13.10 / 20.
    /// </summary>
    public interface ISteamService
    {
        bool IsAvailable { get; }

        bool Initialize();
        void Shutdown();
        void RunCallbacks();

        // Achievements (Section 20.2)
        void UnlockAchievement(string id);
        bool IsAchievementUnlocked(string id);

        // Stats (Section 20.3)
        void SetStat(string id, int value);
        int GetStat(string id);
        void IncrementStat(string id, int amount);
        void StoreStats();

        // Leaderboards (Section 20.4)
        void SubmitLeaderboardScore(string leaderboardId, int score);
        int GetLocalLeaderboardBest(string leaderboardId);
    }
}
