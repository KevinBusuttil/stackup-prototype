using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Detects nearby interactables each frame and exposes the best (nearest
    /// actionable) one. See CLAUDE_CODE_SPEC.md Section 13.3.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public float Radius = 2.2f;
        public LayerMask Mask = ~0;

        public IInteractable Current { get; private set; }

        private PlayerController player;
        private readonly Collider[] hits = new Collider[16];

        private void Awake() => player = GetComponent<PlayerController>();

        private void Update() => Current = FindBest();

        private IInteractable FindBest()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, Radius, hits, Mask, QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var col = hits[i];
                if (col == null) continue;
                var interactable = col.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract(player)) continue;

                float d = (col.transform.position - transform.position).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = interactable; }
            }
            return best;
        }
    }
}
