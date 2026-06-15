using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// Level result overlay, built from code. Offers Next (campaign), Retry, and
    /// Menu. Keyboard/gamepad: Enter/Space = primary, Esc = menu.
    /// See CLAUDE_CODE_SPEC.md Sections 13.1 / 18.1.
    /// </summary>
    public class ResultScreen : MonoBehaviour
    {
        private GameObject panel;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI detailText;
        private GameObject nextButton;
        private GameObject retryButton;

        private bool visible;
        private bool canNext;
        private int nextIndex;

        public void Init()
        {
            BuildUi();
            Hide();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("Result_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            UiKit.ApplyScale(scaler);
            canvasGo.AddComponent<GraphicRaycaster>();

            panel = new GameObject("Panel");
            panel.transform.SetParent(canvas.transform, false);
            var img = panel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.80f);
            var prt = img.rectTransform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            titleText = Text(panel.transform, "Title", new Vector2(0, 150), 60, "");
            detailText = Text(panel.transform, "Detail", new Vector2(0, 40), 34, "");

            nextButton = MakeButton(panel.transform, "Next", new Vector2(0, -70), new Color(0.20f, 0.65f, 0.30f), "Next ▶", OnNext);
            retryButton = MakeButton(panel.transform, "Retry", new Vector2(0, -150), new Color(0.20f, 0.50f, 0.90f), "Retry", OnRetry);
            MakeButton(panel.transform, "Menu", new Vector2(0, -230), new Color(0.45f, 0.45f, 0.50f), "Main Menu", OnMenu);
        }

        private static TextMeshProUGUI Text(Transform parent, string name, Vector2 pos, float size, string value)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = size;
            t.color = Color.white;
            t.text = value;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900, 160);
            rt.anchoredPosition = pos;
            return t;
        }

        private static GameObject MakeButton(Transform parent, string name, Vector2 pos, Color color, string label, Action onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300, 64);
            rt.anchoredPosition = pos;

            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => onClick());

            var label2 = Text(go.transform, "Label", Vector2.zero, 28, label);
            label2.rectTransform.sizeDelta = new Vector2(300, 64);
            return go;
        }

        public void Show(string title, string message, bool canNext, int nextLevelIndex)
        {
            this.canNext = canNext;
            nextIndex = nextLevelIndex;
            if (titleText != null) titleText.text = title;
            if (detailText != null) detailText.text = message;
            if (nextButton != null) nextButton.SetActive(canNext);
            if (panel != null) panel.SetActive(true);
            visible = true;
            UiKit.SelectFirst(canNext ? nextButton : retryButton);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
            visible = false;
        }

        private void OnNext()
        {
            if (canNext && GameManager.Instance != null) GameManager.Instance.StartCampaignLevel(nextIndex);
        }

        private void OnRetry() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        private void OnMenu()
        {
            if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
            else SceneManager.LoadScene(GameManager.MainMenuScene);
        }

        private void Update()
        {
            if (!visible) return;
            var kb = Keyboard.current;
            var pad = Gamepad.current;

            bool primary = (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                           || (pad != null && pad.buttonSouth.wasPressedThisFrame);
            bool menu = (kb != null && kb.escapeKey.wasPressedThisFrame)
                        || (pad != null && pad.buttonEast.wasPressedThisFrame);

            if (menu) OnMenu();
            else if (primary) { if (canNext) OnNext(); else OnRetry(); }
        }
    }
}
