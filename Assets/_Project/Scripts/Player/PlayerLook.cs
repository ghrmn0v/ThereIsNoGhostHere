
using UnityEngine;
using UnityEngine.InputSystem;
using ThereIsNoGhostHere.Input;

namespace ThereIsNoGhostHere.Player
{
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float sensitivity = 0.1f;
        [SerializeField] private float maxPitch = 85f;

        private PlayerInputReader _input;
        private float _pitch;

        private void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void Update()
        {
            HandleCursorToggle();

            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            float lookY = _input.Look.y * sensitivity;

            _pitch = Mathf.Clamp(
                _pitch - lookY,
                -maxPitch,
                maxPitch
            );

            cameraTransform.localRotation =
                Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void HandleCursorToggle()
        {
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(false);
            }
            else if (
                Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame &&
                Cursor.lockState != CursorLockMode.Locked
            )
            {
                SetCursorLocked(true);
            }
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked
                ? CursorLockMode.Locked
                : CursorLockMode.None;

            Cursor.visible = !locked;
        }
    }
}
