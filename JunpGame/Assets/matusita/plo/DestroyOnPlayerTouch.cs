using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//オブジェクトを消すプログラム　霧

public class DestroyOnPlayerTouch : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}