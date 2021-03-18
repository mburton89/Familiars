using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
public class CombatAvatar : MonoBehaviour
{
    [SerializeField] FamiliarBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;
    [SerializeField] HpBar miniHPBar;

    [SerializeField] int x;
    [SerializeField] int y;

    public int position;

    private CombatManager combatManager;

    List<Node> selectableNodes = new List<Node>();
    Stack<Node> path = new Stack<Node>();

    Node currentNode;

    public Familiar Familiar { get; set; }

    public void Setup()
    {
        Familiar = new Familiar(_base, level);
        combatManager = GameObject.Find("CombatManager").GetComponent<CombatManager>();
        position = (x * 3) + y;

        if (isPlayerUnit)
        {
            this.transform.localScale = new Vector3(1, 1, 1);
            combatManager.playerTeam.field[position].familiarOccupant = this;
        }   
        else
        {
            this.transform.localScale = new Vector3(-1, 1, 1);
            combatManager.enemyTeam.field[position].familiarOccupant = this;
        }

        this.GetComponent<Image>().sprite = Familiar.Base.FamiliarSprite;

        miniHPBar.SetHP((float)Familiar.HP / Familiar.MaxHp);

        

    }
}
*/