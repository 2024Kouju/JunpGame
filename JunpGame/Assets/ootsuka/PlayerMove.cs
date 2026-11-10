
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMove : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float startSpacing = 3f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // ‰ñ“]‚ğ‹Ö~
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public override void OnNetworkSpawn()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

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

        // ‰ŠúˆÊ’u‚ÍHost‚ªŒˆ’è
        if (IsServer)
        {
            float startX =
                ((int)OwnerClientId - 1) * startSpacing;

            rb.position = new Vector2(startX, 0f);
        }
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.W)) y = 1f;
        if (Input.GetKey(KeyCode.S)) y = -1f;

        Vector2 input = new Vector2(x, y).normalized;

        SendInputServerRpc(input);
    }

    [ServerRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendInputServerRpc(Vector2 input)
    {
        moveInput = input;
    }

    private void FixedUpdate()
    {
        if (!IsSpawned || !IsServer)
            return;

        // Host‚ªˆÚ“®‚ğŠÇ—
        rb.velocity = moveInput * moveSpeed;
    }

    public override void OnNetworkDespawn()
    {
        moveInput = Vector2.zero;
    }
}
