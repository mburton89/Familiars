using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlockadeManager : MonoBehaviour
{
    public static PlayerBlockadeManager Instance;

    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //TEST STUFF
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            PlayerPrefs.DeleteAll();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UnlockTownToForest();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UnlockTownToCave();
        }
    }

    public void UnlockTownToForest()
    {
        PlayerPrefs.SetInt("TownToForest", 1);
    }

    public void UnlockTownToCave()
    {
        PlayerPrefs.SetInt("TownToCave", 1);
    }

    public void UnlockForestToCave()
    {
        PlayerPrefs.SetInt("ForestToCave", 1);
    }
}
