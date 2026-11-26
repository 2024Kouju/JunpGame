using UnityEngine;

// Jキー連打で上昇する足場
public class LiftBlock : MonoBehaviour
{
    // 1回押した時の上昇量
    public float stepHeight = 0.3f;

    // 下降速度
    public float downSpeed = 1.5f;

    // J入力がないと下降開始する時間
    public float waitTime = 1.0f;

    // プレイヤーが上に乗っているか
    private bool playerOnTop = false;

    // 最後にJを押した時間
    private float lastPressTime;

    // 初期位置
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        lastPressTime = Time.time;
    }

    void Update()
    {
        // プレイヤーが上にいてJを押した
        if (playerOnTop && Input.GetKeyDown(KeyCode.J))
        {
            transform.position += Vector3.up * stepHeight;

            lastPressTime = Time.time;
        }

        // 一定時間入力がなければ下降
        if (Time.time - lastPressTime > waitTime)
        {
            if (transform.position.y > startPosition.y)
            {
                transform.position -=
                    Vector3.up * downSpeed * Time.deltaTime;

                // 初期位置より下に行かない
                if (transform.position.y < startPosition.y)
                {
                    transform.position = startPosition;
                }
            }
        }
    }

    // プレイヤーが足場の上に乗った
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // プレイヤーが上側にいる場合のみ有効
            foreach (ContactPoint2D point in collision.contacts)
            {
                if (point.normal.y < -0.5f)
                {
                    playerOnTop = true;
                    return;
                }
            }
        }
    }

    // プレイヤーが離れた
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnTop = false;
        }
    }
}