using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// In-game pause overlay (Esc / Start): Resume, Settings, Main Menu. Freezes
    /// gameplay via Time.timeScale and is controller-navigable. See Section 18.1.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private GameObject panel;
        private GameObject firstButton;
        private SettingsMenu settings;
        private bool paused;

        public void Init()
        {
            var canvasGo = new GameObject("Pause_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 15;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            UiKit.ApplyScale(scaler);
            canvasGo.AddComponent<GraphicRaycaster>();

            panel = new GameObject("Panel");
            panel.transform.SetParent(canvas.transform, false);
            var img = panel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.7f);
            var prt = img.rectTransform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            Label(panel.transform, new Vector2(0, 150), 60, "Paused");
            firstButton = MakeButton(panel.transform, new Vector2(0, 30), new Color(0.20f, 0.55f, 0.90f), "Resume", Resume);
            MakeButton(panel.transform, new Vector2(0, -50), new Color(0.45f, 0.45f, 0.55f), "Settings", OpenSettings);
            MakeButton(panel.transform, new Vector2(0, -130), new Color(0.5f, 0.4f, 0.4f), "Main Menu", GoMenu);

            settings = new GameObject("PauseSettings").AddComponent<SettingsMenu>();
            settings.transform.SetParent(transform, false);
            settings.Init();

            panel.SetActive(false);
        }

        private void Update()
        {
            if (TogglePressed()) { if (paused) Resume(); else Pause(); }
        }

        private static bool TogglePressed()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) return true;
            var pad = Gamepad.current;
            if (pad != null && pad.startButton.wasPressedThisFrame) return true;
            return false;
        }

        private void Pause()
        {
            paused = true;
            Time.timeScale = 0f;
            panel.SetActive(true);
            GameManager.Instance?.SetState(GameState.Paused);
            UiKit.SelectFirst(firstButton);
        }

        private void Resume()
        {
            paused = false;
            Time.timeScale = 1f;
            panel.SetActive(false);
            GameManager.Instance?.SetState(GameState.Running);
        }

        private void OpenSettings()
        {
            panel.SetActive(false);
            settings.Open(() => { panel.SetActive(true); UiKit.SelectFirst(firstButton); });
        }

        private void GoMenu()
        {
            Time.timeScale = 1f;
            GameManager.Instance?.GoToMainMenu();
        }

        // ---- builders ----
        private static TextMeshProUGUI Label(Transform parent, Vector2 pos, float size, string value)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = size; t.color = Color.white; t.text = value;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 100); rt.anchoredPosition = pos;
            return t;
        }

        private static GameObject MakeButton(Transform parent, Vector2 pos, Color color, string label, Action onClick)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300, 64); rt.anchoredPosition = pos;
            go.AddComponent<Button>().onClick.AddListener(() => onClick());
            var l = Label(go.transform, Vector2.zero, 28, label);
            l.rectTransform.sizeDelta = new Vector2(300, 64);
            return go;
        }
    }
}
