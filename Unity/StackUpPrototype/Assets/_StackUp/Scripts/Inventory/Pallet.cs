using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// A pallet bound to one order. The player stacks the order's items onto its
    /// grid (Sections 13.6 / 14.2). Shows a placement preview (green = valid,
    /// red = no legal cell) and visualises placed items.
    /// </summary>
    public class Pallet : MonoBehaviour, IInteractable
    {
        public float CellSize = 0.5f;
        public float ItemHeight = 0.45f;

        public PalletGrid Grid { get; private set; }
        public CustomerOrder Order { get; private set; }

        private OrderManager orders;
        private SkuCatalog catalog;
        private readonly List<GameObject> itemVisuals = new List<GameObject>();
        private GameObject preview;
        private Renderer previewRenderer;
        private Material itemMat, validMat, invalidMat;

        public void Init(CustomerOrder order, OrderManager orders, SkuCatalog catalog, int width, int depth, int height)
        {
            Order = order;
            this.orders = orders;
            this.catalog = catalog;
            Grid = new PalletGrid(width, depth, height);

            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");
            itemMat = new Material(lit) { color = new Color(0.85f, 0.60f, 0.30f) };
            validMat = new Material(lit) { color = new Color(0.20f, 0.90f, 0.30f) };
            invalidMat = new Material(lit) { color = new Color(0.90f, 0.20f, 0.20f) };

            BuildBase(lit);
            BuildTrigger();
            BuildPreview();
        }

        private void BuildBase(Shader lit)
        {
            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseGo.name = "PalletBase";
            baseGo.transform.SetParent(transform, false);
            baseGo.transform.localScale = new Vector3(Grid.Width * CellSize + 0.2f, 0.1f, Grid.Depth * CellSize + 0.2f);
            baseGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            var r = baseGo.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = new Material(lit) { color = new Color(0.45f, 0.30f, 0.18f) };
            StripCollider(baseGo);
        }

        private void BuildTrigger()
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 0.5f, 0f);
            box.size = new Vector3(Grid.Width * CellSize + 1f, 1.5f, Grid.Depth * CellSize + 1f);
        }

        private void BuildPreview()
        {
            preview = GameObject.CreatePrimitive(PrimitiveType.Cube);
            preview.name = "PlacementPreview";
            preview.transform.SetParent(transform, false);
            preview.transform.localScale = new Vector3(CellSize * 0.85f, ItemHeight * 0.85f, CellSize * 0.85f);
            previewRenderer = preview.GetComponent<Renderer>();
            StripCollider(preview);
            preview.SetActive(false);
        }

        private static void StripCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) Destroy(c);
        }

        private Vector3 CellWorld(int x, int z, int layer)
        {
            float ox = (Grid.Width - 1) * 0.5f;
            float oz = (Grid.Depth - 1) * 0.5f;
            return transform.position + new Vector3(
                (x - ox) * CellSize,
                0.1f + ItemHeight * (layer + 0.5f),
                (z - oz) * CellSize);
        }

        private PalletItem MakeItem(string sku)
        {
            var def = catalog != null ? catalog.Get(sku) : null;
            return new PalletItem
            {
                SkuId = sku,
                Weight = def != null ? def.WeightClass : WeightClass.Medium,
                Stack = def != null ? def.StackClass : StackClass.Standard
            };
        }

        private string Stackable() => orders != null ? orders.NextStackableSku(Order) : null;

        // ---- IInteractable ----
        public string GetPrompt()
        {
            string sku = Stackable();
            return sku != null ? $"Stack {sku} ({Order.OrderId})" : "Nothing to stack";
        }

        public bool CanInteract(PlayerController player) => Stackable() != null;

        public void Interact(PlayerController player)
        {
            string sku = Stackable();
            if (sku == null) return;

            var item = MakeItem(sku);
            if (Grid.FindFirstValidCell(item, out int x, out int z))
            {
                int layer = Grid.Height(x, z);
                Grid.TryPlace(x, z, item);
                RenderItem(x, z, layer, sku);
                orders.OnStacked(Order, sku);
            }
            else
            {
                orders.OnIllegalStack();
            }
        }

        private void RenderItem(int x, int z, int layer, string sku)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Item_{sku}";
            cube.transform.SetParent(transform, false);
            cube.transform.position = CellWorld(x, z, layer);
            cube.transform.localScale = new Vector3(CellSize * 0.9f, ItemHeight * 0.9f, CellSize * 0.9f);
            StripCollider(cube);

            var r = cube.GetComponent<Renderer>();
            if (r != null)
            {
                var def = catalog != null ? catalog.Get(sku) : null;
                var m = new Material(itemMat);
                if (def != null) m.color = def.DisplayColor;
                r.sharedMaterial = m;
            }
            itemVisuals.Add(cube);
        }

        private void Update()
        {
            if (preview == null) return;

            string sku = Stackable();
            if (sku == null) { preview.SetActive(false); return; }

            preview.SetActive(true);
            var item = MakeItem(sku);
            if (Grid.FindFirstValidCell(item, out int x, out int z))
            {
                preview.transform.position = CellWorld(x, z, Grid.Height(x, z));
                if (previewRenderer != null) previewRenderer.sharedMaterial = validMat;
            }
            else
            {
                preview.transform.position = transform.position + Vector3.up * (0.1f + ItemHeight * (Grid.MaxHeight + 0.5f));
                if (previewRenderer != null) previewRenderer.sharedMaterial = invalidMat;
            }
        }
    }
}
