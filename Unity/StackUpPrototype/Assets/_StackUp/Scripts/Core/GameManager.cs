using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StackUp
{
    /// <summary>
    /// High-level game state machine, mode/level selection, score, and scene
    /// navigation. Persists across scene loads. See CLAUDE_CODE_SPEC.md Section 13.1.
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
        public const string MainMenuScene = "MainMenu";
        public const string GameScene = "Game";

        public static GameManager Instance { get; private set; }

        [SerializeField] private GameMode mode = GameMode.Campaign;

        /// <summary>When true (Bootstrap scene), Start() loads the main menu. Level scenes set this false.</summary>
        public bool AdvanceToMenuOnStart = true;

        public GameState State { get; private set; } = GameState.Boot;
        public GameMode Mode => mode;
        public int Score { get; private set; }

        /// <summary>Selection consumed by the Game scene's LevelBootstrap.</summary>
        public GameMode PendingMode { get; private set; } = GameMode.Campaign;
        public int PendingLevelIndex { get; private set; }

        public event Action<GameState, GameState> StateChanged;
        public event Action<int> ScoreChanged;
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
            SaveService.Load();
            SteamServices.Init();
        }

        private void Update()
        {
            SteamServices.Current?.RunCallbacks();
        }

        private void Start()
        {
            if (AdvanceToMenuOnStart && State == GameState.Boot)
                GoToMainMenu();
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

        // -------------------------------------------------------- navigation
        public void GoToMainMenu()
        {
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(MainMenuScene);
        }

        public void StartCampaignLevel(int levelIndex)
        {
            mode = GameMode.Campaign;
            PendingMode = GameMode.Campaign;
            PendingLevelIndex = levelIndex;
            SetState(GameState.LevelLoading);
            SceneManager.LoadScene(GameScene);
        }

        public void StartEndless()
        {
            mode = GameMode.Endless;
            PendingMode = GameMode.Endless;
            SetState(GameState.LevelLoading);
            SceneManager.LoadScene(GameScene);
        }

        public void ReloadCurrent()
        {
            if (PendingMode == GameMode.Endless) StartEndless();
            else StartCampaignLevel(PendingLevelIndex);
        }
    }
}
