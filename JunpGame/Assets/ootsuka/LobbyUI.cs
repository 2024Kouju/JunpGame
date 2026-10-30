
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    private NetworkManager manager;

    private int fontSize = 22;

    // 自分のPlayer
    private LobbyPlayer localPlayer;

    void Start()
    {
        manager = NetworkManager.Singleton;
    }

    void OnGUI()
    {
        // NetworkManager取得
        if (manager == null)
        {
            manager = NetworkManager.Singleton;
        }

        if (manager == null)
        {
            GUI.Label(new Rect(30, 30, 500, 40),
                "NetworkManagerが見つかりません");
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 25;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(30, 30, 500, 40),
            "ロビー画面", style);

        GUI.Label(new Rect(30, 80, 500, 40),
            "Client : " + manager.IsClient, style);

        GUI.Label(new Rect(30, 120, 500, 40),
            "Host : " + manager.IsHost, style);

        GUI.Label(new Rect(30, 160, 500, 40),
            "IsListening : " + manager.IsListening, style);

        // Player取得
        localPlayer = null;

        if (manager.IsClient && manager.LocalClient != null)
        {
            NetworkObject myObject =
                manager.LocalClient.PlayerObject;

            if (myObject != null)
            {
                localPlayer =
                    myObject.GetComponent<LobbyPlayer>();
            }
        }

        // Client専用：準備ボタン
        if (manager.IsClient && !manager.IsHost)
        {
            if (localPlayer != null)
            {
                bool ready = localPlayer.IsReady.Value;

                string buttonText =
                    ready ? "準備取消" : "準備OK";

                if (GUI.Button(
                    new Rect(30, 220, 300, 80),
                    buttonText))
                {
                    localPlayer.SetReadyServerRpc(!ready);
                }

                GUI.Label(
                    new Rect(30, 320, 500, 40),
                    ready ? "準備完了！" : "まだ準備していません",
                    style
                );
            }
            else
            {
                GUI.Label(
                    new Rect(30, 220, 500, 40),
                    "自分のPlayerを取得できていません",
                    style
                );
            }
        }

        // Host専用：全員の準備状態を確認
        if (manager.IsHost)
        {
            bool allReady = true;
            int playerCount = 0;

            foreach (ulong clientId in manager.ConnectedClientsIds)
            {
                playerCount++;

                NetworkClient client =
                    manager.ConnectedClients[clientId];

                if (client.PlayerObject == null)
                {
                    allReady = false;
                    continue;
                }

                LobbyPlayer player =
                    client.PlayerObject.GetComponent<LobbyPlayer>();

                if (player == null || !player.IsReady.Value)
                {
                    allReady = false;
                }
            }

            GUI.Label(
                new Rect(30, 220, 500, 40),
                "参加人数 : " + playerCount,
                style
            );

            if (allReady && playerCount >= 2)
            {
                if (GUI.Button(
                    new Rect(30, 280, 300, 80),
                    "ゲーム開始"))
                {
                    StartBattle();
                }
            }
            else
            {
                GUI.Label(
                    new Rect(30, 280, 600, 40),
                    "全員の準備完了を待っています",
                    style
                );
            }
        }
    }

    void StartBattle()
    {
        if (manager == null || !manager.IsHost)
            return;

        Debug.Log("GameSceneへ移動します");

        manager.SceneManager.LoadScene(
            "GameScene",
            LoadSceneMode.Single
        );
    }
}
