
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThereIsNoGhostHere.Input
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset controls;

        private InputActionAsset _runtimeControls;
        private InputActionMap _playerMap;
        private InputAction _move;
        private InputAction _look;
        private InputAction _crouch;

        public Vector2 Move => _move.ReadValue<Vector2>();
        public Vector2 Look => _look.ReadValue<Vector2>();
        public bool CrouchHeld => _crouch.IsPressed();

        private void Awake()
        {
            if (controls == null)
            {
                Debug.LogError("PlayerInputReader: Controls asset is not assigned.", this);
                enabled = false;
                return;
            }

            _runtimeControls = Instantiate(controls);

            _playerMap = _runtimeControls.FindActionMap("Player", true);
            _move = _playerMap.FindAction("Move", true);
            _look = _playerMap.FindAction("Look", true);
            _crouch = _playerMap.FindAction("Crouch", true);
        }

        private void OnEnable()
        {
            _playerMap?.Enable();
        }

        private void OnDisable()
        {
            _playerMap?.Disable();
        }

        private void OnDestroy()
        {
            if (_runtimeControls != null)
                Destroy(_runtimeControls);
        }
    }
}
