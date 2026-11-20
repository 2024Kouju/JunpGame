
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

public class LobbyUI : MonoBehaviour
{
    private NetworkManager manager;
    private LobbyPlayer localPlayer;

    private int fontSize = 30;

    private void Start()
    {
        manager = NetworkManager.Singleton;
    }

    private void OnGUI()
    {
        if (manager == null)
            manager = NetworkManager.Singleton;

        if (manager == null)
            return;

        GUI.enabled = true;

        // 1600×900を基準にする
        float scaleX = Screen.width / 1600f;
        float scaleY = Screen.height / 900f;

        GUI.matrix = Matrix4x4.TRS(
            Vector3.zero,
            Quaternion.identity,
            new Vector3(scaleX, scaleY, 1f)
        );

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 42;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        GUI.Label(
            new Rect(0, 25, 1600, 70),
            "キャラクター選択",
            titleStyle
        );

        // 自分のPlayerを取得
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

        // Player取得確認
        if (localPlayer != null)
        {
            GUI.Label(
                new Rect(20, 90, 1560, 40),
                "Player取得成功 / IsOwner : " + localPlayer.IsOwner,
                style
            );
        }
        else
        {
            GUI.Label(
                new Rect(20, 90, 1560, 40),
                "Player取得待機中",
                style
            );
        }

        // Spawn済みの全Playerを取得
        LobbyPlayer[] players =
            FindObjectsOfType<LobbyPlayer>();

        // ClientId順に並べる
        Array.Sort(players, (a, b) =>
            a.OwnerClientId.CompareTo(b.OwnerClientId)
        );

        // 参加人数
        int playerCount = 0;

        foreach (LobbyPlayer player in players)
        {
            if (player != null && player.IsSpawned)
                playerCount++;
        }

        // Player表示
        int displayIndex = 0;

        for (int i = 0; i < players.Length; i++)
        {
            LobbyPlayer player = players[i];

            if (player == null || !player.IsSpawned)
                continue;

            // 2人・3人に応じて横並びに配置
            int x;

            if (playerCount == 2)
            {
                x = displayIndex == 0 ? 400 : 1000;
            }
            else
            {
                x = 100 + displayIndex * 500;
            }

            displayIndex++;

            string playerType =
                player.OwnerClientId == 0
                ? "HOST"
                : "CLIENT";

            GUI.Label(
                new Rect(x, 150, 400, 55),
                playerType,
                style
            );

            // 選択キャラクター番号
            int characterIndex =
                player.SelectedCharacter.Value;

            // キャラクター画像
            if (characterIndex >= 0 &&
                characterIndex < player.characterSprites.Length)
            {
                Sprite sprite =
                    player.characterSprites[characterIndex];

                if (sprite != null)
                {
                    GUI.DrawTexture(
                        new Rect(x + 50, 215, 300, 250),
                        sprite.texture,
                        ScaleMode.ScaleToFit,
                        true
                    );
                }
            }

            GUI.Label(
                new Rect(x, 475, 400, 50),
                "キャラクター " + (characterIndex + 1),
                style
            );

            GUI.Label(
                new Rect(x, 525, 400, 50),
                player.IsReady.Value
                    ? "準備完了！"
                    : "準備中...",
                style
            );
        }

        // キャラクター選択欄
        GUI.Label(
            new Rect(0, 600, 1600, 50),
            "使用するキャラクターを選択",
            style
        );

        // 自分のPlayerが所有者である場合だけ選択可能
        bool canSelect =
            localPlayer != null &&
            localPlayer.IsSpawned &&
            localPlayer.IsOwner;

        GUI.enabled = canSelect;

        for (int i = 0; i < 3; i++)
        {
            int x = 350 + i * 310;

            bool selected =
                canSelect &&
                localPlayer.SelectedCharacter.Value == i;

            if (GUI.Button(
                new Rect(x, 660, 280, 65),
                selected
                    ? "選択中"
                    : "キャラクター " + (i + 1)))
            {
                localPlayer.ChangeCharacterServerRpc(i);
            }
        }

        // 準備ボタン
        if (canSelect)
        {
            bool ready = localPlayer.IsReady.Value;

            if (GUI.Button(
                new Rect(600, 750, 400, 75),
                ready ? "準備取消" : "準備OK"))
            {
                localPlayer.SetReadyServerRpc(!ready);
            }
        }

        GUI.enabled = true;

        // Host専用ゲーム開始
        if (manager.IsHost)
        {
            bool allReady = true;
            int connectedCount =
                manager.ConnectedClientsIds.Count;

            foreach (ulong clientId in manager.ConnectedClientsIds)
            {
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
                new Rect(1150, 750, 400, 45),
                "接続人数 : " + connectedCount
                + " / " + NetworkUI.MaxPlayers,
                style
            );

            // 1人以上接続していて、全員準備完了なら開始可能
            if (allReady && connectedCount >= 1)
            {
                if (GUI.Button(
                    new Rect(1150, 805, 350, 65),
                    "ゲーム開始"))
                {
                    StartBattle();
                }
            }
            else
            {
                GUI.Label(
                    new Rect(1100, 805, 450, 45),
                    "準備完了を待っています",
                    style
                );
            }

            //// 設定した人数が揃い全員準備完了
            //if (allReady &&
            //    connectedCount == NetworkUI.MaxPlayers)
            //{
            //    if (GUI.Button(
            //        new Rect(1150, 805, 350, 65),
            //        "ゲーム開始"))
            //    {
            //        StartBattle();
            //    }
            //}
            //else
            //{
            //    GUI.Label(
            //        new Rect(1100, 805, 450, 45),
            //        "準備完了を待っています",
            //        style
            //    );
            //}
        }

        GUI.enabled = true;

        // GUI.matrixを初期化
        GUI.matrix = Matrix4x4.identity;
    }

    // ゲーム開始
    private void StartBattle()
    {
        if (manager == null || !manager.IsHost)
            return;

        if (!manager.IsListening)
            return;

        manager.SceneManager.LoadScene(
            "GameScene",
            LoadSceneMode.Single
        );
    }
}
