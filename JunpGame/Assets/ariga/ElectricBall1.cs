using UnityEngine;
using System.Collections;

public class ElectricBall : MonoBehaviour
{
    //プレイヤーの移動スクリプト
    public MonoBehaviour playerMovement;

    //行動不能になる時間
    public float stunTime = 2f;

    //一度だけ反応させる
    private bool isTouched = false;

    //プレイヤーのRigidbody2D
    private Rigidbody2D playerRb;

    //プレイヤーが触れたときに実行
    private void OnTriggerEnter2D(Collider2D other)
    {
        //すでに触れていたら何もしない
        if (isTouched)
            return;

        //相手がPlayerか確認する
        if (other.CompareTag("Player"))
        {
            isTouched = true;

            //プレイヤーのRigidbody2Dを取得
            playerRb = other.GetComponent<Rigidbody2D>();

            //移動スクリプトを停止
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            //横方向の速度を0にする
            if (playerRb != null)
            {
                playerRb.velocity = new Vector2(
                    0f,
                    playerRb.velocity.y
                );
            }

            //行動不能の処理を開始
            StartCoroutine(RecoverPlayer());
        }
    }

    //行動不能を解除する処理
    IEnumerator RecoverPlayer()
    {
        //2秒間待つ
        yield return new WaitForSeconds(stunTime);

        //プレイヤーの移動を再開
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        //ビリビリ玉を消す
        Destroy(gameObject);
    }
}