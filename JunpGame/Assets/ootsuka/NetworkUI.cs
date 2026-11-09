
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [Header("入力欄")]
    public TMP_InputField playerNameInput;
    public TMP_InputField ipAddressInput;

    [Header("表示")]
    public TMP_Text localIPAddressText;
    public TMP_Text statusText;
    public TMP_Text maxPlayersText;

    [Header("接続設定")]
    public ushort port = 7777;

    [Header("対戦人数")]
    public int maxPlayers = 2;

    public static string LocalNickname = "Player";

    // Hostが設定した最大人数
    public static int MaxPlayers = 2;

    private NetworkManager manager;
    private string localIPAddress = "取得中";
    private bool connecting = false;



    void Start()
    {
        // NetworkManagerを取得
        manager = NetworkManager.Singleton;

        if (manager == null)
        {
            Debug.LogError("NetworkManagerがありません");
            SetStatus("NetworkManagerがありません");
            return;
        }

        // 接続承認を有効化
        manager.NetworkConfig.ConnectionApproval = true;
        manager.ConnectionApprovalCallback = ApprovalCheck;

        // 接続イベント登録
        manager.OnClientConnectedCallback += OnClientConnected;
        manager.OnClientDisconnectCallback += OnClientDisconnect;

        // IPアドレス取得
        localIPAddress = GetLocalIPAddress();

        // IP入力欄に自分のIPアドレスを初期入力
        if (ipAddressInput != null)
        {
            ipAddressInput.text = localIPAddress;
        }

        UpdateIPAddressText();
        UpdateMaxPlayersText();

        SetStatus("接続待機中");
    }

    void OnDestroy()
    {
        if (manager != null)
        {
            manager.OnClientConnectedCallback -= OnClientConnected;
            manager.OnClientDisconnectCallback -= OnClientDisconnect;
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
        if (manager == null)
            return;

        // 自分自身の接続成功
        if (clientId == manager.LocalClientId)
        {
            connecting = false;

            if (manager.IsHost)
            {
                SetStatus("LobbySceneへ移動します");

                manager.SceneManager.LoadScene(
                    "LobbyScene",
                    LoadSceneMode.Single
                );
            }
            else
            {
                SetStatus("Hostに接続しました");
            }
        }
    }

    // 接続切断
    void OnClientDisconnect(ulong clientId)
    {
        if (manager == null)
            return;

        if (clientId == manager.LocalClientId)
        {
            connecting = false;
            SetStatus("接続が切断されました");
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
        string trimmedName = playerNameInput.text.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
            trimmedName = "Player";

        while (Encoding.UTF8.GetByteCount(trimmedName) > 60)
        {
            trimmedName =
                trimmedName.Substring(0, trimmedName.Length - 1);
        }

        LocalNickname = trimmedName;
    }

    // Host開始ボタン
    public void StartAsHost()
    {
        if (connecting || manager == null || manager.IsListening)
            return;

        SaveNickname();

        MaxPlayers = maxPlayers;

        UnityTransport transport =
            manager.GetComponent<UnityTransport>();

        if (transport == null)
        {
            SetStatus("UnityTransportがありません");
            return;
        }

        transport.SetConnectionData("0.0.0.0", port);

        connecting = true;
        SetStatus("Hostを開始しています...");

        if (!manager.StartHost())
        {
            connecting = false;
            SetStatus("Hostの起動に失敗しました");
            Debug.LogError("Hostの起動に失敗しました");
        }
    }

    // Client接続ボタン
    public void StartAsClient()
    {
        if (connecting || manager == null || manager.IsListening)
            return;

        SaveNickname();

        string hostIP = ipAddressInput.text.Trim();

        if (string.IsNullOrWhiteSpace(hostIP))
        {
            SetStatus("HostのIPアドレスを入力してください");
            return;
        }

        UnityTransport transport =
            manager.GetComponent<UnityTransport>();

        if (transport == null)
        {
            SetStatus("UnityTransportがありません");
            return;
        }

        transport.SetConnectionData(hostIP, port);

        connecting = true;
        SetStatus("Hostに接続しています...");

        if (!manager.StartClient())
        {
            connecting = false;
            SetStatus("Clientの接続開始に失敗しました");
            Debug.LogError("Clientの接続開始に失敗しました");
        }
    }

    // 2人対戦ボタン
    public void SetMaxPlayers2()
    {
        if (connecting || (manager != null && manager.IsListening))
            return;

        maxPlayers = 2;
        UpdateMaxPlayersText();
    }

    // 3人対戦ボタン
    public void SetMaxPlayers3()
    {
        if (connecting || (manager != null && manager.IsListening))
            return;

        maxPlayers = 3;
        UpdateMaxPlayersText();
    }

    // 人数表示
    void UpdateMaxPlayersText()
    {
        if (maxPlayersText != null)
        {
            maxPlayersText.text =
                "対戦人数：" + maxPlayers + "人";
        }
    }

    // IP表示
    void UpdateIPAddressText()
    {
        if (localIPAddressText != null)
        {
            localIPAddressText.text =
                "自分のIPアドレス：" + localIPAddress;
        }
    }

    // 接続状態表示
    void SetStatus(string message)
    {
        Debug.Log(message);

        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
