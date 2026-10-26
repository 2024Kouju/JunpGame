using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;

public class NetworkUI : MonoBehaviour
{
    [Header("接続設定")]
    public string hostAddress = "192.168.1.25";
    public ushort port = 7777;

    [Header("対戦人数")]
    public int maxPlayers = 2;

    private NetworkManager manager;

    // 起動時の処理
    void Awake()
    {
        manager = NetworkManager.Singleton;

        if (manager == null)
        {
            Debug.LogError("NetworkManagerがありません");
            return;
        }

        // 接続人数の上限を設定
        manager.NetworkConfig.ConnectionApproval = true;
        manager.ConnectionApprovalCallback = ApprovalCheck;
    }

    // 接続を許可するか判定
    void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        int currentPlayers = manager.ConnectedClientsIds.Count;

        if (currentPlayers < maxPlayers)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
        else
        {
            response.Approved = false;
            response.CreatePlayerObject = false;
            response.Reason = "対戦人数の上限に達しています";
        }

        response.Pending = false;
    }

    // このPCのローカルIPアドレスを取得
    string GetLocalIPAddress()
    {
        try
        {
            using (Socket socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp))
            {
                socket.Connect("8.8.8.8", 65530);

                IPEndPoint endPoint =
                    socket.LocalEndPoint as IPEndPoint;

                if (endPoint != null)
                {
                    return endPoint.Address.ToString();
                }
            }
        }
        catch
        {
            // 下の代替処理へ
        }

        // 代替方法でIPアドレスを取得
        try
        {
            foreach (IPAddress address in
                Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (address.AddressFamily ==
                    AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(address))
                {
                    return address.ToString();
                }
            }
        }
        catch
        {
            // 取得できなかった場合
        }

        return "IPアドレスを取得できません";
    }

    // 画面表示
    void OnGUI()
    {
        if (manager == null)
        {
            return;
        }

        // UI全体の大きさ（800×600用）
        float panelWidth = 560f;
        float panelHeight = 600f;

        // 画面中央に配置
        float panelX = (Screen.width - panelWidth) / 2f;
        float panelY = (Screen.height - panelHeight) / 2f;
        // 全体の表示領域
        GUILayout.BeginArea(
            new Rect(panelX, panelY, panelWidth, panelHeight),
            GUI.skin.box
        );

        // 文字サイズ
        GUI.skin.label.fontSize = 24;
        GUI.skin.button.fontSize = 22;
        GUI.skin.textField.fontSize = 22;

        GUILayout.Space(20);

        GUILayout.Label(
            "===== LAN対戦メニュー =====",
            GUILayout.Height(40)
        );

        GUILayout.Space(10);

        GUILayout.Label(
            "このPCのIPアドレス：" + GetLocalIPAddress(),
            GUILayout.Height(35)
        );

        GUILayout.Label(
            "ポート番号：" + port,
            GUILayout.Height(35)
        );

        GUILayout.Space(15);

        if (!manager.IsListening)
        {
            GUILayout.Label(
                "対戦人数を選択",
                GUILayout.Height(35)
            );

            if (GUILayout.Button("2人対戦", GUILayout.Height(45)))
            {
                maxPlayers = 2;
            }

            if (GUILayout.Button("3人対戦", GUILayout.Height(45)))
            {
                maxPlayers = 3;
            }

            GUILayout.Label(
                "現在の設定：" + maxPlayers + "人",
                GUILayout.Height(35)
            );

            GUILayout.Space(10);

            GUILayout.Label(
                "HostのIPアドレス",
                GUILayout.Height(35)
            );

            hostAddress = GUILayout.TextField(
                hostAddress,
                GUILayout.Height(45)
            );

            GUILayout.Space(20);

            if (GUILayout.Button(
                "Hostとして開始",
                GUILayout.Height(60)))
            {
                UnityTransport transport =
                    manager.GetComponent<UnityTransport>();

                if (transport != null)
                {
                    transport.SetConnectionData("0.0.0.0", port);

                    if (!manager.StartHost())
                    {
                        Debug.LogError("Hostの起動に失敗しました");
                    }
                }
                else
                {
                    Debug.LogError("UnityTransportがありません");
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button(
                "Clientとして接続",
                GUILayout.Height(60)))
            {
                UnityTransport transport =
                    manager.GetComponent<UnityTransport>();

                if (transport != null)
                {
                    transport.SetConnectionData(hostAddress, port);

                    if (!manager.StartClient())
                    {
                        Debug.LogError("Clientの接続開始に失敗しました");
                    }
                }
                else
                {
                    Debug.LogError("UnityTransportがありません");
                }
            }
        }
        else
        {
            GUILayout.Space(20);

            GUILayout.Label(
                manager.IsHost
                    ? "Hostとして接続中"
                    : "Clientとして接続中",
                GUILayout.Height(45)
            );

            GUILayout.Label(
                "接続人数：" +
                manager.ConnectedClientsIds.Count +
                " / " + maxPlayers,
                GUILayout.Height(45)
            );
        }

        GUILayout.EndArea();
    }
}