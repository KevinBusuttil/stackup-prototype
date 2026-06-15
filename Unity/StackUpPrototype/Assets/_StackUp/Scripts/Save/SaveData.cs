using System.Collections.Generic;

namespace StackUp
{
    /// <summary>Per-level campaign record. See CLAUDE_CODE_SPEC.md Section 21.</summary>
    [System.Serializable]
    public class LevelRecord
    {
        public int Index;
        public int BestScore;
        public bool Completed;
    }

    /// <summary>Campaign progress: which levels are unlocked and best scores.</summary>
    [System.Serializable]
    public class ProgressData
    {
        public int HighestUnlockedLevel = 0; // index of the furthest unlocked level
        public List<LevelRecord> Levels = new List<LevelRecord>();

        public LevelRecord GetOrCreate(int index)
        {
            foreach (var r in Levels) if (r.Index == index) return r;
            var rec = new LevelRecord { Index = index };
            Levels.Add(rec);
            return rec;
        }
    }

    /// <summary>Endless-mode bests (Section 20.4).</summary>
    [System.Serializable]
    public class HighScoreData
    {
        public int EndlessHighScore;
        public int EndlessHighestWave;
    }
}
