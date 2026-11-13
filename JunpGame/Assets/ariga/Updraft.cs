using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Updraft : MonoBehaviour
{
    //風の強さ
    public float windPower = 5f;

    //プレイヤーが上昇できる最大速度
    public float maxUpSpeed = 3.5f;

    //風の範囲に触れている間、繰り返し実行
    private void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log("上昇気流に接触中：" + other.name);

        //プレイヤーのRigidbody2Dを取得
        //子オブジェクトにColliderがあっても取得できる
        Rigidbody2D rb = other.attachedRigidbody;

        //Rigidbody2Dがなければ終了
        if (rb == null) return;

        //プレイヤーかどうか確認
        if (!rb.CompareTag("Player") && !other.CompareTag("Player"))
        {
            return;
        }

        //上方向に風の力を加える
        rb.AddForce(Vector2.up * windPower);

        //上昇速度が最大速度を超えたら制限
        if (rb.velocity.y > maxUpSpeed)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                maxUpSpeed
            );
        }
    }
}