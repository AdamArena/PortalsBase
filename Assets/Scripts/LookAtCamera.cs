using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 objectPosition = transform.position;
        objectPosition.y = 0;
        cameraPosition.y = 0;
        Vector3 direction = objectPosition - cameraPosition;
        transform.LookAt(transform.position - direction);
    }
}
