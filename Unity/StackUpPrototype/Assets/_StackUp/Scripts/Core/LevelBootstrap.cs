using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace StackUp
{
    /// <summary>
    /// Programmatically constructs a warehouse level and starts the order loop,
    /// so the slice runs from an otherwise empty scene. Two modes:
    ///   - simple (M1): pick into the tote and load at the dock.
    ///   - full (M2): pick -> stack onto a per-order pallet -> verify -> load,
    ///     with multiple concurrent orders, scoring, and penalties.
    /// Real prefabs/art replace these primitives in M3.
    /// </summary>
    public class LevelBootstrap : MonoBehaviour
    {
        [Header("Level configuration")]
        public bool UseStacking = false;
        public bool UseVerification = false;
        public int MaxConcurrent = 1;

        private const string BoxA = "BOX-A";
        private const string GlassB = "GLASS-B";
        private const string SteelC = "STEEL-C";
        private const string Junk = "JUNK-X";

        private Material floorMat, rackMat, playerMat, dockMat, verifyMat;
        private Shader lit;

        private GameManager game;
        private OrderManager orders;
        private WarehouseGrid grid;
        private SkuCatalog catalog;
        private PlayerController player;
        private ResultScreen result;
        private int palletsCreated;

        private void Start()
        {
            game = EnsureGameManager();
            CreateMaterials();
            BuildEnvironment();
            catalog = BuildCatalog();
            BuildRacks();
            grid = BuildGrid();
            BuildDocks();
            if (UseVerification) BuildVerificationStation();
            player = BuildPlayer();
            BuildCamera(player.transform);

            var score = new GameObject("ScoreSystem").AddComponent<ScoreSystem>();
            score.Init(game);

            orders = new GameObject("OrderManager").AddComponent<OrderManager>();
            orders.Init(player.Tote, score, catalog, UseStacking, UseVerification, MaxConcurrent, MakePallet);

            WireInteractables();
            BuildUi();
            player.Orders = orders;

            EnqueueOrders();
            game.SetMode(GameMode.Campaign);
            game.ResetScore();
            game.SetState(GameState.Running);
            orders.Begin();
        }

        // ------------------------------------------------------------- helpers
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
            rackMat = Mat(new Color(0.30f, 0.33f, 0.40f));
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
            MakeSku(cat, BoxA, "Standard Box", PackagingType.Box, WeightClass.Medium, StackClass.Standard, new Color(0.90f, 0.55f, 0.20f));
            if (UseStacking)
            {
                MakeSku(cat, GlassB, "Glass Case", PackagingType.BottleCase, WeightClass.Light, StackClass.Fragile, new Color(0.40f, 0.80f, 0.95f));
                MakeSku(cat, SteelC, "Steel Bar", PackagingType.Case, WeightClass.Heavy, StackClass.Standard, new Color(0.70f, 0.70f, 0.75f));
                MakeSku(cat, Junk, "Mis-stocked Item", PackagingType.Bag, WeightClass.Light, StackClass.Standard, new Color(0.80f, 0.30f, 0.65f));
            }
            return cat;
        }

        private static void MakeSku(SkuCatalog cat, string id, string name, PackagingType pk, WeightClass w, StackClass s, Color col)
        {
            var sku = ScriptableObject.CreateInstance<SkuDefinition>();
            sku.SkuId = id;
            sku.DisplayName = name;
            sku.PackagingType = pk;
            sku.WeightClass = w;
            sku.StackClass = s;
            sku.DisplayColor = col;
            cat.Register(sku);
        }

        private void BuildRacks()
        {
            if (!UseStacking)
            {
                MakeRackSlot("Slot_A01", new Vector3(-6f, 1f, 7f), BoxA, 10);
                return;
            }
            MakeRackSlot("Slot_A01", new Vector3(-8f, 1f, 7f), BoxA, 12);
            MakeRackSlot("Slot_A02", new Vector3(-5f, 1f, 7f), GlassB, 12);
            MakeRackSlot("Slot_A03", new Vector3(-2f, 1f, 7f), SteelC, 12);
            MakeRackSlot("Slot_A04", new Vector3(1f, 1f, 7f), Junk, 12); // decoy: not in any order
        }

        private void MakeRackSlot(string slotId, Vector3 pos, string sku, int qty)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"RackSlot_{sku}";
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            var def = catalog != null ? catalog.Get(sku) : null;
            SetMat(go, def != null ? Mat(def.DisplayColor) : rackMat);

            var marker = go.AddComponent<SlotMarker>();
            marker.SlotId = slotId;
            marker.ZoneId = "ZONE-A";
            marker.InitialStock.Add(new SlotMarker.StockEntry { SkuId = sku, Quantity = qty });
            go.AddComponent<RackSlot>();
        }

        private WarehouseGrid BuildGrid()
        {
            var g = new GameObject("WarehouseGrid").AddComponent<WarehouseGrid>();
            g.Build();
            return g;
        }

        private void BuildDocks()
        {
            MakeDock("DOCK-1", new Vector3(UseStacking ? 4f : 6f, 0.25f, -6f));
            if (UseStacking) MakeDock("DOCK-2", new Vector3(8f, 0.25f, -6f));
        }

        private void MakeDock(string id, Vector3 pos)
        {
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
        }

        private Pallet MakePallet(CustomerOrder order)
        {
            var go = new GameObject($"Pallet_{order.OrderId}");
            go.transform.position = new Vector3(-3f + palletsCreated * 3f, 0f, -9f);
            palletsCreated++;
            var pallet = go.AddComponent<Pallet>();
            pallet.Init(order, orders, catalog, 3, 3, 4);
            return pallet;
        }

        private void WireInteractables()
        {
            foreach (var slot in FindObjectsByType<RackSlot>(FindObjectsSortMode.None)) slot.Init(grid, orders);
            foreach (var dock in FindObjectsByType<DockLane>(FindObjectsSortMode.None)) dock.Init(orders);
            foreach (var vs in FindObjectsByType<VerificationStation>(FindObjectsSortMode.None)) vs.Init(orders);
        }

        private void BuildUi()
        {
            EnsureEventSystem();

            var hud = new GameObject("HUD").AddComponent<HUD>();
            hud.Init(orders, game, player);

            result = new GameObject("ResultScreen").AddComponent<ResultScreen>();
            result.Init();
            orders.AllOrdersDone += OnAllOrdersDone;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private void EnqueueOrders()
        {
            if (!UseStacking)
            {
                orders.Enqueue(orders.MakeOrder(new List<OrderLine> { Line(BoxA, 1) }, "DOCK-1"));
                return;
            }
            orders.Enqueue(orders.MakeOrder(new List<OrderLine> { Line(BoxA, 2) }, "DOCK-1"));
            orders.Enqueue(orders.MakeOrder(new List<OrderLine> { Line(GlassB, 1), Line(BoxA, 1) }, "DOCK-2"));
            orders.Enqueue(orders.MakeOrder(new List<OrderLine> { Line(SteelC, 1), Line(BoxA, 1) }, "DOCK-1"));
        }

        private static OrderLine Line(string sku, int qty) => new OrderLine { SkuId = sku, Quantity = qty };

        private void OnAllOrdersDone()
        {
            int best = orders.Score != null ? orders.Score.BestCombo : 0;
            game.CompleteLevel(game.Score);
            if (result != null)
                result.Show($"All orders loaded!\nScore: {game.Score}\nBest combo: x{best}");
        }

        private static void SetMat(GameObject go, Material m)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = m;
        }
    }
}
