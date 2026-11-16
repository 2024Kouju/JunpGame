
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("キャラクター画像")]
    public Sprite[] characterSprites = new Sprite[3];

    private SpriteRenderer spriteRenderer;

    // 選択キャラクター
    public NetworkVariable<int> SelectedCharacter =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    // 準備状態
    public NetworkVariable<bool> IsReady =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        SelectedCharacter.OnValueChanged += OnCharacterChanged;

        UpdateCharacterImage(SelectedCharacter.Value);
        UpdateVisibility();
    }

    public override void OnNetworkDespawn()
    {
        SelectedCharacter.OnValueChanged -= OnCharacterChanged;
    }

    // シーン切り替え時
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        UpdateVisibility();
    }

    // Player画像の表示切り替え
    private void UpdateVisibility()
    {
        if (spriteRenderer == null)
            return;

        bool isLobby =
            SceneManager.GetActiveScene().name == "LobbyScene";

        // LobbySceneでは非表示
        // GameSceneでは表示
        spriteRenderer.enabled = !isLobby;
    }

    // キャラクター変更
    [ServerRpc(RequireOwnership = true)]
    public void ChangeCharacterServerRpc(int index)
    {
        if (index < 0 || index >= characterSprites.Length)
            return;

        SelectedCharacter.Value = index;

        // キャラクター変更時は準備解除
        IsReady.Value = false;

        Debug.Log(
            OwnerClientId + "番のプレイヤーがキャラクター"
            + (index + 1) + "を選択しました"
        );
    }

    // キャラクター変更時
    private void OnCharacterChanged(int oldValue, int newValue)
    {
        UpdateCharacterImage(newValue);
    }

    // 画像変更
    private void UpdateCharacterImage(int index)
    {
        if (spriteRenderer == null)
            return;

        if (index >= 0 && index < characterSprites.Length)
        {
            spriteRenderer.sprite = characterSprites[index];
        }
    }

    // 準備状態変更
    [ServerRpc(RequireOwnership = true)]
    public void SetReadyServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }
}
