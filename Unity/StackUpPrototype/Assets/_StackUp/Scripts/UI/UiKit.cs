using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace StackUp
{
    /// <summary>
    /// Shared UI helpers: an EventSystem wired for mouse + keyboard + controller,
    /// canvas scaling honouring the player's UI-scale setting (Steam Deck
    /// readability), and a select-first helper for controller-first navigation.
    /// See CLAUDE_CODE_SPEC.md Sections 18.3 / 19.
    /// </summary>
    public static class UiKit
    {
        public const float BaseWidth = 1280f;
        public const float BaseHeight = 720f;

        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            // Default actions give pointer + navigate + submit/cancel for all devices.
            go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        /// <summary>Configures a scaler so a larger UI-scale setting enlarges the UI.</summary>
        public static void ApplyScale(CanvasScaler scaler)
        {
            float s = Mathf.Clamp(SaveService.Settings.UiScale, 1f, 1.6f);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(BaseWidth / s, BaseHeight / s);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        public static void SelectFirst(GameObject go)
        {
            if (go != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(go);
        }
    }
}
