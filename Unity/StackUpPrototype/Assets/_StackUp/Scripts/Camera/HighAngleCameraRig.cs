using UnityEngine;

namespace StackUp
{
    /// <summary>
    /// Smooth high-angle follow camera (Section 7). A lightweight custom rig so
    /// the vertical slice has no Cinemachine dependency; can be swapped for a
    /// CinemachineCamera later without touching gameplay code.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class HighAngleCameraRig : MonoBehaviour
    {
        public Transform Target;
        [Range(30f, 80f)] public float Pitch = 55f;
        public float Distance = 12f;
        public float Height = 1.5f;
        [Range(20f, 60f)] public float FieldOfView = 34f;
        public float FollowDamping = 8f;

        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.fieldOfView = FieldOfView;
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            if (!Mathf.Approximately(cam.fieldOfView, FieldOfView)) cam.fieldOfView = FieldOfView;

            Quaternion rot = Quaternion.Euler(Pitch, 0f, 0f);
            Vector3 offset = rot * new Vector3(0f, 0f, -Distance);
            Vector3 desired = Target.position + Vector3.up * Height + offset;

            float t = 1f - Mathf.Exp(-FollowDamping * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desired, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, t);
        }
    }
}
