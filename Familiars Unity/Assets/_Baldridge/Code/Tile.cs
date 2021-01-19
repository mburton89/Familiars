using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum State {Normal, Move, Switch, Target, AllyTarget}
    public CombatAvatar familiarOccupant;
    public GameObject hazardOccupant;

    public int x;
    public int y;

    public State currentState;

    private CombatManager cm;
    [SerializeField] private SpriteRenderer sprite;

    private List<Tile> neighbors = new List<Tile>();

    // Start is called before the first frame update
    void Start()
    {
        cm = GameObject.Find("CombatManager").GetComponent<CombatManager>();
        currentState = State.Normal;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
    private void OnMouseDown()
    {
        switch (currentState)
        {
            case State.Normal:
                cm.selectedFamiliar = familiarOccupant;
                if (familiarOccupant != null)
                {
                    //cm.UpdateMenuState(BattleMenuState.Main);
                }
                else
                {
                    //cm.UpdateMenuState(BattleMenuState.None);
                }
                break;
            case State.Move:
                break;
            case State.Switch:
                break;
            case State.Target:
                break;
                

        }
       
        
        
    }*/

    public void SetState(State st)
    {
        currentState = st;
        switch (currentState)
        {
            case State.Normal:
                sprite.color = Color.white;
                break;
            case State.Move:
                sprite.color = Color.blue;
                break;
            case State.Switch:
                sprite.color = Color.blue;
                break;
            case State.Target:
                sprite.color = Color.red;
                break;
            case State.AllyTarget:
                sprite.color = Color.green;
                break;
            default:
                currentState = State.Normal;
                sprite.color = Color.white;
                break;
        }
    }


    public void RefreshTile()
    {
        familiarOccupant = null;
        hazardOccupant = null;
        SetState(State.Normal);
    }

    public List<Tile> GetNeighbors()
    {
        return neighbors;
    }

    public void AddNeighbors(Tile t)
    {
        neighbors.Add(t);
    }
}
