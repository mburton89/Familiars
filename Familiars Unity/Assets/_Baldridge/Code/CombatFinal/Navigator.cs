using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Navigator : MonoBehaviour
{
    public void SetLocation(Tile t)
    {
        this.gameObject.transform.position = t.gameObject.transform.position;
    }

    public void SetActive(bool active)
    {
        Debug.Log("[Navigator.cs/SetActive( " + active + ")]");
        this.gameObject.GetComponent<Image>().enabled = active;
    }
}
