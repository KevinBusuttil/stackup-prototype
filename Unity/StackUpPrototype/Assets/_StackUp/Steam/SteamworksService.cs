#if STEAMWORKS_NET
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Real Steam implementation of <see cref="ISteamService"/> via Steamworks.NET.
    /// Lives in its own assembly gated by the STEAMWORKS_NET define, so it only
    /// compiles once you install Steamworks.NET and enable the define — the default
    /// project keeps using <see cref="MockSteamService"/>. It registers itself
    /// before the first scene loads (winning over the mock); if Steam isn't running
    /// the locator falls back to the mock. See docs/STEAMWORKS_SETUP.md (M6 #51-#54).
    /// </summary>
    public class SteamworksService : ISteamService
    {
        private bool initialized;
        private readonly Dictionary<string, SteamLeaderboard_t> boards = new Dictionary<string, SteamLeaderboard_t>();
        private readonly Dictionary<string, int> pendingScores = new Dictionary<string, int>();
        private readonly Dictionary<string, CallResult<LeaderboardFindResult_t>> finds =
            new Dictionary<string, CallResult<LeaderboardFindResult_t>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoRegister() => SteamServices.Init(new SteamworksService());

        public bool IsAvailable => initialized;

        public bool Initialize()
        {
            if (initialized) return true;
            try
            {
                if (!SteamAPI.Init())
                {
                    Debug.LogWarning("[Steam] SteamAPI.Init failed (Steam running? steam_appid.txt present?).");
                    return false;
                }
                initialized = true;
                SteamUserStats.RequestCurrentStats();
                Debug.Log("[Steam] initialized.");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Steam] init exception: " + e.Message);
                return false;
            }
        }

        public void Shutdown()
        {
            if (!initialized) return;
            SteamAPI.Shutdown();
            initialized = false;
        }

        public void RunCallbacks()
        {
            if (initialized) SteamAPI.RunCallbacks();
        }

        // ---- achievements ----
        public void UnlockAchievement(string id)
        {
            if (!initialized || string.IsNullOrEmpty(id)) return;
            SteamUserStats.SetAchievement(id);
            SteamUserStats.StoreStats();
        }

        public bool IsAchievementUnlocked(string id)
        {
            return initialized && id != null && SteamUserStats.GetAchievement(id, out bool got) && got;
        }

        // ---- stats ----
        public void SetStat(string id, int value)
        {
            if (initialized && !string.IsNullOrEmpty(id)) SteamUserStats.SetStat(id, value);
        }

        public int GetStat(string id)
        {
            return initialized && id != null && SteamUserStats.GetStat(id, out int v) ? v : 0;
        }

        public void IncrementStat(string id, int amount)
        {
            if (!initialized || string.IsNullOrEmpty(id) || amount == 0) return;
            SetStat(id, GetStat(id) + amount);
        }

        public void StoreStats()
        {
            if (initialized) SteamUserStats.StoreStats();
        }

        // ---- leaderboards ----
        public void SubmitLeaderboardScore(string leaderboardId, int score)
        {
            if (!initialized || string.IsNullOrEmpty(leaderboardId)) return;

            if (boards.TryGetValue(leaderboardId, out var handle))
            {
                Upload(handle, score);
                return;
            }

            // keep the best pending score until the handle resolves
            pendingScores[leaderboardId] = pendingScores.TryGetValue(leaderboardId, out int p) ? Mathf.Max(p, score) : score;

            if (finds.ContainsKey(leaderboardId)) return;
            var cr = CallResult<LeaderboardFindResult_t>.Create((res, fail) => OnFind(leaderboardId, res, fail));
            finds[leaderboardId] = cr;
            var call = SteamUserStats.FindOrCreateLeaderboard(
                leaderboardId,
                ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending,
                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);
            cr.Set(call);
        }

        private void OnFind(string id, LeaderboardFindResult_t res, bool failure)
        {
            finds.Remove(id);
            if (failure || res.m_bLeaderboardFound == 0) return;
            boards[id] = res.m_hSteamLeaderboard;
            if (pendingScores.TryGetValue(id, out int s))
            {
                Upload(res.m_hSteamLeaderboard, s);
                pendingScores.Remove(id);
            }
        }

        private static void Upload(SteamLeaderboard_t handle, int score)
        {
            SteamUserStats.UploadLeaderboardScore(
                handle,
                ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
                score, null, 0);
        }

        public int GetLocalLeaderboardBest(string leaderboardId) => 0; // server-side; not cached locally
    }
}
#endif
