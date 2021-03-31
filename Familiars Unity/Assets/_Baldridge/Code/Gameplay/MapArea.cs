using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Encounter> wildFamiliars;

    public List<Familiar> GetRandomWildFamiliars()
    {
        List<Familiar> encounter = new List<Familiar>();
        for (int i = 0; i < 3; i++)
        {
            var wildFamiliar = wildFamiliars[Random.Range(0, wildFamiliars.Count)];
            Familiar newFamiliar = new Familiar(wildFamiliar.Base, wildFamiliar.Level);
            newFamiliar.Init();
            encounter.Add(newFamiliar);
        }
        return encounter;
    }
}

[System.Serializable]
public class Encounter
{
    [SerializeField] FamiliarBase fBase;
    [SerializeField] int level;

    public FamiliarBase Base
    {
        get { return fBase; }
    }

    public int Level
    {
        get { return level; }
    }
}