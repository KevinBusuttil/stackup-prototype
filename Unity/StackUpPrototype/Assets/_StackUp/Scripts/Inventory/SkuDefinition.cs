using UnityEngine;

namespace StackUp
{
    public enum PackagingType
    {
        Box,
        Case,
        Bag,
        BottleCase
    }

    public enum WeightClass
    {
        Light,
        Medium,
        Heavy
    }

    public enum StackClass
    {
        Standard,
        Fragile,
        Liquid
    }

    /// <summary>
    /// Design-time definition of a stock-keeping unit (SKU).
    /// Authored as a ScriptableObject asset under
    /// Assets/_StackUp/ScriptableObjects/SKUs/.
    /// </summary>
    [CreateAssetMenu(menuName = "StackUp/SKU")]
    public class SkuDefinition : ScriptableObject
    {
        public string SkuId;
        public string DisplayName;
        public PackagingType PackagingType;
        public WeightClass WeightClass;
        public StackClass StackClass;
        public Sprite Icon;
        public Color DisplayColor = Color.white;
        public GameObject VisualPrefab;
    }
}
