
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class PlayerNickname : NetworkBehaviour
{
    [Header("名前表示用TextMeshPro UI")]
    public TextMeshProUGUI nicknameText;

    [Header("Player頭上の表示位置")]
    public Vector3 nameOffset = new Vector3(0f, 1.2f, 0f);

    private NetworkVariable<FixedString64Bytes> nickname =
        new NetworkVariable<FixedString64Bytes>(
            new FixedString64Bytes("Player"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

    public override void OnNetworkSpawn()
    {
        nickname.OnValueChanged += OnNicknameChanged;

        if (IsOwner)
        {
            string playerName = NetworkUI.LocalNickname;

            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Player" + OwnerClientId;
            }

            nickname.Value = new FixedString64Bytes(playerName);
        }

        UpdateNicknameText();
    }

    public override void OnNetworkDespawn()
    {
        nickname.OnValueChanged -= OnNicknameChanged;
    }

    void LateUpdate()
    {
        if (nicknameText != null)
        {
            // Playerの頭上に追従
            nicknameText.transform.position =
                transform.position + nameOffset;

            // 2Dゲームのカメラに対して文字を正面に向ける
            if (Camera.main != null)
            {
                nicknameText.transform.rotation =
                    Camera.main.transform.rotation;
            }
        }
    }

    void OnNicknameChanged(
        FixedString64Bytes oldValue,
        FixedString64Bytes newValue)
    {
        UpdateNicknameText();
    }
    public string GetNickname()
    {
        return nickname.Value.ToString();
    }
    void UpdateNicknameText()
    {
        if (nicknameText != null)
        {
            nicknameText.text = nickname.Value.ToString();
        }
    }
}
