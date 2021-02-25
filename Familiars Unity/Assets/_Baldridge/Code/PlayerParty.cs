using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParty : FamiliarParty
{
    public static PlayerParty Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CurrentFamiliarsController.Instance.UpdatePlayerFamiliars(familiars);
    }
}
