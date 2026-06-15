using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace StackUp
{
    /// <summary>
    /// Scenery that fades out when it blocks the camera's view of the player.
    /// Caches its opaque materials and swaps to transparent copies while faded,
    /// restoring the originals once clear. See CLAUDE_CODE_SPEC.md Section 8.
    /// </summary>
    public class FadeableObject : MonoBehaviour
    {
        public float FadedAlpha = 0.3f;
        public float FadeSpeed = 6f;

        private readonly List<Renderer> renderers = new List<Renderer>();
        private readonly List<Material> opaque = new List<Material>();
        private readonly List<Material> transparent = new List<Material>();

        private float current = 1f;
        private float target = 1f;
        private bool transparentApplied;

        private void Awake()
        {
            foreach (var r in GetComponentsInChildren<Renderer>())
            {
                var src = r.sharedMaterial;
                if (src == null) continue;
                renderers.Add(r);
                opaque.Add(src);

                var t = new Material(src);
                MakeTransparent(t);
                transparent.Add(t);
            }
        }

        public void SetOccluding(bool occluding)
        {
            target = occluding ? FadedAlpha : 1f;
        }

        private void Update()
        {
            if (Mathf.Approximately(current, target)) return;
            current = Mathf.MoveTowards(current, target, FadeSpeed * Time.deltaTime);
            Apply(current);
        }

        private void Apply(float alpha)
        {
            if (alpha >= 0.999f)
            {
                if (!transparentApplied) return;
                for (int i = 0; i < renderers.Count; i++) renderers[i].sharedMaterial = opaque[i];
                transparentApplied = false;
                return;
            }

            for (int i = 0; i < renderers.Count; i++)
            {
                var m = transparent[i];
                Color c = m.color;
                c.a = alpha;
                m.color = c;
                renderers[i].sharedMaterial = m;
            }
            transparentApplied = true;
        }

        private void OnDestroy()
        {
            foreach (var m in transparent) if (m != null) Destroy(m);
        }

        /// <summary>Reconfigures a URP/Lit material instance for alpha-blended transparency.</summary>
        private static void MakeTransparent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
