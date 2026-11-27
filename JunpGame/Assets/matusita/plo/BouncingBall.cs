using UnityEngine;

// プレイヤーを弾き飛ばし、時間経過で元の位置に戻る
public class BouncingBall : MonoBehaviour
{
    [Header("吹き飛ばす力")]
    public float bounceForce = 10f;

    [Header("戻るまでの時間")]
    public float returnTime = 3f;

    // 初期位置を保存
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // プレイヤーに当たったとき
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRb =
                collision.gameObject.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                // プレイヤーを弾き飛ばす方向
                Vector2 direction =
                    (collision.transform.position - transform.position).normalized;

                playerRb.AddForce(
                    direction * bounceForce,
                    ForceMode2D.Impulse);
            }

            // returnTime秒後に元の位置へ戻る
            Invoke(nameof(ReturnToStart), returnTime);
        }
    }

    // 初期位置へ戻す
    private void ReturnToStart()
    {
        transform.position = startPosition;
    }
}