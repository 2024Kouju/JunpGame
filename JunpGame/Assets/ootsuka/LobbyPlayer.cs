
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyPlayer : NetworkBehaviour
{
    [Header("キャラクター画像")]
    public Sprite[] characterSprites = new Sprite[3];

    [Header("Animator Controllers")]
    public RuntimeAnimatorController player1Animator;
    public RuntimeAnimatorController player2Animator;
    public RuntimeAnimatorController player3Animator;

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
            // Player1・2・3すべてアニメーションを使用
            animator.enabled = isGameScene;
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

    // キャラクター画像とAnimator Controllerを設定
    private void UpdateCharacterAppearance(int index)
    {
        if (animator == null)
            return;

        switch (index)
        {
            case 0:
                animator.runtimeAnimatorController = player1Animator;
                break;

            case 1:
                animator.runtimeAnimatorController = player2Animator;
                break;

            case 2:
                animator.runtimeAnimatorController = player3Animator;
                break;
        }
    }

    // 準備状態変更
    [ServerRpc(RequireOwnership = true)]
    public void SetReadyServerRpc(bool ready)
    {
        IsReady.Value = ready;
    }
}
