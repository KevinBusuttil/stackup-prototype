using System;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// High-level game state machine and entry point. Owns the overall game
    /// state, selected mode, and score. Gameplay systems subscribe to events
    /// rather than polling. See CLAUDE_CODE_SPEC.md Section 13.1.
    /// </summary>
    public enum GameState
    {
        Boot,
        MainMenu,
        LevelLoading,
        Running,
        Paused,
        Results
    }

    public enum GameMode
    {
        Campaign,
        Endless
    }

    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameMode mode = GameMode.Campaign;

        /// <summary>When true (Bootstrap scene), Start() advances Boot -> MainMenu. Level scenes set this false.</summary>
        public bool AdvanceToMenuOnStart = true;

        public GameState State { get; private set; } = GameState.Boot;
        public GameMode Mode => mode;
        public int Score { get; private set; }

        /// <summary>Raised whenever the game state changes (old, new).</summary>
        public event Action<GameState, GameState> StateChanged;
        /// <summary>Raised whenever the score changes (new total).</summary>
        public event Action<int> ScoreChanged;
        /// <summary>Raised when a level finishes (final score).</summary>
        public event Action<int> LevelCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (AdvanceToMenuOnStart && State == GameState.Boot)
            {
                SetState(GameState.MainMenu);
            }
        }

        public void SetState(GameState next)
        {
            if (next == State) return;

            GameState previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
        }

        public void SetMode(GameMode newMode) => mode = newMode;

        public void AddScore(int points)
        {
            Score = Mathf.Max(0, Score + points);
            ScoreChanged?.Invoke(Score);
        }

        public void ResetScore()
        {
            Score = 0;
            ScoreChanged?.Invoke(Score);
        }

        public void CompleteLevel(int finalScore)
        {
            SetState(GameState.Results);
            LevelCompleted?.Invoke(finalScore);
        }
    }
}
