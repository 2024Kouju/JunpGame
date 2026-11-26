using UnityEngine;

public class BouncingBall : MonoBehaviour
{
    [Header("プレイヤー検知")]
    public float detectionRange = 5f;

    [Header("ボール移動")]
    public float moveSpeed = 3f;

    [Header("吹き飛ばし")]
    public float bounceForce = 10f;

    private Rigidbody2D rb;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            Vector2 direction =
                (player.position - transform.position).normalized;

            rb.velocity = direction * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb =
                collision.gameObject.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                Vector2 direction =
                    (collision.transform.position - transform.position)
                    .normalized;

                playerRb.AddForce(direction * bounceForce,
                    ForceMode2D.Impulse);
            }
        }
    }
}