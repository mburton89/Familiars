using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InScenePortal : MonoBehaviour
{
    [SerializeField] Transform transportPoint;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.transform.position = transportPoint.position;
        }
    }
}
