using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace StackUp
{
    /// <summary>
    /// Programmatically constructs the M1 vertical-slice warehouse — floor, one
    /// rack with stocked slots, a dock lane, the player robot, a high-angle
    /// camera and the HUD — then starts the order loop. This lets the whole
    /// Pick -> Load slice run from an otherwise empty scene. Real prefabs/art
    /// replace these primitives in M3.
    /// </summary>
    public class LevelBootstrap : MonoBehaviour
    {
        [Header("Order (M1: one SKU / one order)")]
        public string SkuId = "BOX-A";
        public int OrderQuantity = 1;
        public string DockLaneId = "DOCK-1";

        [Header("Scoring")]
        public int PointsPerOrder = 100;

        private Material floorMat, rackMat, slotMat, playerMat, dockMat;

        private GameManager game;
        private OrderManager orders;
        private WarehouseGrid grid;
        private PlayerController player;
        private HUD hud;
        private ResultScreen result;

        private void Start()
        {
            game = EnsureGameManager();
            CreateMaterials();
            BuildEnvironment();
            BuildCatalog();
            BuildRack();
            grid = BuildGrid();
            BuildDock();
            player = BuildPlayer();
            BuildCamera(player.transform);
            orders = BuildOrders(player);
            WireInteractables();
            BuildUi();
            StartLevel();
        }

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
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");
            floorMat = Mat(lit, new Color(0.55f, 0.55f, 0.58f));
            rackMat = Mat(lit, new Color(0.30f, 0.33f, 0.40f));
            slotMat = Mat(lit, new Color(0.85f, 0.70f, 0.20f));
            playerMat = Mat(lit, new Color(0.20f, 0.70f, 0.90f));
            dockMat = Mat(lit, new Color(0.20f, 0.75f, 0.35f));
        }

        private static Material Mat(Shader shader, Color c) => new Material(shader) { color = c };

        private void BuildEnvironment()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(4f, 1f, 4f); // 40 x 40
            SetMat(floor, floorMat);

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
        }

        private void BuildCatalog()
        {
            var catalog = new GameObject("SkuCatalog").AddComponent<SkuCatalog>();
            var sku = ScriptableObject.CreateInstance<SkuDefinition>();
            sku.SkuId = SkuId;
            sku.DisplayName = "Standard Box";
            sku.PackagingType = PackagingType.Box;
            sku.WeightClass = WeightClass.Medium;
            sku.StackClass = StackClass.Standard;
            sku.DisplayColor = new Color(0.90f, 0.55f, 0.20f);
            catalog.Register(sku);
        }

        private void BuildRack()
        {
            var rack = new GameObject("RackBay_A");
            rack.transform.position = new Vector3(-6f, 0f, 6f);

            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Mesh";
            frame.transform.SetParent(rack.transform, false);
            frame.transform.localScale = new Vector3(4f, 2f, 1f);
            frame.transform.localPosition = new Vector3(0f, 1f, 0f);
            SetMat(frame, rackMat);

            // Three slots along the rack face; stock the target SKU in the middle.
            for (int i = 0; i < 3; i++)
            {
                var slotGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                slotGo.name = $"Slot_A01_L01_C{i + 1:00}";
                slotGo.transform.SetParent(rack.transform, false);
                slotGo.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                slotGo.transform.localPosition = new Vector3(-1.3f + i * 1.3f, 1.4f, -0.7f);
                SetMat(slotGo, slotMat);

                var marker = slotGo.AddComponent<SlotMarker>();
                marker.SlotId = slotGo.name;
                marker.ZoneId = "ZONE-A";
                if (i == 1)
                    marker.InitialStock.Add(new SlotMarker.StockEntry { SkuId = SkuId, Quantity = 10 });

                slotGo.AddComponent<RackSlot>();
            }
        }

        private WarehouseGrid BuildGrid()
        {
            var g = new GameObject("WarehouseGrid").AddComponent<WarehouseGrid>();
            g.Build();
            return g;
        }

        private void BuildDock()
        {
            var dock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dock.name = "DockLane";
            dock.transform.position = new Vector3(6f, 0.25f, -6f);
            dock.transform.localScale = new Vector3(3f, 0.5f, 3f);
            SetMat(dock, dockMat);

            dock.AddComponent<DockLane>().DockLaneId = DockLaneId;
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

        private OrderManager BuildOrders(PlayerController p)
        {
            var om = new GameObject("OrderManager").AddComponent<OrderManager>();
            om.BindTote(p.Tote);
            return om;
        }

        private void WireInteractables()
        {
            foreach (var slot in FindObjectsByType<RackSlot>(FindObjectsSortMode.None))
                slot.Init(grid, orders);
            foreach (var dock in FindObjectsByType<DockLane>(FindObjectsSortMode.None))
                dock.Init(orders);
        }

        private void BuildUi()
        {
            EnsureEventSystem();

            hud = new GameObject("HUD").AddComponent<HUD>();
            hud.Init(orders, game, player);

            result = new GameObject("ResultScreen").AddComponent<ResultScreen>();
            result.Init();
            orders.OrderCompleted += OnOrderCompleted;
        }

        /// <summary>UI buttons need an EventSystem + input module to receive pointer/controller events.</summary>
        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }

        private void StartLevel()
        {
            game.SetMode(GameMode.Campaign);
            game.ResetScore();
            game.SetState(GameState.Running);

            var lines = new List<OrderLine> { new OrderLine { SkuId = SkuId, Quantity = OrderQuantity } };
            orders.GenerateOrder(lines, DockLaneId);
        }

        private void OnOrderCompleted(CustomerOrder order)
        {
            game.AddScore(PointsPerOrder);
            game.CompleteLevel(game.Score);
            if (result != null)
                result.Show($"Order {order.OrderId} loaded at {order.DockLaneId}\nScore: {game.Score}");
        }

        private static void SetMat(GameObject go, Material m)
        {
            var r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = m;
        }
    }
}
