using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace ThereIsNoGhostHere.Networking
{
    [DisallowMultipleComponent]
    public class NetworkBootstrapUI : MonoBehaviour
    {
        [SerializeField] private string address = "127.0.0.1";
        [SerializeField] private ushort port = 7777;

        private void OnGUI()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 260, 200));

            if (!nm.IsClient && !nm.IsServer)
            {
                address = GUILayout.TextField(address);
                if (GUILayout.Button("Host")) { Configure(nm); nm.StartHost(); }
                if (GUILayout.Button("Client")) { Configure(nm); nm.StartClient(); }
                if (GUILayout.Button("Server")) { Configure(nm); nm.StartServer(); }
            }
            else
            {
                string mode = nm.IsHost ? "Host" : nm.IsServer ? "Server" : "Client";
                GUILayout.Label("Mode: " + mode);
                if (nm.IsServer)
                    GUILayout.Label("Connected clients: " + nm.ConnectedClientsIds.Count);
                if (GUILayout.Button("Disconnect")) nm.Shutdown();
            }

            GUILayout.EndArea();
        }

        private void Configure(NetworkManager nm)
        {
            var transport = nm.GetComponent<UnityTransport>();
            transport.SetConnectionData(address, port);
        }
    }
}