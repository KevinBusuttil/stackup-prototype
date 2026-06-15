using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// In-game HUD, built from code. Lists active orders (highlighting the
    /// selected one), tote contents, score / combo / rework, the interaction
    /// prompt, and transient verification feedback. See Section 18.2.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        private OrderManager orders;
        private GameManager game;
        private PlayerController player;

        private TextMeshProUGUI ordersText;
        private TextMeshProUGUI toteText;
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI promptText;
        private TextMeshProUGUI verifyText;

        private readonly StringBuilder sb = new StringBuilder();
        private float verifyTimer;

        public void Init(OrderManager orders, GameManager game, PlayerController player)
        {
            this.orders = orders;
            this.game = game;
            this.player = player;
            BuildUi();
            if (orders != null) orders.VerificationReported += OnVerification;
        }

        private void OnDestroy()
        {
            if (orders != null) orders.VerificationReported -= OnVerification;
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("HUD_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            ordersText = MakeText(canvas.transform, "Orders", new Vector2(24, -24), TextAlignmentOptions.TopLeft, 24, new Vector2(0, 1), new Vector2(620, 300));
            toteText = MakeText(canvas.transform, "Tote", new Vector2(24, 24), TextAlignmentOptions.BottomLeft, 22, new Vector2(0, 0), new Vector2(620, 120));
            scoreText = MakeText(canvas.transform, "Score", new Vector2(-24, -24), TextAlignmentOptions.TopRight, 26, new Vector2(1, 1), new Vector2(420, 140));
            promptText = MakeText(canvas.transform, "Prompt", new Vector2(0, 70), TextAlignmentOptions.Bottom, 30, new Vector2(0.5f, 0), new Vector2(900, 50));
            verifyText = MakeText(canvas.transform, "Verify", new Vector2(0, 150), TextAlignmentOptions.Center, 30, new Vector2(0.5f, 0.5f), new Vector2(900, 300));
            verifyText.text = "";
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 anchoredPos,
            TextAlignmentOptions align, float size, Vector2 anchor, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.alignment = align;
            t.fontSize = size;
            t.color = Color.white;

            var rt = t.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            return t;
        }

        private void OnVerification(VerificationResult r)
        {
            sb.Clear();
            sb.Append(r.Passed ? "<color=#5fd35f>VERIFIED</color>\n" : "<color=#e05555>VERIFICATION FAILED</color>\n");
            foreach (var l in r.Lines)
                sb.Append($"{l.SkuId}: {l.Collected}/{l.Required}{(l.Missing > 0 ? $"  (missing {l.Missing})" : "")}\n");
            if (r.WrongItems > 0) sb.Append($"Wrong items: {r.WrongItems}\n");
            if (!r.Passed) sb.Append("Rework: pick the missing items, stack, verify again.");
            verifyText.text = sb.ToString();
            verifyTimer = 3.5f;
        }

        private void Update()
        {
            if (orders == null) return;

            sb.Clear();
            var list = orders.ActiveOrders;
            if (list.Count == 0) sb.Append("No active orders");
            else
            {
                sb.Append(list.Count > 1 ? "Orders (Q / Tab to cycle):\n" : "Order:\n");
                for (int i = 0; i < list.Count; i++)
                {
                    var o = list[i];
                    bool sel = i == orders.SelectedIndex;
                    sb.Append(sel ? "> " : "   ");
                    sb.Append($"{o.OrderId} [{o.DockLaneId}] ");
                    if (orders.IsReadyToLoad(o)) sb.Append("READY → LOAD");
                    else if (o.State == OrderState.VerificationFailed) sb.Append("REWORK");
                    else sb.Append(o.State.ToString());
                    float sla = orders.GetSlaRemaining(o);
                    if (sla >= 0f) sb.Append($"  ⏱ {Mathf.CeilToInt(sla)}s");
                    sb.Append('\n');
                    foreach (var line in o.Lines)
                        sb.Append($"      {line.SkuId} {orders.Collected(o, line.SkuId)}/{line.Quantity}\n");
                }
            }
            ordersText.text = sb.ToString();

            if (player != null && player.Tote != null)
            {
                sb.Clear();
                sb.Append($"Tote {player.Tote.Inventory.UnitCount}/{player.Tote.Inventory.MaxUnits}");
                foreach (var kv in player.Tote.Inventory.Contents) sb.Append($"   {kv.Key}:{kv.Value}");
                toteText.text = sb.ToString();
            }

            int combo = orders.Score != null ? orders.Score.Combo : 0;
            int score = game != null ? game.Score : 0;
            string waveLine = orders.Endless ? $"\nWave: {orders.Wave}" : "";
            scoreText.text = $"Score: {score}\nCombo: x{combo}\nRework: {orders.ActiveReworkJobs}{waveLine}";

            var current = player != null && player.Interactor != null ? player.Interactor.Current : null;
            promptText.text = current != null ? $"[E] {current.GetPrompt()}" : "";

            if (verifyTimer > 0f)
            {
                verifyTimer -= Time.deltaTime;
                if (verifyTimer <= 0f) verifyText.text = "";
            }
        }
    }
}
