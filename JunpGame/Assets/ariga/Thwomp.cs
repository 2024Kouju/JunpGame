using UnityEngine;
using System.Collections;

public class Thwomp : MonoBehaviour
{
    //プレイヤーの位置を取得するための変数
    public Transform player;

    //プレイヤーが岩に近づいたと判断する距離
    public float triggerDistance = 3f;

    //プレイヤーが離れたと判断する距離
    public float resetDistance = 5f;

    //岩が落下する距離
    public float fallDistance = 4f;

    //岩が落下するときの速さ
    public float fallSpeed = 10f;

    //岩が上に戻るときの速さ
    public float riseSpeed = 3f;

    //岩が地面に着いてから停止する時間
    public float waitTime = 1.5f;

    //岩を物理的に動かすための変数
    private Rigidbody2D rb;

    //岩が最初にあった位置を記録する変数
    private Vector2 startPosition;

    //岩が動いているかどうかを確認する変数
    //false = 動いていない
    // true = 動いている
    private bool isMoving = false;


    //ゲーム開始時に1回だけ実行される
    void Start()
    {



        //岩についているRigidbody2Dを取得する
        rb = GetComponent<Rigidbody2D>();

    
        //ゲーム開始時の岩の位置を保存する
        startPosition = rb.position;
    }



    //毎フレーム繰り返し実行される
    void Update()
    {
        //プレイヤーが設定されていない場合や
        //岩がすでに動いている場合は何もしない
        if (player == null || isMoving)
            return;

        //岩とプレイヤーの距離を計算する
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        //プレイヤーが設定した距離まで近づいたら
        //岩を動かす処理を開始する
        if (distance <= triggerDistance)
        {
            StartCoroutine(ThwompAction());
        }
    }


    //岩が落下して戻るまでの動きをまとめた処理
    IEnumerator ThwompAction()
    {
        //岩が動いている状態にする
        //これで何度も落下するのを防ぐ
        isMoving = true;

        //岩が落ちる場所を計算する
        //最初の位置から下にfallDistance分移動する
        Vector2 bottomPosition =
            startPosition - Vector2.up * fallDistance;
        
        //岩が落下地点に到着するまで繰り返す
        while (Vector2.Distance(
            rb.position, bottomPosition) > 0.02f)
        {
            //岩を少しずつ落下地点に近づける
            rb.MovePosition(Vector2.MoveTowards(
                rb.position,
                bottomPosition,
                fallSpeed * Time.fixedDeltaTime
            ));

            //次の物理演算のタイミングまで待つ
            yield return new WaitForFixedUpdate();
        }


        //地面に着いたら設定した時間だけ停止する
        yield return new WaitForSeconds(waitTime);

        //岩が最初の位置に戻るまで繰り返す
        while (Vector2.Distance(
            rb.position, startPosition) > 0.02f)
        {
            //岩を少しずつ最初の位置に近づける
            rb.MovePosition(Vector2.MoveTowards(
                rb.position,
                startPosition,
                riseSpeed * Time.fixedDeltaTime
            ));

            //次の物理演算のタイミングまで待つ
            yield return new WaitForFixedUpdate();
        }


        //
        //プレイヤーが設定した距離以上に離れるまで待つ
        while (player != null &&
            Vector2.Distance(transform.position,
            player.position) <= resetDistance)
        {
            //次のフレームまで待つ
            yield return null;
        }

        //プレイヤーが離れたら
        //岩を再び動かせる状態にする
        isMoving = false;
    }
}