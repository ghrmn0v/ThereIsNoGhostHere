using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace ThereIsNoGhostHere.Networking
{
    [DisallowMultipleComponent]
    public class DedicatedServerBootstrap : MonoBehaviour
    {
        [SerializeField] private ushort defaultPort = 7777;
        [SerializeField] private string listenAddress = "0.0.0.0";
        [SerializeField] private int serverTargetFrameRate = 60;

#if UNITY_SERVER
        private void Start()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                Debug.LogError("[Server] NetworkManager not found in scene.");
                return;
            }

            Application.targetFrameRate = serverTargetFrameRate;

            ushort port = ReadPortArgument(defaultPort);
            var transport = nm.GetComponent<UnityTransport>();
            transport.SetConnectionData(listenAddress, port, listenAddress);

            nm.OnClientConnectedCallback += id =>
                Debug.Log($"[Server] Client connected: {id} | total: {nm.ConnectedClientsIds.Count}");
            nm.OnClientDisconnectCallback += id =>
                Debug.Log($"[Server] Client disconnected: {id} | total: {nm.ConnectedClientsIds.Count}");

            if (nm.StartServer())
                Debug.Log($"[Server] Started on {listenAddress}:{port}");
            else
                Debug.LogError("[Server] StartServer failed.");
        }

        private static ushort ReadPortArgument(ushort fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-port" && ushort.TryParse(args[i + 1], out ushort p))
                    return p;
            }
            return fallback;
        }
#endif
    }
}