
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMove : NetworkBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 5f;
    public float startSpacing = 1f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;

    [Header("Spring Settings")]
    public float springJumpForce = 15f;

    [Header("GameScene Spawn")]
    public float spawnY = -1f;

    [Header("Animation Settings")]
    public float animationPlaybackSpeed = 1f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private LobbyPlayer lobbyPlayer;

    private Vector2 moveInput;
    private bool isGrounded = false;

    private Vector3 previousPosition;

    // 接触している足場を個別に管理
    private HashSet<Collider2D> groundColliders =
        new HashSet<Collider2D>();

    // 接地状態を全員に同期
    private NetworkVariable<bool> Grounded =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private bool IsGameScene()
    {
        return SceneManager.GetActiveScene().name == "GameScene";
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lobbyPlayer = GetComponent<LobbyPlayer>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (animator != null)
        {
            animator.speed = animationPlaybackSpeed;
        }
    }

    public override void OnNetworkSpawn()
    {
        previousPosition = transform.position;

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
        if (!IsGameScene() || !IsSpawned)
            return;

        UpdateAnimation();

        if (!IsOwner)
            return;

        float x = 0f;

        if (Input.GetKey(KeyCode.A))
            x = -1f;

        if (Input.GetKey(KeyCode.D))
            x = 1f;

        Vector2 input = new Vector2(x, 0f).normalized;

        SendInputServerRpc(input);

        if (Input.GetKeyDown(KeyCode.W))
        {
            JumpServerRpc();
        }
    }

    // アニメーション更新
    private void UpdateAnimation()
    {
        if (animator == null || !animator.enabled)
            return;

        if (spriteRenderer == null)
            return;

        float deltaX =
            transform.position.x - previousPosition.x;

        float speed =
            Mathf.Abs(deltaX) /
            Mathf.Max(Time.deltaTime, 0.0001f);

        animator.SetFloat("Speed", speed);

        // ネットワーク同期された接地状態を使用
        animator.SetBool("Grounded", Grounded.Value);

        // キャラクターごとの左右反転
        bool reverseFlipDirection = false;

        if (lobbyPlayer != null)
        {
            int characterIndex =
                lobbyPlayer.SelectedCharacter.Value;

            reverseFlipDirection =
                characterIndex == 1 ||
                characterIndex == 2;
        }

        if (deltaX > 0.001f)
        {
            spriteRenderer.flipX = !reverseFlipDirection;
        }
        else if (deltaX < -0.001f)
        {
            spriteRenderer.flipX = reverseFlipDirection;
        }

        previousPosition = transform.position;
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

        // ジャンプ時に接地状態を解除
        ClearGroundContacts();
    }

    private void FixedUpdate()
    {
        if (!IsGameScene())
            return;

        if (!IsSpawned || !IsServer)
            return;

        rb.velocity = new Vector2(
            moveInput.x * moveSpeed,
            rb.velocity.y
        );
    }

    // 足場との接触判定
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer || !IsGameScene())
            return;

        // バネに着地したとき
        if (collision.collider.CompareTag("Spring"))
        {
            if (rb.velocity.y <= 0f &&
                rb.position.y > collision.transform.position.y)
            {
                rb.velocity = new Vector2(
                    rb.velocity.x,
                    springJumpForce
                );

                ClearGroundContacts();

                Debug.Log("バネでジャンプ！");
            }

            return;
        }

        // 通常の足場
        if (collision.collider.CompareTag("Ground"))
        {
            UpdateGroundContact(collision);
        }
    }

    // 接触し続けている間も判定
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsServer || !IsGameScene())
            return;

        if (collision.collider.CompareTag("Ground"))
        {
            UpdateGroundContact(collision);
        }
    }

    // 足場との接触が終了したとき
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!IsServer || !IsGameScene())
            return;

        if (collision.collider.CompareTag("Ground"))
        {
            groundColliders.Remove(collision.collider);

            UpdateGroundedState();
        }
    }

    // 足場との接触状態を更新
    private void UpdateGroundContact(Collision2D collision)
    {
        // 上向きの面に接触しているか確認
        bool standingOnGround = false;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                standingOnGround = true;
                break;
            }
        }

        // 上昇中は接地扱いにしない
        if (rb.velocity.y > 0.1f)
            return;

        if (standingOnGround)
        {
            groundColliders.Add(collision.collider);
        }

        UpdateGroundedState();
    }

    // 接地状態を更新
    private void UpdateGroundedState()
    {
        groundColliders.RemoveWhere(
            collider => collider == null
        );

        isGrounded = groundColliders.Count > 0;

        Grounded.Value = isGrounded;
    }

    // 接地状態を解除
    private void ClearGroundContacts()
    {
        groundColliders.Clear();

        isGrounded = false;
        Grounded.Value = false;
    }

    // GameSceneへの移動完了時
    private void OnSceneLoadCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (sceneName != "GameScene")
            return;

        if (!IsServer)
            return;

        var connectedIds =
            NetworkManager.Singleton.ConnectedClientsIds;

        ulong[] clientIds =
            new ulong[connectedIds.Count];

        for (int i = 0; i < connectedIds.Count; i++)
        {
            clientIds[i] = connectedIds[i];
        }

        Array.Sort(clientIds);

        int playerIndex =
            Array.IndexOf(clientIds, OwnerClientId);

        // 左から等間隔に配置
        float x =
            (playerIndex - (clientIds.Length - 1) / 2f)
            * startSpacing;

        transform.position = new Vector3(
            x,
            spawnY,
            0f
        );

        rb.velocity = Vector2.zero;
        moveInput = Vector2.zero;

        ClearGroundContacts();

        previousPosition = transform.position;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer &&
            NetworkManager != null &&
            NetworkManager.SceneManager != null)
        {
            NetworkManager.SceneManager.OnLoadEventCompleted
                -= OnSceneLoadCompleted;
        }

        moveInput = Vector2.zero;

        ClearGroundContacts();
    }
}
