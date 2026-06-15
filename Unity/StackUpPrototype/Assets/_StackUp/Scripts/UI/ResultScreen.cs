using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// Level result overlay, built from code. Shown when the order completes;
    /// "Play Again" reloads the level. See CLAUDE_CODE_SPEC.md Sections 13.1 / 18.1.
    /// </summary>
    public class ResultScreen : MonoBehaviour
    {
        private GameObject panel;
        private TextMeshProUGUI detail;

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
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            panel = new GameObject("Panel");
            panel.transform.SetParent(canvas.transform, false);
            var img = panel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.78f);
            var prt = img.rectTransform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            MakeText(panel.transform, "Title", new Vector2(0, 90), 64, "Level Complete!");
            detail = MakeText(panel.transform, "Detail", new Vector2(0, 0), 34, "");

            var btnGo = new GameObject("PlayAgain");
            btnGo.transform.SetParent(panel.transform, false);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.20f, 0.50f, 0.90f, 1f);
            var brt = btnImg.rectTransform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(280, 76);
            brt.anchoredPosition = new Vector2(0, -110);

            var btn = btnGo.AddComponent<Button>();
            btn.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));

            var label = MakeText(btnGo.transform, "Label", Vector2.zero, 30, "Play Again");
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, Vector2 pos, float size, string value)
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
            rt.sizeDelta = new Vector2(900, 140);
            rt.anchoredPosition = pos;
            return t;
        }

        public void Show(string message)
        {
            if (detail != null) detail.text = message;
            if (panel != null) panel.SetActive(true);
        }

        public void Hide()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
