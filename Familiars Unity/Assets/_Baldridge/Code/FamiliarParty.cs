using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FamiliarParty : MonoBehaviour
{
    [SerializeField] List<Familiar> familiars;

    private void Start()
    {
        foreach (var familiar in familiars)
        {
            familiar.Init();
        }
    }

    public Familiar GetHealthyFamiliar()
    {
        return familiars.Where(x => x.HP > 0).FirstOrDefault();
    }
}
