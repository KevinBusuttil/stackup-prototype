using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// Main menu + level select, built from code. Campaign levels show their
    /// unlock state and best score from <see cref="SaveService"/>.
    /// See CLAUDE_CODE_SPEC.md Sections 18.1 / 24 (M3).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private GameObject mainPanel;
        private GameObject selectPanel;
        private GameObject firstButton;
        private SettingsMenu settings;

        private void Start()
        {
            EnsureGameManager();
            UiKit.EnsureEventSystem();
            EnsureCamera();
            AudioManager.Ensure();
            BuildUi();
            ShowMain();
        }

        private static void EnsureGameManager()
        {
            if (GameManager.Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>().AdvanceToMenuOnStart = false;
        }

        private static void EnsureCamera()
        {
            if (Camera.main != null) return;
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.AddComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            go.GetComponent<Camera>().backgroundColor = new Color(0.10f, 0.12f, 0.16f);
            go.AddComponent<AudioListener>();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("Menu_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            UiKit.ApplyScale(scaler);
            canvasGo.AddComponent<GraphicRaycaster>();

            settings = new GameObject("SettingsMenu").AddComponent<SettingsMenu>();
            settings.transform.SetParent(transform, false);
            settings.Init();

            mainPanel = Panel(canvas.transform, "MainPanel");
            Text(mainPanel.transform, "Title", new Vector2(0, 220), 72, "STACK UP!");
            Text(mainPanel.transform, "Sub", new Vector2(0, 150), 28, "Warehouse Operations");
            firstButton = MakeButton(mainPanel.transform, "Campaign", new Vector2(0, 40), new Color(0.20f, 0.55f, 0.90f), "Campaign", ShowSelect);
            MakeButton(mainPanel.transform, "Endless", new Vector2(0, -40), new Color(0.20f, 0.65f, 0.30f), "Endless", () => GameManager.Instance.StartEndless());
            MakeButton(mainPanel.transform, "Settings", new Vector2(0, -120), new Color(0.45f, 0.45f, 0.55f), "Settings", OpenSettings);
            MakeButton(mainPanel.transform, "Quit", new Vector2(0, -200), new Color(0.5f, 0.4f, 0.4f), "Quit", Quit);

            selectPanel = Panel(canvas.transform, "SelectPanel");
            Text(selectPanel.transform, "SelTitle", new Vector2(0, 280), 48, "Select Level");
            BuildLevelButtons(selectPanel.transform);
            MakeButton(selectPanel.transform, "Back", new Vector2(0, -300), new Color(0.45f, 0.45f, 0.50f), "Back", ShowMain);
        }

        private void BuildLevelButtons(Transform parent)
        {
            int count = LevelLibrary.CampaignCount;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                var cfg = LevelLibrary.Get(index);
                bool unlocked = SaveService.IsUnlocked(index);
                int best = SaveService.BestScore(index);

                // two columns of four
                float x = (i % 2 == 0) ? -200f : 200f;
                float y = 200f - (i / 2) * 90f;

                string label = unlocked
                    ? $"{index + 1}. {cfg.Name}{(best > 0 ? $"  ({best})" : "")}"
                    : $"{index + 1}. Locked";
                Color col = unlocked ? new Color(0.22f, 0.42f, 0.70f) : new Color(0.30f, 0.30f, 0.33f);

                var btn = MakeButton(parent, $"Lvl{index}", new Vector2(x, y), col, label,
                    () => { if (unlocked) GameManager.Instance.StartCampaignLevel(index); });
                btn.GetComponent<Button>().interactable = unlocked;
            }
        }

        private void ShowMain() { mainPanel.SetActive(true); selectPanel.SetActive(false); UiKit.SelectFirst(firstButton); }
        private void ShowSelect() { mainPanel.SetActive(false); selectPanel.SetActive(true); }

        private void OpenSettings()
        {
            mainPanel.SetActive(false);
            settings.Open(ShowMain);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---- tiny UI builders ----
        private static GameObject Panel(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return go;
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
            rt.sizeDelta = new Vector2(900, 120);
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
            rt.sizeDelta = new Vector2(360, 64);
            rt.anchoredPosition = pos;

            go.AddComponent<Button>().onClick.AddListener(() => onClick());

            var label2 = Text(go.transform, "Label", Vector2.zero, 26, label);
            label2.rectTransform.sizeDelta = new Vector2(360, 64);
            return go;
        }
    }
}
