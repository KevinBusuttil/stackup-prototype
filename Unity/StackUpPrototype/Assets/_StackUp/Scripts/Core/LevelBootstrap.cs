using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StackUp
{
    /// <summary>
    /// Builds a warehouse level from a <see cref="LevelConfig"/> and starts the
    /// order loop. In the Game scene the config comes from the player's menu
    /// selection (GameManager); the standalone Warehouse_Level_01/02 scenes
    /// synthesize a config from inspector flags. Art is still primitives (M3 #34/#35
    /// — robot/warehouse models — are deferred as they need Blender assets).
    /// </summary>
    public class LevelBootstrap : MonoBehaviour
    {
        [Header("Source")]
        public bool UseGameManagerSelection;

        [Header("Standalone fallback (when not using the menu)")]
        public bool UseStacking;
        public bool UseVerification;
        public int MaxConcurrent = 1;

        private const string Junk = "JUNK-X";

        private Material floorMat, playerMat, dockMat, verifyMat;
        private Shader lit;

        private GameManager game;
        private OrderManager orders;
        private WarehouseGrid grid;
        private SkuCatalog catalog;
        private PlayerController player;
        private ResultScreen result;

        private LevelConfig cfg;
        private int campaignIndex;
        private readonly List<string> docks = new List<string>();
        private int palletsCreated;
        private System.Random rng = new System.Random();
        private SteamTelemetry telemetry;
        private float startTime;

        private void Start()
        {
            Time.timeScale = 1f; // ensure a fresh scene never starts frozen from a prior pause
            game = EnsureGameManager();
            cfg = ResolveConfig();

            CreateMaterials();
            BuildEnvironment();
            BuildDecor();
            catalog = BuildCatalog();
            BuildRacks();
            grid = BuildGrid();
            BuildDocks();
            if (cfg.UseVerification) BuildVerificationStation();
            player = BuildPlayer();
            BuildCamera(player.transform);

            var score = new GameObject("ScoreSystem").AddComponent<ScoreSystem>();
            score.Init(game);

            orders = new GameObject("OrderManager").AddComponent<OrderManager>();
            orders.Init(player.Tote, score, catalog, cfg, MakePallet, GenerateOrder);

            telemetry = new GameObject("SteamTelemetry").AddComponent<SteamTelemetry>();
            telemetry.Init(orders, score, game, player.Tote, cfg, campaignIndex);
            startTime = Time.time;

            var feedback = new GameObject("FeedbackSystem").AddComponent<FeedbackSystem>();
            feedback.Init();
            var fx = new GameObject("LevelFx").AddComponent<LevelFx>();
            fx.Init(orders, score, player.Tote, feedback);
            AudioManager.Ensure();
            AudioManager.StartAmbience();

            WireInteractables();
            BuildUi();
            player.Orders = orders;

            game.SetMode(cfg.Endless ? GameMode.Endless : GameMode.Campaign);
            game.ResetScore();
            game.SetState(GameState.Running);

            if (!cfg.Endless)
                for (int i = 0; i < cfg.OrderCount; i++) orders.Enqueue(GenerateOrder());
            orders.Begin();
        }

        private LevelConfig ResolveConfig()
        {
            if (UseGameManagerSelection && GameManager.Instance != null)
            {
                var gm = GameManager.Instance;
                if (gm.PendingMode == GameMode.Endless) return LevelLibrary.Endless();
                campaignIndex = gm.PendingLevelIndex;
                return LevelLibrary.Get(campaignIndex);
            }

            // Standalone scenes: synthesize from inspector flags.
            var c = new LevelConfig
            {
                Name = "Sandbox",
                UseStacking = UseStacking,
                UseVerification = UseVerification,
                MaxConcurrent = Mathf.Max(1, MaxConcurrent)
            };
            if (!UseStacking)
            {
                c.OrderCount = 1; c.DockCount = 1; c.SkuPool = new[] { LevelLibrary.BoxA };
                c.MaxLinesPerOrder = 1; c.MaxQtyPerLine = 1;
            }
            else
            {
                c.OrderCount = 3; c.DockCount = 2;
                c.SkuPool = new[] { LevelLibrary.BoxA, LevelLibrary.GlassB, LevelLibrary.SteelC };
                c.MaxLinesPerOrder = 2; c.MaxQtyPerLine = 2; c.IncludeDecoy = UseVerification;
            }
            return c;
        }

        // ----------------------------------------------------------- building
        private GameManager EnsureGameManager()
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                gm = new GameObject("GameManager").AddComponent<GameManager>();
                gm.AdvanceToMenuOnStart = false;
            }
            return gm;
        }

        private void CreateMaterials()
        {
            lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");
            floorMat = Mat(ArtKit.Floor);
            playerMat = Mat(ArtKit.PlayerBlue);
            dockMat = Mat(ArtKit.DockGreen);
            verifyMat = Mat(ArtKit.VerifyAmber);
        }

        private Material Mat(Color c) => new Material(lit) { color = c };

        private void BuildEnvironment()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(4f, 1f, 4f);
            SetMat(floor, floorMat);
            BuildFloorMarkings();

            // Key light — warm, soft-shadowed.
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.color = new Color(1f, 0.97f, 0.90f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.65f;

            // Cool fill from the opposite side so shadowed faces don't go murky.
            var fill = new GameObject("Fill Light").AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.transform.rotation = Quaternion.Euler(40f, 150f, 0f);
            fill.color = new Color(0.72f, 0.80f, 0.95f);
            fill.intensity = 0.35f;
            fill.shadows = LightShadows.None;

            // Bright, slightly cool gradient ambient — keeps the toy look readable.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.68f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.52f, 0.53f, 0.56f);
            RenderSettings.ambientGroundColor = new Color(0.34f, 0.32f, 0.30f);
        }

        /// <summary>Painted aisle line + zone tints so the floor reads as a real warehouse.</summary>
        private void BuildFloorMarkings()
        {
            var root = new GameObject("FloorMarkings").transform; // unscaled, so metres map 1:1
            ArtKit.FloorDecal(root, "AisleLine", new Vector3(0f, 0f, 0.5f), new Vector2(0.25f, 30f), ArtKit.AisleLine);
            ArtKit.FloorDecal(root, "CrossAisle", new Vector3(0f, 0f, 2.5f), new Vector2(26f, 0.25f), ArtKit.AisleLine);
            ArtKit.FloorDecal(root, "ZoneStorage", new Vector3(-4.5f, 0f, 7f), new Vector2(13f, 3.2f), ArtKit.RackMetal);
            ArtKit.FloorDecal(root, "ZoneStaging", new Vector3(0f, 0f, -9f), new Vector2(14f, 3f), ArtKit.ZoneStaging);
            ArtKit.FloorDecal(root, "ZoneVerify", new Vector3(0f, 0f, -2f), new Vector2(4f, 3f), ArtKit.ZoneReceiving);
            // hazard safety lane along the dock approach
            ArtKit.FloorDecal(root, "SafetyLane", new Vector3(0f, 0f, -4.5f), new Vector2(24f, 0.4f), ArtKit.VerifyAmber);
        }

        // ------------------------------------------------------- set dressing
        /// <summary>
        /// Builds the static, non-gameplay warehouse: an enclosing shell (walls +
        /// lit clerestory windows), deep background rack aisles, and ground clutter.
        /// Walls/decor racks carry colliders (they bound the player and the
        /// occluder-fade system can fade them); small props are collider-free.
        /// </summary>
        private void BuildDecor()
        {
            // --- shell -----------------------------------------------------
            MakeWall("BackWall", new Vector3(0f, 0f, 11.5f), 30f, 5.5f, 0f, true);
            MakeWall("LeftWall", new Vector3(-14.5f, 0f, -0.5f), 27f, 4.5f, 90f, true);
            MakeWall("RightWall", new Vector3(14.5f, 0f, -0.5f), 27f, 4.5f, 90f, true);
            MakeWall("FrontWallL", new Vector3(-8.5f, 0f, -12f), 12f, 3.5f, 0f, false);
            MakeWall("FrontWallR", new Vector3(8.5f, 0f, -12f), 12f, 3.5f, 0f, false);

            // --- background storage aisles --------------------------------
            int seed = 1;
            for (float x = -12f; x <= 12.01f; x += 3f) MakeDecorRack(new Vector3(x, 0f, 9.4f), seed++);
            for (float z = -6f; z <= 6.01f; z += 3f) { MakeDecorRack(new Vector3(-12.6f, 0f, z), seed++); MakeDecorRack(new Vector3(12.6f, 0f, z), seed++); }

            // --- ground clutter -------------------------------------------
            MakeProp(t => ArtKit.BuildPalletStack(t, 2), new Vector3(-7.5f, 0f, -9.6f), 0f);
            MakeProp(t => ArtKit.BuildPalletStack(t, 3), new Vector3(9f, 0f, -9.6f), 18f);
            MakeProp(t => ArtKit.BuildPalletStack(t, 1), new Vector3(-11.5f, 0f, 3f), 0f);
            MakeProp(t => ArtKit.BuildPalletStack(t, 2), new Vector3(11.5f, 0f, -3f), 0f);
            // bollards/cones kept off the central pick→stack→verify→dock path
            foreach (var bp in new[]
            {
                new Vector3(-2.8f, 0f, -11.4f), new Vector3(2.8f, 0f, -11.4f), // flank the loading doorway
                new Vector3(-13f, 0f, -9f), new Vector3(13f, 0f, -9f),         // staging corners
                new Vector3(-10.5f, 0f, -4.6f), new Vector3(10.5f, 0f, -4.6f), // safety-lane ends
            }) MakeProp(ArtKit.BuildBollard, bp, 0f);
            MakeProp(ArtKit.BuildCone, new Vector3(-2.8f, 0f, -3.4f), 0f);
            MakeProp(ArtKit.BuildCone, new Vector3(2.8f, 0f, -3.4f), 0f);
        }

        private static void MakeWall(string name, Vector3 center, float length, float height, float rotY, bool windows)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            ArtKit.BuildWallSegment(go.transform, length, height, 0.3f, windows); // children first…
            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, height * 0.5f, 0f);
            col.size = new Vector3(length, height, 0.3f);
            go.AddComponent<FadeableObject>();                                    // …then fade caches them
        }

        private static void MakeDecorRack(Vector3 pos, int seed)
        {
            var go = new GameObject("DecorRack");
            go.transform.position = pos;
            ArtKit.BuildTallRack(go.transform, seed);
            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.3f, 0f);
            col.size = new Vector3(1.3f, 2.6f, 1.1f);
            go.AddComponent<FadeableObject>();
        }

        private static void MakeProp(System.Action<Transform> builder, Vector3 pos, float rotY)
        {
            var go = new GameObject("Prop");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            builder(go.transform);
        }

        private SkuCatalog BuildCatalog()
        {
            var cat = new GameObject("SkuCatalog").AddComponent<SkuCatalog>();
            foreach (var id in cfg.SkuPool) MakeKnownSku(cat, id);
            if (cfg.IncludeDecoy) MakeKnownSku(cat, Junk);
            return cat;
        }

        private static void MakeKnownSku(SkuCatalog cat, string id)
        {
            switch (id)
            {
                case LevelLibrary.BoxA: Reg(cat, id, "Standard Box", PackagingType.Box, WeightClass.Medium, StackClass.Standard, new Color(0.90f, 0.55f, 0.20f)); break;
                case LevelLibrary.GlassB: Reg(cat, id, "Glass Case", PackagingType.BottleCase, WeightClass.Light, StackClass.Fragile, new Color(0.40f, 0.80f, 0.95f)); break;
                case LevelLibrary.SteelC: Reg(cat, id, "Steel Bar", PackagingType.Case, WeightClass.Heavy, StackClass.Standard, new Color(0.70f, 0.70f, 0.75f)); break;
                case LevelLibrary.BagD: Reg(cat, id, "Sack", PackagingType.Bag, WeightClass.Medium, StackClass.Standard, new Color(0.80f, 0.72f, 0.45f)); break;
                default: Reg(cat, id, "Mis-stocked Item", PackagingType.Bag, WeightClass.Light, StackClass.Standard, new Color(0.85f, 0.30f, 0.65f)); break;
            }
        }

        private static void Reg(SkuCatalog cat, string id, string name, PackagingType pk, WeightClass w, StackClass s, Color col)
        {
            var sku = ScriptableObject.CreateInstance<SkuDefinition>();
            sku.SkuId = id; sku.DisplayName = name; sku.PackagingType = pk;
            sku.WeightClass = w; sku.StackClass = s; sku.DisplayColor = col;
            cat.Register(sku);
        }

        private void BuildRacks()
        {
            int idx = 0;
            foreach (var sku in cfg.SkuPool)
            {
                MakeRackSlot($"Slot_{idx:00}", RackPos(idx), sku, 20);
                idx++;
            }
            if (cfg.IncludeDecoy) MakeRackSlot($"Slot_{idx:00}_decoy", RackPos(idx), Junk, 20);
        }

        private static Vector3 RackPos(int idx) => new Vector3(-9f + idx * 3f, 1f, 7f);

        private void MakeRackSlot(string slotId, Vector3 pos, string sku, int qty)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"RackSlot_{sku}";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            var def = catalog != null ? catalog.Get(sku) : null;
            Color stockColor = def != null ? def.DisplayColor : new Color(0.55f, 0.56f, 0.60f);

            var marker = go.AddComponent<SlotMarker>();
            marker.SlotId = slotId;
            marker.ZoneId = "ZONE-A";
            marker.InitialStock.Add(new SlotMarker.StockEntry { SkuId = sku, Quantity = qty });
            go.AddComponent<RackSlot>();

            // Build the rack visual BEFORE FadeableObject so its renderers are
            // cached and fade with the bay when it blocks the camera (Section 8).
            AttachArt(go, "RackBay", 1.0f, root => ArtKit.BuildRackFrame(root, stockColor));
            go.AddComponent<FadeableObject>();
        }

        private WarehouseGrid BuildGrid()
        {
            var g = new GameObject("WarehouseGrid").AddComponent<WarehouseGrid>();
            g.Build();
            return g;
        }

        private void BuildDocks()
        {
            docks.Clear();
            if (cfg.DockCount <= 1)
            {
                MakeDock("DOCK-1", new Vector3(6f, 0.25f, -6f));
            }
            else
            {
                MakeDock("DOCK-1", new Vector3(4f, 0.25f, -6f));
                MakeDock("DOCK-2", new Vector3(8f, 0.25f, -6f));
            }
        }

        private void MakeDock(string id, Vector3 pos)
        {
            docks.Add(id);
            var dock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dock.name = id;
            dock.transform.position = pos;
            dock.transform.localScale = new Vector3(2.6f, 0.5f, 2.6f);
            SetMat(dock, dockMat);
            dock.AddComponent<DockLane>().DockLaneId = id;
            AttachArt(dock, "DockLaneMarker", 0.25f, ArtKit.BuildDockPad);
        }

        private void BuildVerificationStation()
        {
            var v = GameObject.CreatePrimitive(PrimitiveType.Cube);
            v.name = "VerificationStation";
            v.transform.position = new Vector3(0f, 0.75f, -2f);
            v.transform.localScale = new Vector3(2f, 1.5f, 1.2f);
            SetMat(v, verifyMat);
            v.AddComponent<VerificationStation>();
            AttachArt(v, "VerificationStation", 0.75f, ArtKit.BuildScannerProp);
        }

        private PlayerController BuildPlayer()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "PickerBot";
            go.transform.position = new Vector3(0f, 1f, 0f);
            SetMat(go, playerMat);

            var rb = go.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            go.AddComponent<Tote>();
            go.AddComponent<PlayerInteractor>();
            var pc = go.AddComponent<PlayerController>();

            var head = new GameObject("HeadMarker").transform;
            head.SetParent(go.transform, false);
            head.localPosition = new Vector3(0f, 1f, 0f);
            pc.HeadMarker = head;
            AttachArt(go, "PickerBot", 1.0f, root => ArtKit.BuildRobotFallback(root, ArtKit.HiVis));
            return pc;
        }

        /// <summary>
        /// Gives a gameplay primitive a real visual: an imported art prefab if one
        /// exists in Resources/StackUpArt, otherwise the procedural <paramref name="fallback"/>
        /// "set dressing". Either way the placeholder mesh is hidden (its collider
        /// kept), so gameplay is identical with or without imported assets.
        /// </summary>
        private static void AttachArt(GameObject host, string prefabName, float centerHeight, System.Action<Transform> fallback)
        {
            if (!AttachVisual(host, prefabName, centerHeight) && fallback != null)
                fallback(ArtKit.ScaledRoot(host, centerHeight, prefabName + "_Visual"));

            var r = host.GetComponent<Renderer>();
            if (r != null) r.enabled = false;
        }

        /// <summary>
        /// Overlays an imported art prefab on a gameplay primitive if present.
        /// Counters the host's (often non-uniform) scale so the model keeps its
        /// authored size, drops it so its base sits on the floor, and re-tints it
        /// to URP. Returns false (caller falls back to primitives) when no prefab.
        /// </summary>
        private static bool AttachVisual(GameObject host, string artName, float centerHeight)
        {
            var v = PrefabLibrary.Spawn(artName, host.transform);
            if (v == null) return false;

            ModelStyle.Apply(v); // URP re-tint so imported models are never pink

            Vector3 hs = host.transform.localScale;
            v.transform.localScale = new Vector3(Inv(hs.x), Inv(hs.y), Inv(hs.z));
            v.transform.localPosition = new Vector3(0f, -centerHeight * Inv(hs.y), 0f);

            var r = host.GetComponent<Renderer>();
            if (r != null) r.enabled = false;
            return true;
        }

        private static float Inv(float v) => Mathf.Approximately(v, 0f) ? 1f : 1f / v;

        private void BuildCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            // Soft sky background + enable URP post (color grading lives in
            // Assets/DefaultVolumeProfile) so the scene isn't framed in black.
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.66f, 0.74f, 0.82f);
            var camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null) camData.renderPostProcessing = true;

            var rig = cam.GetComponent<HighAngleCameraRig>();
            if (rig == null) rig = cam.gameObject.AddComponent<HighAngleCameraRig>();
            rig.Target = target;

            var fade = cam.GetComponent<OccluderFadeSystem>();
            if (fade == null) fade = cam.gameObject.AddComponent<OccluderFadeSystem>();
            fade.Target = target;
        }

        private Pallet MakePallet(CustomerOrder order)
        {
            var go = new GameObject($"Pallet_{order.OrderId}");
            go.transform.position = new Vector3(-3f + (palletsCreated % 4) * 3f, 0f, -9f);
            palletsCreated++;
            var pallet = go.AddComponent<Pallet>();
            pallet.Init(order, orders, catalog, 3, 3, 4);
            return pallet;
        }

        private CustomerOrder GenerateOrder()
        {
            int maxLines = cfg.MaxLinesPerOrder;
            if (cfg.Endless) maxLines = Mathf.Min(cfg.MaxLinesPerOrder, 1 + orders.CompletedCount / 4);
            maxLines = Mathf.Max(1, maxLines);
            int lineCount = 1 + rng.Next(maxLines);

            var lines = new List<OrderLine>();
            for (int i = 0; i < lineCount; i++)
            {
                string sku = cfg.SkuPool[rng.Next(cfg.SkuPool.Length)];
                int qty = 1 + rng.Next(Mathf.Max(1, cfg.MaxQtyPerLine));
                var existing = lines.Find(l => l.SkuId == sku);
                if (existing != null) existing.Quantity += qty;
                else lines.Add(new OrderLine { SkuId = sku, Quantity = qty });
            }

            string dock = docks.Count > 0 ? docks[rng.Next(docks.Count)] : "DOCK-1";
            float due = cfg.SlaSeconds;
            if (cfg.Endless && due > 0f) due = Mathf.Max(20f, cfg.SlaSeconds - orders.CompletedCount * 1.5f);
            return orders.MakeOrder(lines, dock, due);
        }

        private void WireInteractables()
        {
            foreach (var slot in FindObjectsByType<RackSlot>(FindObjectsInactive.Exclude)) slot.Init(grid, orders);
            foreach (var dock in FindObjectsByType<DockLane>(FindObjectsInactive.Exclude)) dock.Init(orders);
            foreach (var vs in FindObjectsByType<VerificationStation>(FindObjectsInactive.Exclude)) vs.Init(orders);
        }

        private void BuildUi()
        {
            UiKit.EnsureEventSystem();

            var hud = new GameObject("HUD").AddComponent<HUD>();
            hud.Init(orders, game, player);

            result = new GameObject("ResultScreen").AddComponent<ResultScreen>();
            result.Init();

            var pause = new GameObject("PauseMenu").AddComponent<PauseMenu>();
            pause.Init();

            orders.AllOrdersDone += OnCampaignWin;
            orders.EndlessEnded += OnEndlessEnded;
        }

        private void OnCampaignWin()
        {
            bool campaignFromMenu = UseGameManagerSelection && !cfg.Endless;
            if (campaignFromMenu)
                SaveService.CompleteCampaignLevel(campaignIndex, game.Score, LevelLibrary.CampaignCount);

            if (campaignFromMenu) telemetry?.ReportCampaignWin(Time.time - startTime);

            game.CompleteLevel(game.Score);

            int next = campaignIndex + 1;
            bool canNext = campaignFromMenu && next < LevelLibrary.CampaignCount;
            result.Show($"{cfg.Name} — Complete!", $"Score: {game.Score}", canNext, next);
        }

        private void OnEndlessEnded()
        {
            SaveService.RecordEndless(game.Score, orders.Wave);
            telemetry?.ReportEndlessEnd();
            game.CompleteLevel(game.Score);
            result.Show("Run Over",
                $"Score: {game.Score}\nReached wave {orders.Wave}\nBest: {SaveService.HighScores.EndlessHighScore} (wave {SaveService.HighScores.EndlessHighestWave})",
                false, 0);
        }

        private static void SetMat(GameObject go, Material m)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = m;
        }
    }
}
