using UnityEngine;
using ThereIsNoGhostHere.Input;

namespace ThereIsNoGhostHere.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float sprintSpeed = 5f;
        [SerializeField] private float crouchSpeed = 1.5f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -20f;

        [Header("Crouch")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchingHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 8f;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float standingCameraY = 1.6f;
        [SerializeField] private float crouchingCameraY = 0.8f;

        private CharacterController _controller;
        private PlayerInputReader _input;
        private float _verticalVelocity;
        private bool _isCrouching;

        public bool IsCrouching => _isCrouching;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputReader>();
        }

        private void Update()
        {
            UpdateCrouchState();
            UpdateCrouchTransition();
            Move();
        }

        private void UpdateCrouchState()
        {
            if (_input.CrouchHeld)
            {
                _isCrouching = true;
            }
            else if (_isCrouching && CanStandUp())
            {
                _isCrouching = false;
            }
        }

        private bool CanStandUp()
        {
            float radius = _controller.radius * 0.9f;
            Vector3 origin = transform.position + Vector3.up * (_controller.height - radius);
            float distance = standingHeight - _controller.height;
            if (distance <= 0.01f) return true;

            return !Physics.SphereCast(origin, radius, Vector3.up, out _, distance,
                ~0, QueryTriggerInteraction.Ignore);
        }

        private void UpdateCrouchTransition()
        {
            float targetHeight = _isCrouching ? crouchingHeight : standingHeight;
            float t = crouchTransitionSpeed * Time.deltaTime;

            _controller.height = Mathf.Lerp(_controller.height, targetHeight, t);
            _controller.center = new Vector3(0f, _controller.height * 0.5f, 0f);

            if (cameraTransform != null)
            {
                float targetCamY = _isCrouching ? crouchingCameraY : standingCameraY;
                Vector3 pos = cameraTransform.localPosition;
                pos.y = Mathf.Lerp(pos.y, targetCamY, t);
                cameraTransform.localPosition = pos;
            }
        }

        private void Move()
        {
            Vector2 input = _input.Move;
            Vector3 direction = transform.right * input.x + transform.forward * input.y;

            float speed = walkSpeed;
            if (_isCrouching)
                speed = crouchSpeed;
            else if (_input.SprintHeld && input.y > 0f) // sprint yalnız irəli
                speed = sprintSpeed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = direction * speed;
            velocity.y = _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);
        }
    }
}