
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

    [Header("ニックネーム")]
    public string nicknameInput = "Player";

    public static string LocalNickname = "Player";

    private NetworkManager manager;
    private string localIPAddress = "取得中";

    void Awake()
    {
        manager = NetworkManager.Singleton;

        if (manager == null)
        {
            Debug.LogError("NetworkManagerがありません");
            return;
        }

        manager.NetworkConfig.ConnectionApproval = true;
        manager.ConnectionApprovalCallback = ApprovalCheck;
    }

    void Start()
    {
        // IPアドレスは起動時に一度取得
        localIPAddress = GetLocalIPAddress();
    }

    // 接続人数の上限を確認
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
            // 代替方法で取得する
        }

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

    // 接続前にニックネームを確定
    void SaveNickname()
    {
        string trimmedName = nicknameInput.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            trimmedName = "Player";
        }

        // FixedString64Bytesの最大バイト数に合わせる
        while (System.Text.Encoding.UTF8.GetByteCount(trimmedName) > 60)
        {
            trimmedName = trimmedName.Substring(
                0, trimmedName.Length - 1);
        }

        LocalNickname = trimmedName;
    }

    // Hostとして開始
    void StartAsHost()
    {
        SaveNickname();

        UnityTransport transport =
            manager.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("UnityTransportがありません");
            return;
        }

        transport.SetConnectionData("0.0.0.0", port);

        if (!manager.StartHost())
        {
            Debug.LogError("Hostの起動に失敗しました");
        }
    }

    // Clientとして接続
    void StartAsClient()
    {
        SaveNickname();

        UnityTransport transport =
            manager.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("UnityTransportがありません");
            return;
        }

        transport.SetConnectionData(hostAddress, port);

        if (!manager.StartClient())
        {
            Debug.LogError("Clientの接続開始に失敗しました");
        }
    }

    // UI表示
    void OnGUI()
    {
        if (manager == null)
        {
            return;
        }

        float panelWidth = 560f;
        float panelHeight = 520f;

        float panelX = (Screen.width - panelWidth) / 2f;
        float panelY = (Screen.height - panelHeight) / 2f;

        GUILayout.BeginArea(
            new Rect(panelX, panelY, panelWidth, panelHeight),
            GUI.skin.box
        );

        int oldLabelSize = GUI.skin.label.fontSize;
        int oldButtonSize = GUI.skin.button.fontSize;
        int oldTextFieldSize = GUI.skin.textField.fontSize;

        GUI.skin.label.fontSize = 20;
        GUI.skin.button.fontSize = 18;
        GUI.skin.textField.fontSize = 18;

        GUILayout.Space(10);

        GUILayout.Label(
            "LAN対戦メニュー",
            GUILayout.Height(30));

        GUILayout.Label(
            "このPCのIPアドレス：" + localIPAddress,
            GUILayout.Height(30));

        GUILayout.Label(
            "ポート番号：" + port,
            GUILayout.Height(25));

        if (!manager.IsListening)
        {
            GUILayout.Space(5);

            GUILayout.Label("ニックネーム");

            nicknameInput = GUILayout.TextField(
                nicknameInput,
                GUILayout.Height(35));

            GUILayout.Label(
                "使用する名前：" +
                (string.IsNullOrWhiteSpace(nicknameInput)
                    ? "Player"
                    : nicknameInput.Trim()),
                GUILayout.Height(25));

            GUILayout.Space(5);

            GUILayout.Label("対戦人数");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("2人対戦",
                GUILayout.Height(35)))
            {
                maxPlayers = 2;
            }

            if (GUILayout.Button("3人対戦",
                GUILayout.Height(35)))
            {
                maxPlayers = 3;
            }

            GUILayout.EndHorizontal();

            GUILayout.Label(
                "現在の設定：" + maxPlayers + "人",
                GUILayout.Height(25));

            GUILayout.Space(5);

            GUILayout.Label("HostのIPアドレス");

            hostAddress = GUILayout.TextField(
                hostAddress,
                GUILayout.Height(35));

            GUILayout.Space(10);

            if (GUILayout.Button(
                "Hostとして開始",
                GUILayout.Height(45)))
            {
                StartAsHost();
            }

            GUILayout.Space(5);

            if (GUILayout.Button(
                "Clientとして接続",
                GUILayout.Height(45)))
            {
                StartAsClient();
            }
        }
        else
        {
            GUILayout.Space(20);

            GUILayout.Label(
                manager.IsHost
                    ? "Hostとして接続中"
                    : "Clientとして接続中",
                GUILayout.Height(35));

            GUILayout.Label(
                "接続人数：" +
                manager.ConnectedClientsIds.Count +
                " / " + maxPlayers,
                GUILayout.Height(35));

            GUILayout.Label(
                "自分の名前：" + LocalNickname,
                GUILayout.Height(30));
        }

        GUI.skin.label.fontSize = oldLabelSize;
        GUI.skin.button.fontSize = oldButtonSize;
        GUI.skin.textField.fontSize = oldTextFieldSize;

        GUILayout.EndArea();
    }
}
