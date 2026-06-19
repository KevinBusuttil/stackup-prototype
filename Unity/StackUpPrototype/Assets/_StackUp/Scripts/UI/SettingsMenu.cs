using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// Settings overlay: UI scale (Steam Deck readability) and volumes. Built from
    /// code, controller-navigable, persisted via SaveService. See Sections 18.3 / 22.
    /// </summary>
    public class SettingsMenu : MonoBehaviour
    {
        private GameObject panel;
        private CanvasScaler scaler;
        private Action onClosed;
        private GameObject firstSelectable;

        private TextMeshProUGUI uiScaleVal, masterVal, musicVal, sfxVal;

        public void Init()
        {
            var canvasGo = new GameObject("Settings_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            scaler = canvasGo.AddComponent<CanvasScaler>();
            UiKit.ApplyScale(scaler);
            canvasGo.AddComponent<GraphicRaycaster>();

            panel = new GameObject("Panel");
            panel.transform.SetParent(canvas.transform, false);
            var img = panel.AddComponent<Image>();
            img.color = new Color(0.08f, 0.10f, 0.14f, 0.97f);
            var prt = img.rectTransform;
            prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
            prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;

            Label(panel.transform, new Vector2(0, 240), 52, "Settings");

            firstSelectable = Row(panel.transform, 130, "UI Scale", out uiScaleVal,
                () => AdjustUiScale(-0.2f), () => AdjustUiScale(0.2f));
            Row(panel.transform, 50, "Master Volume", out masterVal,
                () => AdjustMaster(-0.1f), () => AdjustMaster(0.1f));
            Row(panel.transform, -30, "Music Volume", out musicVal,
                () => AdjustMusic(-0.1f), () => AdjustMusic(0.1f));
            Row(panel.transform, -110, "SFX Volume", out sfxVal,
                () => AdjustSfx(-0.1f), () => AdjustSfx(0.1f));

            MakeButton(panel.transform, new Vector2(0, -210), new Vector2(260, 64), new Color(0.45f, 0.45f, 0.50f), "Back", Close);

            panel.SetActive(false);
        }

        public void Open(Action onClosed)
        {
            this.onClosed = onClosed;
            Refresh();
            panel.SetActive(true);
            UiKit.SelectFirst(firstSelectable);
        }

        private void Close()
        {
            SaveService.SaveSettings();
            panel.SetActive(false);
            onClosed?.Invoke();
        }

        private void AdjustUiScale(float d)
        {
            SaveService.Settings.UiScale = Mathf.Clamp(Mathf.Round((SaveService.Settings.UiScale + d) * 10f) / 10f, 1f, 1.6f);
            UiKit.ApplyScale(scaler); // live preview on this overlay
            Refresh();
        }

        private void AdjustMaster(float d) { SaveService.Settings.MasterVolume = Clamp01(SaveService.Settings.MasterVolume + d); AudioManager.RefreshMix(); AudioManager.Play(Sfx.Scan); Refresh(); }
        private void AdjustMusic(float d) { SaveService.Settings.MusicVolume = Clamp01(SaveService.Settings.MusicVolume + d); AudioManager.RefreshMix(); Refresh(); }
        private void AdjustSfx(float d) { SaveService.Settings.SfxVolume = Clamp01(SaveService.Settings.SfxVolume + d); AudioManager.Play(Sfx.Scan); Refresh(); }

        private static float Clamp01(float v) => Mathf.Clamp01(Mathf.Round(v * 10f) / 10f);

        private void Refresh()
        {
            var s = SaveService.Settings;
            if (uiScaleVal != null) uiScaleVal.text = $"{Mathf.RoundToInt(s.UiScale * 100f)}%";
            if (masterVal != null) masterVal.text = $"{Mathf.RoundToInt(s.MasterVolume * 100f)}%";
            if (musicVal != null) musicVal.text = $"{Mathf.RoundToInt(s.MusicVolume * 100f)}%";
            if (sfxVal != null) sfxVal.text = $"{Mathf.RoundToInt(s.SfxVolume * 100f)}%";
        }

        // ---- builders ----
        private GameObject Row(Transform parent, float y, string label, out TextMeshProUGUI valueText, Action onMinus, Action onPlus)
        {
            Label(parent, new Vector2(-260, y), 28, label);
            var minus = MakeButton(parent, new Vector2(-40, y), new Vector2(56, 56), new Color(0.3f, 0.4f, 0.6f), "-", onMinus);
            valueText = Label(parent, new Vector2(70, y), 28, "");
            MakeButton(parent, new Vector2(180, y), new Vector2(56, 56), new Color(0.3f, 0.4f, 0.6f), "+", onPlus);
            return minus; // first focus target of the row
        }

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
            rt.sizeDelta = new Vector2(320, 60);
            rt.anchoredPosition = pos;
            return t;
        }

        private static GameObject MakeButton(Transform parent, Vector2 pos, Vector2 size, Color color, string label, Action onClick)
        {
            var go = new GameObject("Button");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size; rt.anchoredPosition = pos;

            go.AddComponent<Button>().onClick.AddListener(() => onClick());
            var l = Label(go.transform, Vector2.zero, 28, label);
            l.rectTransform.sizeDelta = size;
            return go;
        }
    }
}
