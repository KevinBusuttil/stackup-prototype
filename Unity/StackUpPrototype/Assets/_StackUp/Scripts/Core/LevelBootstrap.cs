using System.Collections.Generic;
using UnityEngine;

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
            game = EnsureGameManager();
            cfg = ResolveConfig();

            CreateMaterials();
            BuildEnvironment();
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
            floorMat = Mat(new Color(0.55f, 0.55f, 0.58f));
            playerMat = Mat(new Color(0.20f, 0.70f, 0.90f));
            dockMat = Mat(new Color(0.20f, 0.75f, 0.35f));
            verifyMat = Mat(new Color(0.85f, 0.65f, 0.20f));
        }

        private Material Mat(Color c) => new Material(lit) { color = c };

        private void BuildEnvironment()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(4f, 1f, 4f);
            SetMat(floor, floorMat);

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
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
            SetMat(go, Mat(def != null ? def.DisplayColor : new Color(0.4f, 0.4f, 0.45f)));

            var marker = go.AddComponent<SlotMarker>();
            marker.SlotId = slotId;
            marker.ZoneId = "ZONE-A";
            marker.InitialStock.Add(new SlotMarker.StockEntry { SkuId = sku, Quantity = qty });
            go.AddComponent<RackSlot>();
            go.AddComponent<FadeableObject>(); // racks fade when they block the camera (Section 8)
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
        }

        private void BuildVerificationStation()
        {
            var v = GameObject.CreatePrimitive(PrimitiveType.Cube);
            v.name = "VerificationStation";
            v.transform.position = new Vector3(0f, 0.75f, -2f);
            v.transform.localScale = new Vector3(2f, 1.5f, 1.2f);
            SetMat(v, verifyMat);
            v.AddComponent<VerificationStation>();
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
            return pc;
        }

        private void BuildCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
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
