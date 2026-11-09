using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class MovingFloor : MonoBehaviour
{
    public float moveHeight = 3f;
    public float speed = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float y = Mathf.PingPong(
            Time.time * speed,
            moveHeight
        );

        transform.position = startPosition
            + new Vector3(0f, y, 0f);
    }
}