
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMove : NetworkBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 5f;
    public float startSpacing = 3f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;

    [Header("Spring Settings")]
    public float springJumpForce = 15f;

    [Header("GameScene Spawn")]
    public float spawnY = 1f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;

    private Vector2 moveInput;
    private bool isGrounded = false;

    // GameSceneかどうか確認
    private bool IsGameScene()
    {
        return SceneManager.GetActiveScene().name == "GameScene";
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        // 回転を禁止
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void OnNetworkSpawn()
    {
        // サーバー側でシーン切り替えを監視
        if (IsServer)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted
                -= OnSceneLoadCompleted;

            NetworkManager.SceneManager.OnLoadEventCompleted
                += OnSceneLoadCompleted;
        }
    }

    private void Update()
    {
        // GameScene以外では操作しない
        if (!IsGameScene())
            return;

        // 自分のプレイヤーだけ操作
        if (!IsSpawned || !IsOwner)
            return;

        float x = 0f;

        if (Input.GetKey(KeyCode.A))
            x = -1f;

        if (Input.GetKey(KeyCode.D))
            x = 1f;

        Vector2 input = new Vector2(x, 0f).normalized;

        SendInputServerRpc(input);

        // Wキーでジャンプ
        if (Input.GetKeyDown(KeyCode.W))
        {
            JumpServerRpc();
        }
    }

    // 移動情報をサーバーへ送信
    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendInputServerRpc(Vector2 input)
    {
        if (!IsGameScene())
            return;

        moveInput = input;
    }

    // 通常ジャンプ
    [ServerRpc]
    private void JumpServerRpc()
    {
        if (!IsGameScene())
            return;

        if (!isGrounded)
            return;

        rb.velocity = new Vector2(
            rb.velocity.x,
            jumpForce
        );

        isGrounded = false;
    }

    private void FixedUpdate()
    {
        if (!IsGameScene())
            return;

        if (!IsSpawned || !IsServer)
            return;

        // 横方向の移動
        // 縦方向の速度は維持する
        rb.velocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.velocity.y
        );
    }

    // 衝突したとき
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer || !IsGameScene())
            return;

        // バネに着地したとき
        if (collision.collider.CompareTag("Spring"))
        {
            // 下方向に移動しているときだけ反応
            if (rb.velocity.y <= 0f &&
                rb.position.y > collision.transform.position.y)
            {
                rb.velocity = new Vector2(
                    rb.velocity.x,
                    springJumpForce
                );

                isGrounded = false;

                Debug.Log("バネでジャンプ！");
            }
        }

        // 通常の足場
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    // 衝突から離れたとき
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsServer || !IsGameScene())
            return;

        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    // シーン読み込み完了時
    private void OnSceneLoadCompleted(
        string sceneName,
        LoadSceneMode mode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (!IsServer || sceneName != "GameScene")
            return;

        // プレイヤーごとに出現位置を変更
        float startX = ((int)OwnerClientId - 1) * startSpacing;

        Vector2 spawnPosition = new Vector2(
            startX,
            spawnY
        );

        rb.position = spawnPosition;
        rb.velocity = Vector2.zero;

        moveInput = Vector2.zero;
        isGrounded = false;

        Debug.Log(
            "Player出現位置設定 OwnerID:" +
            OwnerClientId +
            " Position:" +
            spawnPosition
        );
    }

    public override void OnNetworkDespawn()
    {
        // イベント登録解除
        if (IsServer &&
            NetworkManager != null &&
            NetworkManager.SceneManager != null)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted
                -= OnSceneLoadCompleted;
        }

        moveInput = Vector2.zero;
        isGrounded = false;
    }
}
