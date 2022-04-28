using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBackForth: MonoBehaviour
{
    float speed = .2f;
    float distance = 8;
    float startZ;

    // Start is called before the first frame update
    void Start()
    {
        startZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = transform.position;
        newPosition.z = Mathf.SmoothStep(startZ + distance, startZ - distance, Mathf.PingPong(Time.time * speed, 1));
        transform.position = newPosition;
    }
}