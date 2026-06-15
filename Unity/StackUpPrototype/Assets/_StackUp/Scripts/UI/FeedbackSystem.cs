using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// Pooled floating-text popups for feedback ("PERFECT!", "Wrong dock", …).
    /// Object-pooled to avoid per-event allocations. See CLAUDE_CODE_SPEC.md
    /// Sections 18 / 23 (#48 feedback, #50 pooling).
    /// </summary>
    public class FeedbackSystem : MonoBehaviour
    {
        private RectTransform root;
        private readonly Stack<TextMeshProUGUI> pool = new Stack<TextMeshProUGUI>();

        public void Init()
        {
            var canvasGo = new GameObject("Feedback_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            UiKit.ApplyScale(scaler);
            root = canvas.GetComponent<RectTransform>();
        }

        public void Popup(string text, Color color)
        {
            if (root == null) return;
            var t = Rent();
            t.text = text;
            t.color = color;
            var rt = t.rectTransform;
            rt.anchoredPosition = new Vector2(Random.Range(-80f, 80f), 80f);
            t.gameObject.SetActive(true);
            StartCoroutine(Animate(t));
        }

        private IEnumerator Animate(TextMeshProUGUI t)
        {
            var rt = t.rectTransform;
            float life = 0.9f;
            float elapsed = 0f;
            Color start = t.color;
            Vector2 from = rt.anchoredPosition;
            while (elapsed < life)
            {
                elapsed += Time.deltaTime;
                float k = elapsed / life;
                rt.anchoredPosition = from + new Vector2(0f, 90f * k);
                Color c = start; c.a = 1f - k;
                t.color = c;
                yield return null;
            }
            t.gameObject.SetActive(false);
            pool.Push(t);
        }

        private TextMeshProUGUI Rent()
        {
            if (pool.Count > 0) return pool.Pop();
            var go = new GameObject("Popup");
            go.transform.SetParent(root, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = 40;
            t.fontStyle = FontStyles.Bold;
            var rt = t.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 80);
            return t;
        }
    }
}
