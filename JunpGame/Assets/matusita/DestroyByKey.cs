using UnityEngine;

// プレイヤーが触れているときに指定キーで消える
public class DestroyByKey : MonoBehaviour
{
    // Inspectorで変更できるキー
    public KeyCode destroyKey = KeyCode.E;

    // プレイヤーが触れているか
    private bool playerTouching = false;

    void Update()
    {
        // プレイヤーが触れていて指定キーを押した
        if (playerTouching && Input.GetKeyDown(destroyKey))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Playerタグに触れたら判定開始
        if (collision.gameObject.CompareTag("Player"))
        {
            playerTouching = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Playerタグから離れたら判定終了
        if (collision.gameObject.CompareTag("Player"))
        {
            playerTouching = false;
        }
    }
}