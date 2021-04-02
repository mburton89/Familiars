using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    public void SetPosition(Node node)
    {
        transform.position = node.transform.position;
    }
}
