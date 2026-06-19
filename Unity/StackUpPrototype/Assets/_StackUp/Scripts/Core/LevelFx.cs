using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Translates gameplay events into audio cues and feedback popups. Like
    /// SteamTelemetry, it keeps presentation out of the gameplay systems —
    /// they only raise plain events. See CLAUDE_CODE_SPEC.md Sections 18 / 22.
    /// </summary>
    public class LevelFx : MonoBehaviour
    {
        private static readonly Color Good = new Color(0.45f, 0.9f, 0.5f);
        private static readonly Color Bad = new Color(0.95f, 0.45f, 0.45f);

        private OrderManager orders;
        private ScoreSystem score;
        private Tote tote;
        private FeedbackSystem feedback;
        private int lastToteCount;

        public void Init(OrderManager orders, ScoreSystem score, Tote tote, FeedbackSystem feedback)
        {
            this.orders = orders;
            this.score = score;
            this.tote = tote;
            this.feedback = feedback;
            lastToteCount = tote != null ? tote.Inventory.UnitCount : 0;

            if (tote != null) tote.Changed += OnToteChanged;
            if (score != null)
            {
                score.OrderScored += OnOrderScored;
                score.WrongPicked += OnWrongPicked;
                score.WrongDocked += OnWrongDocked;
                score.IllegalStacked += OnIllegalStacked;
                score.FailedVerified += OnFailedVerified;
                score.SlaMissed += OnSlaMissed;
            }
            if (orders != null) orders.VerificationReported += OnVerification;
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
                score.SlaMissed -= OnSlaMissed;
            }
            if (orders != null) orders.VerificationReported -= OnVerification;
        }

        private void OnToteChanged()
        {
            int c = tote != null ? tote.Inventory.UnitCount : 0;
            if (c > lastToteCount) AudioManager.Play(Sfx.Pick);
            else if (c < lastToteCount) AudioManager.Play(Sfx.Place);
            lastToteCount = c;
        }

        private void OnOrderScored(bool perfect)
        {
            AudioManager.Play(Sfx.OrderComplete);
            feedback?.Popup(perfect ? "PERFECT!" : "Order loaded", Good);
        }

        private void OnWrongPicked() { AudioManager.Play(Sfx.Warning); feedback?.Popup("Wrong pick!", Bad); }
        private void OnWrongDocked() { AudioManager.Play(Sfx.Warning); feedback?.Popup("Wrong dock!", Bad); }
        private void OnIllegalStacked() { AudioManager.Play(Sfx.Warning); feedback?.Popup("Can't stack there!", Bad); }
        // Sound for a failed verify is played by OnVerification; this only shows the popup.
        private void OnFailedVerified() => feedback?.Popup("Verification failed", Bad);
        private void OnSlaMissed() { AudioManager.Play(Sfx.Warning); feedback?.Popup("Missed SLA!", Bad); }

        private void OnVerification(VerificationResult r)
        {
            AudioManager.Play(r.Passed ? Sfx.VerifyPass : Sfx.VerifyFail);
            if (r.Passed) feedback?.Popup("VERIFIED", Good);
        }
    }
}
