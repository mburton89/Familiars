using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    public List<Familiar> allFamiliars;
    public List<Familiar> aliveFamiliars;
    public List<Familiar> deadFamiliars;
    public Familiar activeFamiliar;

    public FamiliarBase Familiar0Base;
    public int Familiar0Level;
    public int Familiar0HP;
    public List<Attack> Familiar0Attacks;

    public FamiliarBase Familiar1Base;
    public int Familiar1Level;
    public int Familiar1HP;
    public List<Attack> Familiar1Attacks;

    public FamiliarBase Familiar2Base;
    public int Familiar2Level;
    public int Familiar2HP;
    public List<Attack> Familiar2Attacks;

    private void Start()
    {
        InitFamiliars();
    }

    public void InitFamiliars()
    {
        Familiar familiar0 = new Familiar(Familiar0Base, 1);
        allFamiliars.Add(familiar0);

        Familiar familiar1 = new Familiar(Familiar1Base, 1);
        allFamiliars.Add(familiar1);

        Familiar familiar2 = new Familiar(Familiar2Base, 2);
        allFamiliars.Add(familiar2);

        //allFamiliars[0].Base = Familiar0Base;
        //allFamiliars[0].Level = Familiar0Level;
        //allFamiliars[0].HP = Familiar0HP;
        //allFamiliars[0].Attacks = Familiar0Attacks;

        //allFamiliars[1].Base = Familiar1Base;
        //allFamiliars[1].Level = Familiar1Level;
        //allFamiliars[1].HP = Familiar1HP;
        //allFamiliars[1].Attacks = Familiar1Attacks;

        //allFamiliars[2].Base = Familiar2Base;
        //allFamiliars[2].Level = Familiar2Level;
        //allFamiliars[2].HP = Familiar2HP;
        //allFamiliars[2].Attacks = Familiar2Attacks;
    }
}
