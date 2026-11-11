using UnityEngine;
using UnityEngine.InputSystem;

namespace ThereIsNoGhostHere.Input
{
    public class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset controls;

        private InputActionMap _playerMap;
        private InputAction _move;
        private InputAction _look;
        private InputAction _sprint;
        private InputAction _crouch;

        public Vector2 Move => _move.ReadValue<Vector2>();
        public Vector2 Look => _look.ReadValue<Vector2>();
        public bool SprintHeld => _sprint.IsPressed();
        public bool CrouchHeld => _crouch.IsPressed();

        private void Awake()
        {
            _playerMap = controls.FindActionMap("Player", true);
            _move = _playerMap.FindAction("Move", true);
            _look = _playerMap.FindAction("Look", true);
            _sprint = _playerMap.FindAction("Sprint", true);
            _crouch = _playerMap.FindAction("Crouch", true);
        }

        private void OnEnable() => _playerMap.Enable();
        private void OnDisable() => _playerMap.Disable();
    }
}