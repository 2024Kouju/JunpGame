using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerKeep : MonoBehaviour
{
    private static NetworkManagerKeep instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
