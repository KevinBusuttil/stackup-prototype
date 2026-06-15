using UnityEngine;
using UnityEngine.InputSystem;

namespace StackUp
{
    /// <summary>
    /// Top-down robot movement + interaction trigger, driven by the Input System
    /// (keyboard or gamepad). See CLAUDE_CODE_SPEC.md Sections 13.2 / 19.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        public float MoveSpeed = 6f;
        public float TurnSpeed = 12f;

        public Tote Tote { get; private set; }
        public PlayerInteractor Interactor { get; private set; }
        public Transform HeadMarker { get; set; }

        private Rigidbody rb;
        private Vector3 moveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            Tote = GetComponent<Tote>();
            if (Tote == null) Tote = gameObject.AddComponent<Tote>();
            Interactor = GetComponent<PlayerInteractor>();
        }

        private void Update()
        {
            Vector2 move = ReadMove();
            moveInput = new Vector3(move.x, 0f, move.y);
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

            if (InteractPressed() && Interactor != null)
            {
                var target = Interactor.Current;
                if (target != null && target.CanInteract(this)) target.Interact(this);
            }
        }

        private void FixedUpdate()
        {
            Vector3 velocity = moveInput * MoveSpeed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

            if (moveInput.sqrMagnitude > 0.001f)
            {
                Quaternion look = Quaternion.LookRotation(moveInput, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, TurnSpeed * Time.fixedDeltaTime));
            }
        }

        private static Vector2 ReadMove()
        {
            Vector2 v = Vector2.zero;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.y -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1f;
            }
            var pad = Gamepad.current;
            if (pad != null)
            {
                Vector2 stick = pad.leftStick.ReadValue();
                if (stick.sqrMagnitude > v.sqrMagnitude) v = stick;
            }
            return v;
        }

        private static bool InteractPressed()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.eKey.wasPressedThisFrame) return true;
            var pad = Gamepad.current;
            if (pad != null && pad.buttonSouth.wasPressedThisFrame) return true;
            return false;
        }
    }
}
