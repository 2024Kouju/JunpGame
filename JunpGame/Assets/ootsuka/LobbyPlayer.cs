
using UnityEngine;
using Unity.Netcode;

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
    }

    public override void OnNetworkDespawn()
    {
        SelectedCharacter.OnValueChanged -= OnCharacterChanged;
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
