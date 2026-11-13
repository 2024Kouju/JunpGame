
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D playerRb;

    public float speed = 5f;

    private Vector3 originalScale;

    private bool isDamaged = false;
    private bool canMove = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isDamaged)
        {
            isDamaged = true;
            canMove = false;

            // ダメージアニメーション
            animator.SetTrigger("DamageTrigger");

            // 移動を停止
            playerRb.velocity = new Vector2(0, playerRb.velocity.y);

            // 2秒後に操作を戻す
            Invoke(nameof(ResetDamage), 2f);
        }
    }

    private void ResetDamage()
    {
        isDamaged = false;
        canMove = true;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody2D>();

        originalScale = transform.localScale;
    }

    void Update()
    {
        // 操作できない間は移動処理を行わない
        if (!canMove)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        float move = Input.GetAxisRaw("Horizontal");

        playerRb.velocity =
            new Vector2(move * speed, playerRb.velocity.y);

        animator.SetBool("isWalking", move != 0);

        bool isJump = Mathf.Abs(playerRb.velocity.y) > 0.1f;
        animator.SetBool("isJump", isJump);

        if (move > 0)
        {
            transform.localScale =
                new Vector3(-originalScale.x,
                            originalScale.y,
                            originalScale.z);
        }
        else if (move < 0)
        {
            transform.localScale =
                new Vector3(originalScale.x,
                            originalScale.y,
                            originalScale.z);
        }
    }
}
