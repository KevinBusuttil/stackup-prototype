using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// Minimal in-game HUD, built entirely from code so the slice needs no
    /// authored Canvas. Shows the active job, tote contents, score, and the
    /// current interaction prompt. See CLAUDE_CODE_SPEC.md Section 18.2.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        private OrderManager orders;
        private GameManager game;
        private PlayerController player;

        private TextMeshProUGUI jobText;
        private TextMeshProUGUI toteText;
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI promptText;
        private readonly StringBuilder sb = new StringBuilder();

        public void Init(OrderManager orders, GameManager game, PlayerController player)
        {
            this.orders = orders;
            this.game = game;
            this.player = player;
            BuildUi();
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

            jobText = MakeText(canvas.transform, "Job", new Vector2(24, -24), TextAlignmentOptions.TopLeft, 28, new Vector2(0, 1));
            toteText = MakeText(canvas.transform, "Tote", new Vector2(24, -84), TextAlignmentOptions.TopLeft, 22, new Vector2(0, 1));
            scoreText = MakeText(canvas.transform, "Score", new Vector2(-24, -24), TextAlignmentOptions.TopRight, 28, new Vector2(1, 1));
            promptText = MakeText(canvas.transform, "Prompt", new Vector2(0, 64), TextAlignmentOptions.Bottom, 30, new Vector2(0.5f, 0));
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 anchoredPos,
            TextAlignmentOptions align, float size, Vector2 anchor)
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
            rt.sizeDelta = new Vector2(640, 60);
            rt.anchoredPosition = anchoredPos;
            return t;
        }

        private void Update()
        {
            if (orders == null) return;
            var order = orders.ActiveOrder;

            sb.Clear();
            if (order == null) sb.Append("No active order");
            else if (orders.IsReadyToLoad()) sb.Append($"Order {order.OrderId}:  LOAD at {order.DockLaneId}");
            else
            {
                sb.Append($"Order {order.OrderId} — Pick:  ");
                bool first = true;
                foreach (var line in order.Lines)
                {
                    if (!first) sb.Append(",  ");
                    sb.Append($"{line.SkuId} {orders.CollectedQuantity(line.SkuId)}/{line.Quantity}");
                    first = false;
                }
            }
            jobText.text = sb.ToString();

            if (player != null && player.Tote != null)
                toteText.text = $"Tote: {player.Tote.Inventory.UnitCount}/{player.Tote.Inventory.MaxUnits}";

            if (game != null) scoreText.text = $"Score: {game.Score}";

            var current = player != null && player.Interactor != null ? player.Interactor.Current : null;
            promptText.text = current != null ? $"[E] {current.GetPrompt()}" : "";
        }
    }
}
