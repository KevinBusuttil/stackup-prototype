using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// The single place that translates gameplay events into Steam achievements,
    /// stats, and leaderboard submissions via <see cref="ISteamService"/>. Gameplay
    /// systems raise plain C# events and never reference Steam. See
    /// CLAUDE_CODE_SPEC.md Sections 20.2 / 20.3 / 20.4 (M4 #39-#41).
    /// </summary>
    public class SteamTelemetry : MonoBehaviour
    {
        private OrderManager orders;
        private ScoreSystem score;
        private GameManager game;
        private Tote tote;
        private LevelConfig cfg;
        private int campaignIndex;

        private int lastToteCount;
        private bool firstPick;
        private bool firstOrder;
        private int illegalStacks;
        private int dockStreak;
        private int highestWaveSeen;
        private float playSeconds;

        private static ISteamService Steam => SteamServices.Current;

        public void Init(OrderManager orders, ScoreSystem score, GameManager game, Tote tote, LevelConfig cfg, int campaignIndex)
        {
            this.orders = orders;
            this.score = score;
            this.game = game;
            this.tote = tote;
            this.cfg = cfg;
            this.campaignIndex = campaignIndex;

            lastToteCount = tote != null ? tote.Inventory.UnitCount : 0;

            if (tote != null) tote.Changed += OnToteChanged;
            if (score != null)
            {
                score.OrderScored += OnOrderScored;
                score.WrongPicked += OnWrongPicked;
                score.WrongDocked += OnWrongDocked;
                score.IllegalStacked += OnIllegalStacked;
                score.FailedVerified += OnFailedVerified;
            }
            if (orders != null) orders.OrderCompleted += OnOrderCompleted;
        }

        private void OnDestroy()
        {
            if (tote != null) tote.Changed -= OnToteChanged;
            if (score != null)
            {
                score.OrderScored -= OnOrderScored;
                score.WrongPicked -= OnWrongPicked;
                score.WrongDocked -= OnWrongDocked;
                score.IllegalStacked -= OnIllegalStacked;
                score.FailedVerified -= OnFailedVerified;
            }
            if (orders != null) orders.OrderCompleted -= OnOrderCompleted;
        }

        private void Update()
        {
            if (game != null && game.State == GameState.Running) playSeconds += Time.deltaTime;
        }

        // -------------------------------------------------------- live hooks
        private void OnToteChanged()
        {
            int c = tote != null ? tote.Inventory.UnitCount : 0;
            if (c > lastToteCount)
            {
                Steam?.IncrementStat(SteamIds.StatPicks, c - lastToteCount);
                if (!firstPick) { firstPick = true; Steam?.UnlockAchievement(SteamIds.FirstPick); }
            }
            lastToteCount = c;
        }

        private void OnOrderCompleted(CustomerOrder order)
        {
            Steam?.IncrementStat(SteamIds.StatOrders, 1);
            if (!firstOrder) { firstOrder = true; Steam?.UnlockAchievement(SteamIds.FirstOrder); }

            dockStreak++;
            if (dockStreak >= 5) Steam?.UnlockAchievement(SteamIds.DockPerfect);

            if (orders != null && orders.Endless)
            {
                int w = orders.Wave;
                if (w > highestWaveSeen) { highestWaveSeen = w; Steam?.SetStat(SteamIds.StatHighestWave, w); }
                if (w >= 10) Steam?.UnlockAchievement(SteamIds.Endless10);
                if (w >= 25) Steam?.UnlockAchievement(SteamIds.Endless25);
            }
        }

        private void OnOrderScored(bool perfect)
        {
            if (perfect)
            {
                Steam?.IncrementStat(SteamIds.StatPerfect, 1);
                Steam?.UnlockAchievement(SteamIds.PerfectOrder);
                int combo = score != null ? score.Combo : 0;
                if (combo >= 5) Steam?.UnlockAchievement(SteamIds.Combo5);
                if (combo >= 10) Steam?.UnlockAchievement(SteamIds.Combo10);
            }
            if (score != null) Steam?.SetStat(SteamIds.StatBestCombo, score.BestCombo);
        }

        private void OnWrongPicked() => Steam?.IncrementStat(SteamIds.StatWrongPicks, 1);
        private void OnWrongDocked() => dockStreak = 0;
        private void OnIllegalStacked() => illegalStacks++;
        private void OnFailedVerified() => Steam?.IncrementStat(SteamIds.StatRework, 1);

        // ------------------------------------------------------ level results
        public void ReportCampaignWin(float elapsedSeconds)
        {
            FlushPlayTime();
            if (cfg != null && cfg.UseStacking && illegalStacks == 0) Steam?.UnlockAchievement(SteamIds.StackMaster);
            if (orders != null && orders.ReworkJobsCreated == 0) Steam?.UnlockAchievement(SteamIds.NoRework);
            if (cfg != null && cfg.TargetTimeSeconds > 0f && elapsedSeconds <= cfg.TargetTimeSeconds)
                Steam?.UnlockAchievement(SteamIds.SpeedRunner);

            Steam?.SubmitLeaderboardScore(SteamIds.LevelTimeBoard(campaignIndex), Mathf.RoundToInt(elapsedSeconds));

            if (SaveService.AllCampaignComplete(LevelLibrary.CampaignCount))
                Steam?.UnlockAchievement(SteamIds.WarehousePro);

            Steam?.StoreStats();
        }

        public void ReportEndlessEnd()
        {
            FlushPlayTime();
            Steam?.SubmitLeaderboardScore(SteamIds.LbEndlessScore, game != null ? game.Score : 0);
            Steam?.SubmitLeaderboardScore(SteamIds.LbEndlessWave, orders != null ? orders.Wave : 0);
            Steam?.StoreStats();
        }

        private void FlushPlayTime()
        {
            int whole = Mathf.FloorToInt(playSeconds);
            if (whole > 0) { Steam?.IncrementStat(SteamIds.StatPlayTime, whole); playSeconds -= whole; }
        }
    }
}
