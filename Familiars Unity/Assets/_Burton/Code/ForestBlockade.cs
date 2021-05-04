using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestBlockade : MonoBehaviour
{
    public GameObject toPortalBlockade;
    public GameObject toTownBlockade;

    void Start()
    {
        InvokeRepeating("CheckPlayerBlockadeStatus", 0, 5);
    }

    void CheckPlayerBlockadeStatus()
    {
        if (PlayerPrefs.GetInt("ToPortal") == 1)
        {
            toPortalBlockade.SetActive(false);
        }
        else
        {
            toPortalBlockade.SetActive(true); 
        }

        if (PlayerPrefs.GetInt("ForestToTown") == 1)
        {
            toTownBlockade.SetActive(false);
        }
        else
        {
            toTownBlockade.SetActive(true);
        }
    }
}
