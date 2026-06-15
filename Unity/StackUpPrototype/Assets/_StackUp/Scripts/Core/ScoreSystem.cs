using System;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Scoring + combo + penalties (Sections 17 / 28). Owns the combo state and
    /// applies score deltas through the GameManager so total score has one home.
    /// </summary>
    public class ScoreSystem : MonoBehaviour
    {
        [Header("Rewards")]
        public int BasePerOrder = 100;
        public int PerfectBonus = 50;
        public int ComboStep = 25;

        [Header("Penalties")]
        public int WrongPickPenalty = 15;
        public int WrongDockPenalty = 25;
        public int FailedVerificationPenalty = 20;
        public int IllegalStackPenalty = 10;
        public int MissedSlaPenalty = 40;

        public int Combo { get; private set; }
        public int BestCombo { get; private set; }

        public event Action Changed;

        // Telemetry signals (gameplay raises these; SteamTelemetry listens).
        public event Action<bool> OrderScored;   // perfect?
        public event Action WrongPicked;
        public event Action WrongDocked;
        public event Action IllegalStacked;
        public event Action FailedVerified;
        public event Action SlaMissed;

        private GameManager game;

        public void Init(GameManager game) => this.game = game;

        private void Apply(int delta)
        {
            game?.AddScore(delta);
            Changed?.Invoke();
        }

        public void CompleteOrder(bool perfect, float timeBonus)
        {
            int comboBonus = Combo * ComboStep;
            int total = BasePerOrder + comboBonus + Mathf.RoundToInt(Mathf.Max(0f, timeBonus)) + (perfect ? PerfectBonus : 0);
            Apply(total);

            if (perfect)
            {
                Combo++;
                if (Combo > BestCombo) BestCombo = Combo;
            }
            else
            {
                Combo = 0;
            }
            OrderScored?.Invoke(perfect);
        }

        public void WrongPick() { Combo = 0; Apply(-WrongPickPenalty); WrongPicked?.Invoke(); }
        public void WrongDock() { Combo = 0; Apply(-WrongDockPenalty); WrongDocked?.Invoke(); }
        public void FailedVerification() { Combo = 0; Apply(-FailedVerificationPenalty); FailedVerified?.Invoke(); }
        public void IllegalStack() { Apply(-IllegalStackPenalty); IllegalStacked?.Invoke(); }
        public void MissedSla() { Combo = 0; Apply(-MissedSlaPenalty); SlaMissed?.Invoke(); }
    }
}
