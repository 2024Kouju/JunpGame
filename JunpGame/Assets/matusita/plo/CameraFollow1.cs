//カメラがプレイヤーを追従するプログラム
using UnityEngine;

public class CameraFollow1 : MonoBehaviour
{
    public Transform player;
    public float offsetZ = -10f;

    void LateUpdate()
    {
        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            offsetZ
        );
    }
}