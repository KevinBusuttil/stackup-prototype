using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Procedural "set dressing" + a shared toy-warehouse colour palette. These
    /// builders make the placeholder scene read as a charming low-poly warehouse
    /// even when no imported art prefabs are present (see
    /// docs/VISUAL_VERTICAL_SLICE_PLAN.md). Everything here is purely visual —
    /// the builders never add gameplay colliders, and the gameplay primitives
    /// keep their own colliders. When a real prefab exists in
    /// Resources/StackUpArt it is used instead (see PrefabLibrary / AttachVisual).
    /// </summary>
    public static class ArtKit
    {
        // ---- Toy palette (bright, readable, warehouse-themed) -------------
        public static readonly Color Floor        = new Color(0.80f, 0.82f, 0.85f);
        public static readonly Color FloorAisle   = new Color(0.88f, 0.89f, 0.92f);
        public static readonly Color AisleLine    = new Color(0.97f, 0.85f, 0.30f);
        public static readonly Color RackMetal    = new Color(0.27f, 0.45f, 0.66f);
        public static readonly Color RackBeam     = new Color(0.96f, 0.62f, 0.14f);
        public static readonly Color PlayerBlue   = new Color(0.27f, 0.74f, 0.92f);
        public static readonly Color DockGreen     = new Color(0.24f, 0.80f, 0.42f);
        public static readonly Color VerifyAmber   = new Color(0.98f, 0.74f, 0.20f);
        public static readonly Color Hazard        = new Color(0.13f, 0.13f, 0.15f);
        public static readonly Color ZoneReceiving = new Color(0.32f, 0.58f, 0.95f);
        public static readonly Color ZoneStaging   = new Color(0.96f, 0.80f, 0.26f);

        // robot fallback colours
        public static readonly Color BodyLight = new Color(0.86f, 0.88f, 0.91f);
        public static readonly Color HiVis     = new Color(0.98f, 0.80f, 0.10f);
        public static readonly Color CapDark   = new Color(0.22f, 0.22f, 0.26f);
        public static readonly Color DarkPart  = new Color(0.12f, 0.12f, 0.14f);
        public static readonly Color ScreenCyan = new Color(0.32f, 0.95f, 1f);

        // building shell / props
        public static readonly Color WallMain  = new Color(0.87f, 0.88f, 0.91f);
        public static readonly Color WallTrim  = new Color(0.27f, 0.48f, 0.70f);
        public static readonly Color WallBase  = new Color(0.40f, 0.43f, 0.49f);
        public static readonly Color Girder    = new Color(0.33f, 0.36f, 0.43f);
        public static readonly Color WindowLit = new Color(0.72f, 0.86f, 1f);
        public static readonly Color Crate     = new Color(0.78f, 0.60f, 0.36f);
        public static readonly Color BollardY  = new Color(0.97f, 0.78f, 0.12f);
        public static readonly Color ConeOrange = new Color(0.98f, 0.42f, 0.10f);

        /// <summary>Varied stock-box colours for decorative (non-gameplay) rack bays.</summary>
        public static readonly Color[] StockPalette =
        {
            new Color(0.90f, 0.55f, 0.20f), new Color(0.40f, 0.80f, 0.95f),
            new Color(0.70f, 0.72f, 0.78f), new Color(0.80f, 0.72f, 0.45f),
            new Color(0.55f, 0.78f, 0.40f), new Color(0.86f, 0.42f, 0.46f),
        };

        private static Shader litShader;
        private static Shader Lit => litShader != null ? litShader
            : (litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));

        // Cache opaque materials by colour so the (now much larger) static scene
        // shares a handful of materials instead of one per box. Safe because
        // FadeableObject fades per-renderer transparent *copies*, never these.
        private static readonly Dictionary<Color, Material> matCache = new Dictionary<Color, Material>();

        public static Material Mat(Color c)
        {
            if (!matCache.TryGetValue(c, out var m) || m == null)
            {
                m = new Material(Lit) { color = c };
                matCache[c] = m;
            }
            return m;
        }

        public static Material Emissive(Color c, float intensity = 1.8f)
        {
            var m = Mat(c);
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            m.SetColor("_EmissionColor", c * intensity);
            return m;
        }

        private static float Inv(float v) => Mathf.Approximately(v, 0f) ? 1f : 1f / v;

        /// <summary>
        /// Creates a child whose scale cancels the host's (often non-uniform) scale
        /// and whose origin sits on the floor — the same convention as
        /// <c>AttachVisual</c>, so procedural fallbacks and imported prefabs line up.
        /// </summary>
        public static Transform ScaledRoot(GameObject host, float centerHeight, string name = "ArtVisual")
        {
            var root = new GameObject(name).transform;
            root.SetParent(host.transform, false);
            Vector3 hs = host.transform.localScale;
            root.localScale = new Vector3(Inv(hs.x), Inv(hs.y), Inv(hs.z));
            root.localPosition = new Vector3(0f, -centerHeight * Inv(hs.y), 0f);
            root.localRotation = Quaternion.identity;
            return root;
        }

        /// <summary>A coloured box, in metres, parented under <paramref name="parent"/>. Collider stripped unless <paramref name="keepCollider"/>.</summary>
        public static GameObject Box(Transform parent, string name, Vector3 size, Vector3 localPos, Material mat, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localScale = size;
            go.transform.localPosition = localPos;
            if (!keepCollider)
            {
                var col = go.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);
            }
            var r = go.GetComponent<Renderer>();
            if (r != null && mat != null) r.sharedMaterial = mat;
            return go;
        }

        // ------------------------------------------------------------ builders

        /// <summary>A low-poly pallet-rack bay: uprights, orange beams, two shelves and a stock box.</summary>
        public static void BuildRackFrame(Transform root, Color stockColor)
        {
            const float w = 1.25f, d = 1.05f, h = 1.7f, t = 0.08f;
            var metal = Mat(RackMetal);
            var beam = Mat(RackBeam);

            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    Box(root, "Upright", new Vector3(t, h, t),
                        new Vector3(sx * (w / 2 - t), h / 2, sz * (d / 2 - t)), metal);

            foreach (float y in new[] { 0.45f, 1.05f, 1.62f })
            {
                Box(root, "BeamFront", new Vector3(w, t * 1.3f, t), new Vector3(0f, y, -(d / 2 - t)), beam);
                Box(root, "BeamBack", new Vector3(w, t * 1.3f, t), new Vector3(0f, y, d / 2 - t), beam);
            }

            Box(root, "Shelf0", new Vector3(w - 2 * t, t, d - 2 * t), new Vector3(0f, 0.45f, 0f), metal);
            Box(root, "Shelf1", new Vector3(w - 2 * t, t, d - 2 * t), new Vector3(0f, 1.05f, 0f), metal);
            Box(root, "Stock", new Vector3(0.8f, 0.55f, 0.72f), new Vector3(0f, 0.78f, 0f), Mat(stockColor));
        }

        /// <summary>A painted dock pad with hazard stripes and a bright border.</summary>
        public static void BuildDockPad(Transform root)
        {
            Box(root, "Lane", new Vector3(2.5f, 0.04f, 2.5f), new Vector3(0f, 0.02f, 0f), Mat(DockGreen));
            for (int i = -2; i <= 2; i++)
                Box(root, "Stripe", new Vector3(0.16f, 0.05f, 2.3f), new Vector3(i * 0.5f, 0.045f, 0f), Mat(Hazard));
            Box(root, "BorderF", new Vector3(2.5f, 0.06f, 0.12f), new Vector3(0f, 0.05f, 1.2f), Mat(VerifyAmber));
            Box(root, "BorderB", new Vector3(2.5f, 0.06f, 0.12f), new Vector3(0f, 0.05f, -1.2f), Mat(VerifyAmber));
        }

        /// <summary>A scanner station: plinth, post and a glowing terminal screen.</summary>
        public static void BuildScannerProp(Transform root)
        {
            Box(root, "Plinth", new Vector3(1.3f, 0.5f, 0.9f), new Vector3(0f, 0.25f, 0f), Mat(VerifyAmber));
            Box(root, "Scanner", new Vector3(1.1f, 0.12f, 0.7f), new Vector3(0f, 0.56f, 0.1f), Mat(DarkPart));
            Box(root, "Post", new Vector3(0.12f, 1.0f, 0.12f), new Vector3(0f, 1.0f, -0.32f), Mat(RackMetal));
            Box(root, "Screen", new Vector3(0.9f, 0.6f, 0.07f), new Vector3(0f, 1.25f, -0.24f), Emissive(ScreenCyan, 1.4f));
        }

        /// <summary>A simple wheeled picker-bot from primitives (used only when no PickerBot prefab exists).</summary>
        public static void BuildRobotFallback(Transform root, Color vest)
        {
            Box(root, "Wheel", new Vector3(0.16f, 0.34f, 0.34f), new Vector3(-0.30f, 0.22f, 0f), Mat(DarkPart));
            Box(root, "Wheel", new Vector3(0.16f, 0.34f, 0.34f), new Vector3(0.30f, 0.22f, 0f), Mat(DarkPart));
            Box(root, "Body", new Vector3(0.62f, 0.7f, 0.46f), new Vector3(0f, 0.75f, 0f), Mat(BodyLight));
            Box(root, "Vest", new Vector3(0.66f, 0.36f, 0.50f), new Vector3(0f, 0.62f, 0f), Mat(vest));
            Box(root, "Head", new Vector3(0.44f, 0.4f, 0.42f), new Vector3(0f, 1.26f, 0f), Mat(BodyLight));
            Box(root, "Screen", new Vector3(0.34f, 0.26f, 0.06f), new Vector3(0f, 1.28f, 0.21f), Emissive(DarkPart, 0f));
            Box(root, "EyeL", new Vector3(0.07f, 0.08f, 0.04f), new Vector3(-0.08f, 1.31f, 0.23f), Emissive(ScreenCyan));
            Box(root, "EyeR", new Vector3(0.07f, 0.08f, 0.04f), new Vector3(0.08f, 1.31f, 0.23f), Emissive(ScreenCyan));
            Box(root, "Cap", new Vector3(0.48f, 0.1f, 0.44f), new Vector3(0f, 1.48f, 0f), Mat(CapDark));
            Box(root, "CapPeak", new Vector3(0.3f, 0.05f, 0.18f), new Vector3(0f, 1.46f, 0.28f), Mat(CapDark));
        }

        /// <summary>A thin coloured floor decal (zone marking / aisle line) just above the floor plane.</summary>
        public static GameObject FloorDecal(Transform parent, string name, Vector3 center, Vector2 size, Color c)
        {
            return Box(parent, name, new Vector3(size.x, 0.02f, size.y), center + Vector3.up * 0.02f, Mat(c));
        }

        /// <summary>
        /// A wall segment (kick plate, panel, pillars, top trim, optional lit
        /// clerestory windows) built along local +X, centred, from the floor up.
        /// Children are collider-free; the caller adds one bounding BoxCollider.
        /// </summary>
        public static void BuildWallSegment(Transform p, float length, float height, float thick, bool windows)
        {
            Box(p, "Kick", new Vector3(length, 0.4f, thick * 1.1f), new Vector3(0f, 0.2f, 0f), Mat(WallBase));
            Box(p, "Panel", new Vector3(length, height - 0.4f, thick), new Vector3(0f, 0.4f + (height - 0.4f) * 0.5f, 0f), Mat(WallMain));
            Box(p, "TopTrim", new Vector3(length, 0.28f, thick * 1.25f), new Vector3(0f, height - 0.14f, 0f), Mat(WallTrim));

            int bays = Mathf.Max(2, Mathf.RoundToInt(length / 3f));
            for (int i = 0; i <= bays; i++)
            {
                float x = -length / 2f + i * (length / bays);
                Box(p, "Pillar", new Vector3(0.18f, height, thick * 1.3f), new Vector3(x, height * 0.5f, 0f), Mat(Girder));
            }
            if (windows)
            {
                float wy = height * 0.66f;
                var glass = Emissive(WindowLit, 0.5f);
                for (int i = 0; i < bays; i++)
                {
                    float x = -length / 2f + (i + 0.5f) * (length / bays);
                    Box(p, "Window", new Vector3(length / bays * 0.62f, 0.8f, thick * 0.5f), new Vector3(x, wy, thick * 0.25f), glass);
                }
            }
        }

        /// <summary>A taller decorative rack bay (3 shelf levels) with mixed stock — for background aisles.</summary>
        public static void BuildTallRack(Transform root, int seed)
        {
            const float w = 1.25f, d = 1.05f, h = 2.6f, t = 0.09f;
            var metal = Mat(RackMetal);
            var beam = Mat(RackBeam);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                    Box(root, "Upright", new Vector3(t, h, t), new Vector3(sx * (w / 2 - t), h / 2, sz * (d / 2 - t)), metal);

            float[] levels = { 0.5f, 1.25f, 2.0f };
            foreach (float y in levels)
            {
                Box(root, "BeamF", new Vector3(w, t * 1.3f, t), new Vector3(0f, y, -(d / 2 - t)), beam);
                Box(root, "BeamB", new Vector3(w, t * 1.3f, t), new Vector3(0f, y, d / 2 - t), beam);
                Box(root, "Shelf", new Vector3(w - 2 * t, t, d - 2 * t), new Vector3(0f, y, 0f), metal);
                // a couple of stock boxes per level (deterministic by seed)
                if (((seed + Mathf.RoundToInt(y * 10)) % 4) != 0)
                    Box(root, "Stock", new Vector3(0.78f, 0.5f, 0.7f), new Vector3(0f, y + 0.3f, 0f),
                        Mat(StockPalette[(seed + Mathf.RoundToInt(y)) % StockPalette.Length]));
            }
        }

        /// <summary>A loose pallet with a small stack of crates — ground clutter for staging zones.</summary>
        public static void BuildPalletStack(Transform root, int boxes)
        {
            Box(root, "PalletDeck", new Vector3(1.1f, 0.12f, 0.9f), new Vector3(0f, 0.06f, 0f), Mat(new Color(0.52f, 0.36f, 0.20f)));
            Box(root, "PalletFootL", new Vector3(0.12f, 0.12f, 0.9f), new Vector3(-0.45f, -0.02f, 0f), Mat(new Color(0.42f, 0.29f, 0.16f)), false);
            Box(root, "PalletFootR", new Vector3(0.12f, 0.12f, 0.9f), new Vector3(0.45f, -0.02f, 0f), Mat(new Color(0.42f, 0.29f, 0.16f)), false);
            for (int i = 0; i < boxes; i++)
            {
                float y = 0.12f + 0.32f + i * 0.42f;
                float jx = (i % 2 == 0) ? 0.06f : -0.07f;
                Box(root, "Crate", new Vector3(0.82f, 0.40f, 0.72f), new Vector3(jx, y, 0f), Mat(StockPalette[i % StockPalette.Length]));
            }
        }

        /// <summary>A black/yellow safety bollard.</summary>
        public static void BuildBollard(Transform root)
        {
            Box(root, "Post", new Vector3(0.16f, 0.9f, 0.16f), new Vector3(0f, 0.45f, 0f), Mat(BollardY));
            Box(root, "BandLo", new Vector3(0.18f, 0.16f, 0.18f), new Vector3(0f, 0.22f, 0f), Mat(DarkPart));
            Box(root, "BandHi", new Vector3(0.18f, 0.16f, 0.18f), new Vector3(0f, 0.66f, 0f), Mat(DarkPart));
        }

        /// <summary>A traffic cone (stacked boxes — keeps it low-poly).</summary>
        public static void BuildCone(Transform root)
        {
            Box(root, "Foot", new Vector3(0.4f, 0.05f, 0.4f), new Vector3(0f, 0.025f, 0f), Mat(ConeOrange));
            Box(root, "Mid", new Vector3(0.22f, 0.22f, 0.22f), new Vector3(0f, 0.16f, 0f), Mat(ConeOrange));
            Box(root, "Top", new Vector3(0.10f, 0.18f, 0.10f), new Vector3(0f, 0.36f, 0f), Mat(ConeOrange));
            Box(root, "Stripe", new Vector3(0.24f, 0.07f, 0.24f), new Vector3(0f, 0.22f, 0f), Mat(BodyLight));
        }

        /// <summary>Maps an SKU packaging type to a prop prefab name (used only if the prefab exists).</summary>
        public static string PrefabForPackaging(PackagingType p)
        {
            switch (p)
            {
                case PackagingType.BottleCase: return "BottleCase";
                case PackagingType.Bag: return "Bag";
                case PackagingType.Case: return "BoxLarge";
                default: return "BoxMedium";
            }
        }
    }
}
