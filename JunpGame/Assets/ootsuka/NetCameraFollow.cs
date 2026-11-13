
using UnityEngine;
using Unity.Netcode;

public class NetCameraFollow : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private Transform target;

    private void LateUpdate()
    {
        // ©•ª‚ª‘€ì‚µ‚Ä‚¢‚éPlayer‚ğ’T‚·
        if (target == null)
        {
            FindTarget();
        }

        if (target == null)
            return;

        // •âŠÔ‚¹‚¸APlayer‚ÌˆÊ’u‚É’¼Ú’Ç]
        transform.position = target.position + offset;
    }

    private void FindTarget()
    {
        PlayerMove[] players =
            FindObjectsByType<PlayerMove>(
                FindObjectsSortMode.None
            );

        foreach (PlayerMove player in players)
        {
            if (player.IsSpawned && player.IsOwner)
            {
                target = player.transform;
                break;
            }
        }
    }
}
