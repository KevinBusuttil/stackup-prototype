using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Spherecasts from the camera toward the player and fades any
    /// <see cref="FadeableObject"/> in the way, restoring it once clear.
    /// The player, tote, and interaction objects carry no FadeableObject, so they
    /// never fade. See CLAUDE_CODE_SPEC.md Section 8.
    /// </summary>
    public class OccluderFadeSystem : MonoBehaviour
    {
        public Transform Target;
        public float CastRadius = 0.4f;

        private readonly HashSet<FadeableObject> faded = new HashSet<FadeableObject>();
        private readonly HashSet<FadeableObject> current = new HashSet<FadeableObject>();
        private readonly RaycastHit[] hits = new RaycastHit[32];

        private void LateUpdate()
        {
            current.Clear();
            if (Target != null)
            {
                Vector3 from = transform.position;
                Vector3 to = Target.position;
                Vector3 dir = to - from;
                float dist = dir.magnitude;
                if (dist > 0.01f)
                {
                    int count = Physics.SphereCastNonAlloc(from, CastRadius, dir / dist, hits, dist, ~0, QueryTriggerInteraction.Ignore);
                    for (int i = 0; i < count; i++)
                    {
                        var f = hits[i].collider.GetComponentInParent<FadeableObject>();
                        if (f == null) continue;
                        f.SetOccluding(true);
                        current.Add(f);
                    }
                }
            }

            // Restore anything no longer blocking.
            foreach (var f in faded)
                if (f != null && !current.Contains(f)) f.SetOccluding(false);

            faded.Clear();
            foreach (var f in current) faded.Add(f);
        }
    }
}
