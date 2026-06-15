using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Lightweight registry of <see cref="SkuDefinition"/>s, keyed by SkuId. For
    /// M1 it is populated at runtime; later it can be backed by authored assets.
    /// See CLAUDE_CODE_SPEC.md Section 13.5.
    /// </summary>
    public class SkuCatalog : MonoBehaviour
    {
        private readonly Dictionary<string, SkuDefinition> byId = new Dictionary<string, SkuDefinition>();

        public IEnumerable<SkuDefinition> All => byId.Values;

        public void Register(SkuDefinition sku)
        {
            if (sku != null && !string.IsNullOrEmpty(sku.SkuId)) byId[sku.SkuId] = sku;
        }

        public SkuDefinition Get(string skuId)
        {
            return skuId != null && byId.TryGetValue(skuId, out var sku) ? sku : null;
        }

        public bool TryGet(string skuId, out SkuDefinition sku) => byId.TryGetValue(skuId ?? "", out sku);
    }
}
