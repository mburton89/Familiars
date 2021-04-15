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
