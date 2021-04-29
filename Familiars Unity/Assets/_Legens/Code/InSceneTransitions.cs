using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InSceneTransitions : MonoBehaviour
{
    public Transform transformToBe;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            collision.transform.position = transformToBe.position;
        }
    }
}
