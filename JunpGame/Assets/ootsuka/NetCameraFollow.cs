
using UnityEngine;
using Unity.Netcode;

public class NetCameraFollow : MonoBehaviour
{
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    public float smoothSpeed = 10f;

    private Transform target;

    private void LateUpdate()
    {
        // ©•ª‚ª‘€ì‚µ‚Ä‚¢‚éPlayer‚ğ’T‚·
        if (target == null)
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

        // Player‚ªŒ©‚Â‚©‚ç‚È‚¯‚ê‚Î‰½‚à‚µ‚È‚¢
        if (target == null) return;

        // ƒJƒƒ‰‚ğ’Ç]‚³‚¹‚é
        Vector3 targetPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}
