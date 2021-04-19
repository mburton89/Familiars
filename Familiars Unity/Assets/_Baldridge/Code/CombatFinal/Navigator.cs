using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigator : MonoBehaviour
{
    public void SetLocation(Tile t)
    {
        this.gameObject.transform.position = t.gameObject.transform.position;
    }

    public void SetActive(bool active)
    {
        this.gameObject.SetActive(active);
    }
}
