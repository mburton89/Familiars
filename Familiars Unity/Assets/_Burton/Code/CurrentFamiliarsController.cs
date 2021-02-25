using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentFamiliarsController : MonoBehaviour
{
    public static CurrentFamiliarsController Instance;

    public List<Familiar> playerFamilars;
    public List<Familiar> enemyFamiliars;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdatePlayerFamiliars(List<Familiar> newPlayerFamiliars)
    {
        playerFamilars = new List<Familiar>();
        playerFamilars = newPlayerFamiliars;
    }

    public void UpdateEnemyFamiliars(List<Familiar> newEnemyFamiliars)
    {
        enemyFamiliars = new List<Familiar>();
        enemyFamiliars = newEnemyFamiliars;
    }
}
