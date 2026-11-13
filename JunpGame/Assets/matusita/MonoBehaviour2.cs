
using UnityEngine;

public class Spring2 : MonoBehaviour
{
    public float bounceForce = 15f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

            rb.velocity = new Vector2(bounceForce, rb.velocity.y);
        }
    }
}
