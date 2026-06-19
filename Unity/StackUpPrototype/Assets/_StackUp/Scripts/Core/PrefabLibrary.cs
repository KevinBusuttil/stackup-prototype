using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Optional art layer. If a prefab exists at
    /// Assets/_StackUp/Resources/StackUpArt/&lt;name&gt;.prefab it is instantiated;
    /// otherwise callers fall back to primitives. This lets the imported Blender
    /// models (M3 #34/#35) replace the runtime primitives without changing
    /// gameplay code — drop prefabs into Resources and they appear.
    /// </summary>
    public static class PrefabLibrary
    {
        private const string Root = "StackUpArt/";

        public static bool Has(string name) => Resources.Load<GameObject>(Root + name) != null;

        public static GameObject Spawn(string name, Transform parent)
        {
            var prefab = Resources.Load<GameObject>(Root + name);
            if (prefab == null) return null;
            var go = Object.Instantiate(prefab);
            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.identity;
            return go;
        }
    }
}
