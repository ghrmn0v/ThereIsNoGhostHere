using Unity.Netcode;
using UnityEngine;

namespace ThereIsNoGhostHere.Networking
{
    [DisallowMultipleComponent]
    public class PlayerVisualController : NetworkBehaviour
    {
        [SerializeField] private Renderer visualRenderer;
        [SerializeField] private Color[] playerColors =
            { Color.cyan, Color.yellow, Color.green, Color.magenta };

        private CharacterController _controller;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            if (visualRenderer == null) return;

            // Öz Capsule-un öz kameranın görüntüsünə mane olmasın
            visualRenderer.enabled = !IsOwner;

            // Hər oyunçu öz ClientId-sinə görə rəng alır
            int index = (int)(OwnerClientId % (ulong)playerColors.Length);
            visualRenderer.material.color = playerColors[index];
        }

        private void LateUpdate()
        {
            if (visualRenderer == null || _controller == null) return;

            // CharacterController torpaqda skinWidth qədər yuxarıda dayanır,
            // Capsule isə ayaqlarının torpağa toxunması üçün həmin qədər aşağı salınır.
            float skin = _controller.skinWidth;
            float h = _controller.height + skin;

            Transform t = visualRenderer.transform;
            t.localScale = new Vector3(t.localScale.x, h * 0.5f, t.localScale.z);
            t.localPosition = new Vector3(0f, h * 0.5f - skin, 0f);
        }
    }
}