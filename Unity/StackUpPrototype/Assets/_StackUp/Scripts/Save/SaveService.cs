using System.IO;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// JSON persistence under Application.persistentDataPath. Steam Cloud can map
    /// these files later (M4/M6). See CLAUDE_CODE_SPEC.md Section 21.
    /// </summary>
    public static class SaveService
    {
        private const string ProgressFile = "progress.json";
        private const string HighScoreFile = "highscores.json";

        public static ProgressData Progress { get; private set; } = new ProgressData();
        public static HighScoreData HighScores { get; private set; } = new HighScoreData();

        private static bool loaded;

        public static void Load()
        {
            Progress = Read<ProgressData>(ProgressFile) ?? new ProgressData();
            HighScores = Read<HighScoreData>(HighScoreFile) ?? new HighScoreData();
            loaded = true;
        }

        private static void EnsureLoaded()
        {
            if (!loaded) Load();
        }

        public static bool IsUnlocked(int levelIndex)
        {
            EnsureLoaded();
            return levelIndex <= Progress.HighestUnlockedLevel;
        }

        public static int BestScore(int levelIndex)
        {
            EnsureLoaded();
            foreach (var r in Progress.Levels) if (r.Index == levelIndex) return r.BestScore;
            return 0;
        }

        /// <summary>Records a campaign completion, unlocks the next level, and saves.</summary>
        public static void CompleteCampaignLevel(int levelIndex, int score, int totalLevels)
        {
            EnsureLoaded();
            var rec = Progress.GetOrCreate(levelIndex);
            rec.Completed = true;
            if (score > rec.BestScore) rec.BestScore = score;

            int next = levelIndex + 1;
            if (next < totalLevels && next > Progress.HighestUnlockedLevel)
                Progress.HighestUnlockedLevel = next;

            Write(ProgressFile, Progress);
        }

        public static void RecordEndless(int score, int wave)
        {
            EnsureLoaded();
            bool changed = false;
            if (score > HighScores.EndlessHighScore) { HighScores.EndlessHighScore = score; changed = true; }
            if (wave > HighScores.EndlessHighestWave) { HighScores.EndlessHighestWave = wave; changed = true; }
            if (changed) Write(HighScoreFile, HighScores);
        }

        private static T Read<T>(string fileName) where T : class
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, fileName);
                if (!File.Exists(path)) return null;
                return JsonUtility.FromJson<T>(File.ReadAllText(path));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"SaveService: failed to read {fileName}: {e.Message}");
                return null;
            }
        }

        private static void Write<T>(string fileName, T data)
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, fileName);
                File.WriteAllText(path, JsonUtility.ToJson(data, true));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"SaveService: failed to write {fileName}: {e.Message}");
            }
        }
    }
}
