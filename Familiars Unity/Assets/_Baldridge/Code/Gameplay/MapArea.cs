using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Familiar> wildFamiliars;

    public List<Familiar> GetRandomWildFamiliars()
    {
        List<Familiar> encounter = new List<Familiar>();
        for (int i = 0; i < 3; i++)
        {
            var wildFamiliar = wildFamiliars[Random.Range(0, wildFamiliars.Count)];
            wildFamiliar.Init();
            encounter.Add(wildFamiliar);
        }
        return encounter;
    }
}
