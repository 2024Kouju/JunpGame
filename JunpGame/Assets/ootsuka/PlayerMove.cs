using UnityEngine;
using Unity.Netcode;

public class PlayerMove : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private Vector2 moveInput;

    public override void OnNetworkSpawn()
    {
        // サーバー側で初期位置を設定
        if (IsServer)
        {
            transform.position = new Vector3(
                OwnerClientId * 3f,
                0f,
                0f
            );
        }

        // Playerの色を設定
        SpriteRenderer sr =
            GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            // Host = 赤、Client = 青
            sr.color = OwnerClientId == 0
                ? Color.red
                : Color.blue;
        }
    }

    void Update()
    {
        // 自分のプレイヤーだけ操作
        if (!IsSpawned || !IsOwner)
            return;

        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.W)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;

        moveInput = new Vector2(x, y).normalized;
    }

    void FixedUpdate()
    {
        if (!IsSpawned || !IsOwner)
            return;

        // サーバーに移動入力を送信
        MoveRpc(moveInput);
    }

    [Rpc(SendTo.Server)]
    private void MoveRpc(Vector2 direction)
    {
        Vector3 movement = new Vector3(
            direction.x,
            direction.y,
            0f
        );

        transform.position +=
            movement * moveSpeed * Time.fixedDeltaTime;
    }
}
