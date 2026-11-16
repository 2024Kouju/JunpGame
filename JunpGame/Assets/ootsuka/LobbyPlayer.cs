
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("キャラクター画像")]
    public Sprite[] characterSprites = new Sprite[3];

    [Header("Player1アニメーション")]
    public RuntimeAnimatorController player1Animator;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

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
        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        SelectedCharacter.OnValueChanged += OnCharacterChanged;

        UpdateCharacterAppearance(SelectedCharacter.Value);
        UpdateVisibility();
    }

    public override void OnNetworkDespawn()
    {
        SelectedCharacter.OnValueChanged -= OnCharacterChanged;
    }

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

    // 表示とアニメーションの切り替え
    private void UpdateVisibility()
    {
        bool isGameScene =
            SceneManager.GetActiveScene().name == "GameScene";

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isGameScene;
        }

        if (animator != null)
        {
            // Player1だけアニメーションを使用
            bool isPlayer1 = SelectedCharacter.Value == 0;

            animator.enabled = isGameScene && isPlayer1;
        }
    }

    // キャラクター変更
    [ServerRpc(RequireOwnership = true)]
    public void ChangeCharacterServerRpc(int index)
    {
        if (index < 0 || index >= characterSprites.Length)
            return;

        SelectedCharacter.Value = index;

        IsReady.Value = false;

        Debug.Log(
            OwnerClientId + "番のプレイヤーがキャラクター"
            + (index + 1) + "を選択しました"
        );
    }

    private void OnCharacterChanged(int oldValue, int newValue)
    {
        UpdateCharacterAppearance(newValue);
        UpdateVisibility();
    }

    // キャラクター画像とAnimator設定
    private void UpdateCharacterAppearance(int index)
    {
        if (index < 0 || index >= characterSprites.Length)
            return;

        if (animator != null)
        {
            if (index == 0)
            {
                // Player1
                if (player1Animator != null)
                {
                    animator.runtimeAnimatorController =
                        player1Animator;
                }
            }
        }

        // Player2・3は従来の静止画像
        if (spriteRenderer != null)
        {
            if (index != 0)
            {
                spriteRenderer.sprite = characterSprites[index];
            }
        }
    }

    // 準備状態変更
    [ServerRpc(RequireOwnership = true)]
    public void SetReadyServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }
}
