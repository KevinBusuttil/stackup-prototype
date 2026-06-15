using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// In-memory stand-in for Steam so the game runs without it. Records
    /// achievements/stats/leaderboard bests and logs them; persists nothing
    /// itself. Replaced by a real Steamworks-backed service in M6.
    /// </summary>
    public class MockSteamService : ISteamService
    {
        private readonly HashSet<string> achievements = new HashSet<string>();
        private readonly Dictionary<string, int> stats = new Dictionary<string, int>();
        private readonly Dictionary<string, int> leaderboardBest = new Dictionary<string, int>();

        public bool IsAvailable => false; // not real Steam

        public bool Initialize()
        {
            Debug.Log("[MockSteam] initialized (no real Steam).");
            return true;
        }

        public void Shutdown() { }
        public void RunCallbacks() { }

        public void UnlockAchievement(string id)
        {
            if (string.IsNullOrEmpty(id) || !achievements.Add(id)) return;
            Debug.Log($"[MockSteam] 🏆 achievement unlocked: {id}");
        }

        public bool IsAchievementUnlocked(string id) => id != null && achievements.Contains(id);

        public void SetStat(string id, int value)
        {
            if (string.IsNullOrEmpty(id)) return;
            stats[id] = value;
        }

        public int GetStat(string id) => id != null && stats.TryGetValue(id, out int v) ? v : 0;

        public void IncrementStat(string id, int amount)
        {
            if (string.IsNullOrEmpty(id) || amount == 0) return;
            stats[id] = GetStat(id) + amount;
        }

        public void StoreStats()
        {
            Debug.Log($"[MockSteam] stored {stats.Count} stats, {achievements.Count} achievements.");
        }

        public void SubmitLeaderboardScore(string leaderboardId, int score)
        {
            if (string.IsNullOrEmpty(leaderboardId)) return;
            if (!leaderboardBest.TryGetValue(leaderboardId, out int best) || score > best)
                leaderboardBest[leaderboardId] = score;
            Debug.Log($"[MockSteam] 📊 leaderboard '{leaderboardId}' <- {score}");
        }

        public int GetLocalLeaderboardBest(string leaderboardId)
            => leaderboardId != null && leaderboardBest.TryGetValue(leaderboardId, out int v) ? v : 0;
    }
}
