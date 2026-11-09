using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D playerRb;

    public float speed = 5f;

    private Vector3 originalScale;

    private bool isDamaged = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isDamaged)
        {
            isDamaged = true;

            animator.SetTrigger("DamageTrigger");

            Invoke(nameof(ResetDamage), 0.5f);
        }
    }

    private void ResetDamage()
    {
        isDamaged = false;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        playerRb = GetComponent<Rigidbody2D>();

        originalScale = transform.localScale;
    }

    void Update()
    {
        float move = Input.GetAxisRaw("Horizontal");

        playerRb.velocity =
            new Vector2(move * speed, playerRb.velocity.y);

        animator.SetBool("isWalking", move != 0);

        if (Input.GetKeyDown(KeyCode.W))
        {
            animator.SetTrigger("JumpTrigger");
        }

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