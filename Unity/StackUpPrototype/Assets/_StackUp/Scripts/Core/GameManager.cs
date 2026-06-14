using System;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// High-level game state machine and entry point. Owns the overall game
    /// state, selected mode, and score. Gameplay systems subscribe to
    /// <see cref="StateChanged"/> rather than polling.
    /// See CLAUDE_CODE_SPEC.md Section 13.1.
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

        public GameState State { get; private set; } = GameState.Boot;
        public GameMode Mode => mode;
        public int Score { get; private set; }

        /// <summary>Raised whenever the game state changes (old, new).</summary>
        public event Action<GameState, GameState> StateChanged;

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
            // Bootstrap: from here we would load the Main Menu. For the M0
            // prototype we simply report that the game came up cleanly.
            SetState(GameState.MainMenu);
        }

        public void SetState(GameState next)
        {
            if (next == State)
            {
                return;
            }

            GameState previous = State;
            State = next;
            StateChanged?.Invoke(previous, next);
        }

        public void SetMode(GameMode newMode)
        {
            mode = newMode;
        }

        public void AddScore(int points)
        {
            Score = Mathf.Max(0, Score + points);
        }

        public void ResetScore()
        {
            Score = 0;
        }
    }
}
