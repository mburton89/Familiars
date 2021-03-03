using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentFamiliarsController : MonoBehaviour
{
    public static CurrentFamiliarsController Instance;

    public List<Familiar> playerFamiliars;
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
        playerFamiliars = new List<Familiar>();
        playerFamiliars = newPlayerFamiliars;
        for (int i = 0; i < playerFamiliars.Count; i++)
        {
            playerFamiliars[i].Init();
        }
    }

    public void UpdateEnemyFamiliars(List<Familiar> newEnemyFamiliars)
    {
        enemyFamiliars = new List<Familiar>();
        enemyFamiliars = newEnemyFamiliars;
        for (int i = 0; i < enemyFamiliars.Count; i++)
        {
            enemyFamiliars[i].Init();
        }
    }
}
