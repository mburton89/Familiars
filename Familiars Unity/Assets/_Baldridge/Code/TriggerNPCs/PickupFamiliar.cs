using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupFamiliar : MonoBehaviour
{
    [SerializeField] FirstFamiliarSelection familiarSelection;

    bool activatable;

    void Update()
    {
        if (activatable && Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("[PickupFamiliar.cs] check");
            familiarSelection?.gameObject.SetActive(true);
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            activatable = true;        
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            activatable = false;
        }
    }
}
