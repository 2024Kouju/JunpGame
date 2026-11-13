using UnityEngine;
using System.Collections;

public class Balloon : MonoBehaviour
{
    //風船が上昇する速さ
    public float riseSpeed = 2f;

    //風船が空気を保てる時間
    public float airTime = 5f;

    //風船がしぼむ時間
    public float deflateTime = 0.5f;

    //プレイヤーの移動スクリプト
    public MonoBehaviour playerMovement;

    //上昇中かどうか
    private bool isRising = false;

    //風船のRigidbody2D
    private Rigidbody2D rb;

    //風船のCollider
    private Collider2D balloonCollider;

    //プレイヤーのRigidbody2D
    private Rigidbody2D playerRb;

    //プレイヤーの元の親
    private Transform originalParent;

    //プレイヤーの元の重力
    private float originalGravity;

    //プレイヤーの元の物理設定
    private bool originalSimulated;

    //プレイヤーの移動スクリプトが有効だったか
    private bool originalMovementEnabled;

    //風船の最初の大きさ
    private Vector3 originalScale;


    //ゲーム開始時に実行
    void Start()
    {
        //Rigidbody2Dを取得
        rb = GetComponent<Rigidbody2D>();

        //Colliderを取得
        balloonCollider = GetComponent<Collider2D>();

        //最初の大きさを保存
        originalScale = transform.localScale;
    }


    //プレイヤーが風船に乗ったとき
    void OnCollisionEnter2D(Collision2D collision)
    {
        //すでに上昇していたら何もしない
        if (isRising)
            return;

        //相手がPlayerか確認
        if (collision.gameObject.CompareTag("Player"))
        {
            //プレイヤーのColliderを取得
            Collider2D playerCollider =
                collision.gameObject.GetComponent<Collider2D>();

            //プレイヤーが風船の上側にいるか確認
            if (playerCollider != null &&
                playerCollider.bounds.min.y >=
                balloonCollider.bounds.center.y)
            {
                //プレイヤーのRigidbody2Dを取得
                playerRb =
                    collision.gameObject.GetComponent<Rigidbody2D>();

                if (playerRb == null)
                    return;

                //上昇を開始
                StartCoroutine(RiseBalloon());
            }
        }
    }


    //風船が上昇する処理
    IEnumerator RiseBalloon()
    {
        isRising = true;

        //プレイヤーの元の設定を保存
        originalParent = playerRb.transform.parent;
        originalGravity = playerRb.gravityScale;
        originalSimulated = playerRb.simulated;

        if (playerMovement != null)
        {
            originalMovementEnabled = playerMovement.enabled;

            //プレイヤーの操作を一時停止
            playerMovement.enabled = false;
        }

        //プレイヤーの物理演算を一時停止
        playerRb.simulated = false;

        //プレイヤーを風船に乗せる
        playerRb.transform.SetParent(transform, true);


        //空気がなくなるまで上昇する
        float timer = 0f;

        while (timer < airTime)
        {
            //風船を上に動かす
            rb.MovePosition(
                rb.position +
                Vector2.up * riseSpeed * Time.fixedDeltaTime
            );

            //時間を進める
            timer += Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }


        
        //プレイヤーを風船から切り離す
        playerRb.transform.SetParent(originalParent, true);

        //プレイヤーの物理演算を戻す
        playerRb.simulated = originalSimulated;

        //重力を元に戻す
        playerRb.gravityScale = originalGravity;

        //プレイヤーの操作を戻す
        if (playerMovement != null)
        {
            playerMovement.enabled = originalMovementEnabled;
        }

        //風船の当たり判定を消す
        balloonCollider.enabled = false;

        //風船が小さくなる
        float deflateTimer = 0f;

        while (deflateTimer < deflateTime)
        {
            float rate = deflateTimer / deflateTime;

            transform.localScale =
                Vector3.Lerp(originalScale, Vector3.zero, rate);

            deflateTimer += Time.deltaTime;

            yield return null;
        }

        //風船を完全に消す
        Destroy(gameObject);
    }
}