using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Re-tints an imported model's renderers with URP materials by matching the
    /// source material name. This gives the Blender models correct colours (and
    /// glowing screen/eye faces) under URP without any in-editor material
    /// conversion — so they never import pink. See docs/ART_PIPELINE.md.
    /// </summary>
    public static class ModelStyle
    {
        private static Shader lit;
        private static Shader Lit => lit != null ? lit
            : (lit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));

        public static void Apply(GameObject root)
        {
            if (root == null) return;
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                string n = r.sharedMaterial != null ? r.sharedMaterial.name.ToLowerInvariant() : "";
                Color col = Lookup(n, out bool emissive);
                var m = new Material(Lit) { color = col };
                if (emissive)
                {
                    m.EnableKeyword("_EMISSION");
                    m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    m.SetColor("_EmissionColor", col * 2.2f);
                }
                r.sharedMaterial = m;
            }
        }

        private static Color Lookup(string n, out bool emissive)
        {
            emissive = false;
            if (Has(n, "eye")) { emissive = true; return new Color(0.30f, 0.95f, 1f); }
            if (Has(n, "screen")) return new Color(0.05f, 0.07f, 0.09f);
            if (Has(n, "hivisyellow") || Has(n, "yellow") || Has(n, "paint")) return new Color(1f, 0.83f, 0.10f);
            if (Has(n, "hivisorange") || Has(n, "caporange") || Has(n, "orange")) return new Color(1f, 0.48f, 0.08f);
            if (Has(n, "hivisgreen") || Has(n, "zonegreen") || Has(n, "green")) return new Color(0.16f, 0.62f, 0.22f);
            if (Has(n, "forks")) return new Color(0.85f, 0.70f, 0.10f);
            if (Has(n, "hardhat") || Has(n, "zoneblue") || Has(n, "plastic")) return new Color(0.20f, 0.45f, 0.80f);
            if (Has(n, "goggles") || Has(n, "tablet") || Has(n, "dark") || Has(n, "tyre")) return new Color(0.12f, 0.12f, 0.14f);
            if (Has(n, "cap")) return new Color(0.20f, 0.20f, 0.24f);
            if (Has(n, "wood")) return new Color(0.60f, 0.45f, 0.25f);
            if (Has(n, "card")) return new Color(0.78f, 0.62f, 0.40f);
            if (Has(n, "tape")) return new Color(0.85f, 0.78f, 0.55f);
            if (Has(n, "metal")) return new Color(0.30f, 0.34f, 0.45f);
            if (Has(n, "glass")) return new Color(0.45f, 0.75f, 0.85f);
            if (Has(n, "white")) return new Color(0.92f, 0.92f, 0.92f);
            if (Has(n, "wall") || Has(n, "floor")) return new Color(0.60f, 0.61f, 0.64f);
            if (Has(n, "body")) return new Color(0.82f, 0.84f, 0.88f);
            return new Color(0.75f, 0.76f, 0.80f);
        }

        private static bool Has(string n, string key) => n.Contains(key);
    }
}
