using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownBlockadeManager : MonoBehaviour
{
    public GameObject townToRouteBlockade;
    public GameObject townToCaveBlockade;

    void Start()
    {
        InvokeRepeating("CheckPlayerBlockadeStatus", 0, 5);
    }

    void CheckPlayerBlockadeStatus()
    {
        if (PlayerPrefs.GetInt("TownToForest") == 1)
        {
            townToRouteBlockade.SetActive(false);
        }
        else
        {
            townToRouteBlockade.SetActive(true); 
        }

        if (PlayerPrefs.GetInt("TownToCave") == 1)
        {
            townToCaveBlockade.SetActive(false);
        }
        else
        {
            townToCaveBlockade.SetActive(true);
        }
    }
    
}
