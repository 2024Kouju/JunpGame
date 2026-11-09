
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameSceneCheck : MonoBehaviour
{
    void Start()
    {
        Debug.Log("================================");
        Debug.Log("GameSceneに移動しました！");
        Debug.Log("現在のシーン：" + SceneManager.GetActiveScene().name);

        if (NetworkManager.Singleton != null)
        {
            Debug.Log("通信状態：" +
                NetworkManager.Singleton.IsListening);

            Debug.Log("Host：" +
                NetworkManager.Singleton.IsHost);

            Debug.Log("Client：" +
                NetworkManager.Singleton.IsClient);
        }

        Debug.Log("================================");
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.green;

        GUI.Label(
            new Rect(30, 30, 600, 50),
            "GameSceneに移動成功！",
            style
        );
    }
}
