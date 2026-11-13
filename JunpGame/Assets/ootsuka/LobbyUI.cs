
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

public class LobbyUI : MonoBehaviour
{
    private NetworkManager manager;
    private LobbyPlayer localPlayer;

    private int fontSize = 22;

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

        // GUIの有効状態をリセット
        GUI.enabled = true;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 30;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        GUI.Label(
            new Rect(0, 20, 800, 50),
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

        // Player取得・所有権確認
        if (localPlayer != null)
        {
            GUI.Label(
                new Rect(20, 65, 760, 30),
                "Player取得成功 / IsOwner : " + localPlayer.IsOwner,
                style
            );
        }
        else
        {
            GUI.Label(
                new Rect(20, 65, 760, 30),
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

        // 全プレイヤーを表示
        for (int i = 0; i < players.Length; i++)
        {
            LobbyPlayer player = players[i];

            if (player == null || !player.IsSpawned)
                continue;

            // 2人を左右に配置
            int x = i == 0 ? 100 : 500;

            string playerType =
                player.OwnerClientId == 0
                ? "HOST"
                : "CLIENT";

            // プレイヤー名
            GUI.Label(
                new Rect(x, 105, 200, 40),
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
                        new Rect(x + 25, 150, 150, 150),
                        sprite.texture,
                        ScaleMode.ScaleToFit,
                        true
                    );
                }
            }

            GUI.Label(
                new Rect(x, 305, 200, 40),
                "キャラクター " + (characterIndex + 1),
                style
            );

            GUI.Label(
                new Rect(x, 345, 200, 40),
                player.IsReady.Value
                    ? "準備完了！"
                    : "準備中...",
                style
            );
        }

        // キャラクター選択欄
        GUI.Label(
            new Rect(0, 395, 800, 40),
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
            int x = 160 + i * 160;

            bool selected =
                canSelect &&
                localPlayer.SelectedCharacter.Value == i;

            if (GUI.Button(
                new Rect(x, 440, 140, 50),
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
                new Rect(250, 510, 300, 55),
                ready ? "準備取消" : "準備OK"))
            {
                localPlayer.SetReadyServerRpc(!ready);
            }
        }

        // GUIを再び有効化
        GUI.enabled = true;

        // Host専用ゲーム開始
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
                new Rect(570, 510, 200, 35),
                "参加人数 : " + playerCount,
                style
            );

            if (allReady && playerCount >= 2)
            {
                if (GUI.Button(
                    new Rect(570, 550, 200, 45),
                    "ゲーム開始"))
                {
                    StartBattle();
                }
            }
            else
            {
                GUI.Label(
                    new Rect(570, 550, 220, 35),
                    "準備完了を待っています",
                    style
                );
            }
        }

        GUI.enabled = true;
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
