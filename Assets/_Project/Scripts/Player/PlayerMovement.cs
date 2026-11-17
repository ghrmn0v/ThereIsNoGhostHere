
using Unity.Netcode;
using UnityEngine;
using ThereIsNoGhostHere.Input;

namespace ThereIsNoGhostHere.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(NetworkObject))]
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Speed")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float crouchSpeed = 1.5f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -20f;

        [Header("Crouch")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchingHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 8f;

        [Header("Look")]
        [SerializeField] private float lookSensitivity = 0.1f;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float standingCameraY = 1.6f;
        [SerializeField] private float crouchingCameraY = 0.8f;

        [Header("Network")]
        [SerializeField] private float inputTimeout = 0.25f;

        private CharacterController _controller;
        private PlayerInputReader _input;
        private PlayerLook _look;

        private float _verticalVelocity;
        private float _lastInputTime;
        private Vector2 _serverMoveInput;
        private float _serverLookX;
        private bool _serverCrouchHeld;

        private readonly NetworkVariable<bool> _isCrouching =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server
            );

        public bool IsCrouching => _isCrouching.Value;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputReader>();
            _look = GetComponent<PlayerLook>();
        }
        public override void OnNetworkSpawn()
        {
            Debug.Log(
                $"Player spawned | OwnerClientId: {OwnerClientId} | LocalClientId: {NetworkManager.LocalClientId} | IsOwner: {IsOwner} | IsServer: {IsServer}"
            );
            
            bool isLocalPlayer = IsOwner;

            if (_input != null)
                _input.enabled = isLocalPlayer;

            if (_look != null)
                _look.enabled = isLocalPlayer;

            if (cameraTransform != null)
            {
                Camera playerCamera = cameraTransform.GetComponent<Camera>();
                AudioListener listener =
                    cameraTransform.GetComponent<AudioListener>();

                if (playerCamera != null)
                    playerCamera.enabled = isLocalPlayer;

                if (listener != null)
                    listener.enabled = isLocalPlayer;
            }

            if (IsServer)
            {
                _lastInputTime = Time.time;
            }
        }

        private void Update()
        {
            if (!IsSpawned)
                return;

            if (IsOwner)
                SendLocalInput();

            if (IsServer)
                ServerMovementUpdate();

            UpdateCrouchTransition();
        }

        private void SendLocalInput()
        {
            Vector2 move = _input.Move;
            float lookX = _input.Look.x;
            bool crouchHeld = _input.CrouchHeld;

            SubmitInputRpc(move, lookX, crouchHeld);
        }

        [Rpc(
            SendTo.Server,
            Delivery = RpcDelivery.Unreliable,
            InvokePermission = RpcInvokePermission.Owner
        )]
        private void SubmitInputRpc(
            Vector2 move,
            float lookX,
            bool crouchHeld,
            RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != OwnerClientId)
                return;

            _serverMoveInput = Vector2.ClampMagnitude(move, 1f);
            _serverLookX = Mathf.Clamp(lookX, -100f, 100f);
            _serverCrouchHeld = crouchHeld;
            _lastInputTime = Time.time;
        }

        private void ServerMovementUpdate()
        {
            if (Time.time - _lastInputTime > inputTimeout)
            {
                _serverMoveInput = Vector2.zero;
                _serverLookX = 0f;
                _serverCrouchHeld = false;
            }

            UpdateServerCrouchState();

            transform.Rotate(
                Vector3.up * (_serverLookX * lookSensitivity)
            );

            Vector3 direction =
                transform.right * _serverMoveInput.x +
                transform.forward * _serverMoveInput.y;

            float speed = _isCrouching.Value
                ? crouchSpeed
                : walkSpeed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = direction * speed;
            velocity.y = _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);
        }

        private void UpdateServerCrouchState()
        {
            if (_serverCrouchHeld)
            {
                if (!_isCrouching.Value)
                    _isCrouching.Value = true;
            }
            else if (_isCrouching.Value && CanStandUp())
            {
                _isCrouching.Value = false;
            }
        }

        private bool CanStandUp()
        {
            float radius = _controller.radius * 0.9f;

            Vector3 origin =
                transform.position +
                Vector3.up * (_controller.height - radius);

            float distance = standingHeight - _controller.height;

            if (distance <= 0.01f)
                return true;

            return !Physics.SphereCast(
                origin,
                radius,
                Vector3.up,
                out _,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore
            );
        }

        private void UpdateCrouchTransition()
        {
            float targetHeight = _isCrouching.Value
                ? crouchingHeight
                : standingHeight;

            float t = crouchTransitionSpeed * Time.deltaTime;

            _controller.height = Mathf.Lerp(
                _controller.height,
                targetHeight,
                t
            );

            _controller.center =
                new Vector3(0f, _controller.height * 0.5f, 0f);

            if (cameraTransform != null && IsOwner)
            {
                float targetCameraY = _isCrouching.Value
                    ? crouchingCameraY
                    : standingCameraY;

                Vector3 cameraPosition = cameraTransform.localPosition;
                cameraPosition.y = Mathf.Lerp(
                    cameraPosition.y,
                    targetCameraY,
                    t
                );

                cameraTransform.localPosition = cameraPosition;
            }
        }
    }
}
