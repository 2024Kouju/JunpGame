using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkStart : MonoBehaviour
{
    public GameObject connectionPanel;

    private NetworkManager manager;

    void Start()
    {
        manager = NetworkManager.Singleton;
    }

    void Update()
    {
        if (manager == null) return;
        if (!manager.IsListening) return;

        // 接続済みプレイヤーが2人になったら非表示
        if (manager.ConnectedClientsIds.Count >= 2)
        {
            connectionPanel.SetActive(false);
        }
    }

    public void StartHost()
    {
        var transport =
            manager.GetComponent<UnityTransport>();

        transport.SetConnectionData(
            "127.0.0.1", 7777, "0.0.0.0");

        manager.StartHost();
    }

    public void StartClient()
    {
        var transport =
            manager.GetComponent<UnityTransport>();

        transport.SetConnectionData(
            "127.0.0.1", 7777);

        manager.StartClient();
    }
}