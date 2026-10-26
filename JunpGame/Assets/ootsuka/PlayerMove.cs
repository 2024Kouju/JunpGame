using UnityEngine;
using Unity.Netcode;

public class PlayerMove : NetworkBehaviour
{
    public float moveSpeed = 5f;

    // スタート位置の間隔
    public float startSpacing = 3f;

    public override void OnNetworkSpawn()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Host = 赤、2人目 = 青、3人目 = 黄色
        if (sr != null)
        {
            switch (OwnerClientId)
            {
                case 0:
                    sr.color = Color.red;
                    break;

                case 1:
                    sr.color = Color.blue;
                    break;

                case 2:
                    sr.color = Color.yellow;
                    break;

                default:
                    sr.color = Color.white;
                    break;
            }
        }

        // 自分が操作するPlayerだけ初期位置を設定
        if (IsOwner)
        {
            float startX = ((int)OwnerClientId - 1) * startSpacing;

            transform.position = new Vector3(
                startX,
                0f,
                0f
            );
        }
    }

    void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.W)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;

        Vector3 direction = new Vector3(x, y, 0f);

        transform.position +=
            direction.normalized *
            moveSpeed * Time.deltaTime;
    }
}