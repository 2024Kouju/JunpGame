
//ƒrƒŠƒrƒŠ‹Ê


using UnityEngine;

public class DestroyOnPlayerCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
