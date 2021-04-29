using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownBlockadeManager : MonoBehaviour
{
    public GameObject townToForestBlockade;
    public GameObject townToCaveBlockade;

    void Start()
    {
        InvokeRepeating("CheckPlayerBlockadeStatus", 0, 5);
    }

    void CheckPlayerBlockadeStatus()
    {
        if (PlayerPrefs.GetInt("TownToForest") == 1)
        {
            townToForestBlockade.SetActive(false);
        }
        else
        {
            townToForestBlockade.SetActive(true); 
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
