
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

    // Hostが設定した最大人数
    public static int MaxPlayers = 2;

    private NetworkManager manager;
    private string localIPAddress = "取得中";

    private bool connecting = false;

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

        // 接続成功時の処理
        manager.OnClientConnectedCallback += OnClientConnected;
    }

    void Start()
    {
        localIPAddress = GetLocalIPAddress();
    }

    void OnDestroy()
    {
        if (manager != null)
        {
            manager.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    // 接続人数の上限を確認
    void ApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        int currentPlayers = manager.ConnectedClientsIds.Count;

        if (currentPlayers < MaxPlayers)
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

    // 接続成功
    void OnClientConnected(ulong clientId)
    {
        if (clientId == manager.LocalClientId && manager.IsHost)
        {
            manager.SceneManager.LoadScene(
                "LobbyScene",
                LoadSceneMode.Single
            );
        }
    }

    // IPアドレス取得
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
                    return endPoint.Address.ToString();
            }
        }
        catch { }

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
        catch { }

        return "IPアドレスを取得できません";
    }

    // ニックネーム保存
    void SaveNickname()
    {
        string trimmedName = nicknameInput.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
            trimmedName = "Player";

        while (Encoding.UTF8.GetByteCount(trimmedName) > 60)
        {
            trimmedName =
                trimmedName.Substring(0, trimmedName.Length - 1);
        }

        LocalNickname = trimmedName;
    }

    // Host開始
    void StartAsHost()
    {
        if (connecting || manager.IsListening)
            return;

        SaveNickname();

        MaxPlayers = maxPlayers;

        UnityTransport transport =
            manager.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("UnityTransportがありません");
            return;
        }

        transport.SetConnectionData("0.0.0.0", port);

        connecting = true;

        if (!manager.StartHost())
        {
            connecting = false;
            Debug.LogError("Hostの起動に失敗しました");
        }
    }

    // Client接続
    void StartAsClient()
    {
        if (connecting || manager.IsListening)
            return;

        SaveNickname();

        UnityTransport transport =
            manager.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("UnityTransportがありません");
            return;
        }

        transport.SetConnectionData(hostAddress, port);

        connecting = true;

        if (!manager.StartClient())
        {
            connecting = false;
            Debug.LogError("Clientの接続開始に失敗しました");
        }
    }

    // UI
    void OnGUI()
    {
        if (manager == null || manager.IsListening)
            return;

        float panelWidth = 620f;
        float panelHeight = 500f;

        float panelX = (Screen.width - panelWidth) * 0.5f;
        float panelY = (Screen.height - panelHeight) * 0.5f;

        GUILayout.BeginArea(
            new Rect(panelX, panelY, panelWidth, panelHeight),
            GUI.skin.box);

        //-------------------------------------------------
        // スタイル
        //-------------------------------------------------

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 32;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor =
            new Color(1f, 0.9f, 0.3f);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 20;

        GUIStyle textFieldStyle =
            new GUIStyle(GUI.skin.textField);
        textFieldStyle.fontSize = 18;

        GUIStyle buttonStyle =
            new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 18;

        //-------------------------------------------------
        // タイトル
        //-------------------------------------------------

        GUILayout.Space(15);

        GUILayout.Label(
            "LAN対戦メニュー",
            titleStyle,
            GUILayout.Height(45));

        GUILayout.Space(15);

        //-------------------------------------------------
        // 接続情報
        //-------------------------------------------------

        GUILayout.Label(
            "このPCのIPアドレス : " + localIPAddress,
            labelStyle);

        GUILayout.Label(
            "ポート番号 : " + port,
            labelStyle);

        GUILayout.Space(20);

        //-------------------------------------------------
        // ニックネーム
        //-------------------------------------------------

        GUILayout.Label(
            "ニックネーム",
            labelStyle);

        nicknameInput = GUILayout.TextField(
            nicknameInput,
            textFieldStyle,
            GUILayout.Height(40));

        GUILayout.Space(20);

        //-------------------------------------------------
        // 人数設定
        //-------------------------------------------------

        GUILayout.Label(
            "対戦人数",
            labelStyle);

        GUILayout.BeginHorizontal();

        Color oldColor = GUI.backgroundColor;

        GUI.backgroundColor =
            maxPlayers == 2
            ? new Color(0.3f, 0.8f, 0.3f)
            : Color.white;

        if (GUILayout.Button(
            "2人対戦",
            buttonStyle,
            GUILayout.Height(40)))
        {
            maxPlayers = 2;
        }

        GUI.backgroundColor =
            maxPlayers == 3
            ? new Color(0.3f, 0.8f, 0.3f)
            : Color.white;

        if (GUILayout.Button(
            "3人対戦",
            buttonStyle,
            GUILayout.Height(40)))
        {
            maxPlayers = 3;
        }

        GUI.backgroundColor = oldColor;

        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.Label(
            "現在の設定 : " + maxPlayers + "人",
            labelStyle);

        GUILayout.Space(20);

        //-------------------------------------------------
        // ホストIP
        //-------------------------------------------------

        GUILayout.Label(
            "HostのIPアドレス",
            labelStyle);

        hostAddress = GUILayout.TextField(
            hostAddress,
            textFieldStyle,
            GUILayout.Height(40));

        GUILayout.Space(25);

        //-------------------------------------------------
        // ボタン
        //-------------------------------------------------

        GUI.enabled = !connecting;

        GUI.backgroundColor =
            new Color(0.4f, 0.8f, 0.4f);

        if (GUILayout.Button(
            "Hostとして開始",
            buttonStyle,
            GUILayout.Height(55)))
        {
            StartAsHost();
        }

        GUILayout.Space(10);

        GUI.backgroundColor =
            new Color(0.4f, 0.7f, 1.0f);

        if (GUILayout.Button(
            "Clientとして接続",
            buttonStyle,
            GUILayout.Height(55)))
        {
            StartAsClient();
        }

        GUI.backgroundColor = oldColor;
        GUI.enabled = true;

        //-------------------------------------------------
        // 接続中
        //-------------------------------------------------

        if (connecting)
        {
            GUILayout.Space(15);

            GUIStyle centerLabel =
                new GUIStyle(labelStyle);

            centerLabel.alignment =
                TextAnchor.MiddleCenter;

            GUILayout.Label(
                "接続処理中...",
                centerLabel);
        }

        GUILayout.EndArea();
    }
}
