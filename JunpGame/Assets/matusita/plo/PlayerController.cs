//プレイヤーを動かすプログラム


using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private Rigidbody2D rb;
    private bool isGrounded = true;
    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 動けない場合は操作を受け付けない
        if (!canMove)
        {
            return;
        }

        // A Dで左右移動
        float moveX = 0;

        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveX = 1;
        }

        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // Wでジャンプ
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 地面に触れたらジャンプ可能
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        // Enemyに触れたら2秒間動けなくする
        if (collision.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(StopMovement());
        }
    }

    IEnumerator StopMovement()
    {
        canMove = false;

        // 横方向の速度を0にする
        rb.velocity = new Vector2(0, rb.velocity.y);

        // 2秒待つ
        yield return new WaitForSeconds(2f);

        // 操作可能に戻す
        canMove = true;
    }
}
