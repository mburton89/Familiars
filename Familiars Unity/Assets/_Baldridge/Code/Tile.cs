using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TileState { Normal, ActiveSource, Source, Move, Switch, ActiveTarget, Target, AllyTarget, TargetReticle, AllyTargetReticle }

public class Tile : MonoBehaviour
{
    public CombatUnit familiarOccupant;
    public GameObject hazardOccupant;

    public int x;
    public int y;

    public TileState currentState;

    public bool playerField;

    [SerializeField] bool mainField;

    [SerializeField] CombatHandler ch;
    [SerializeField] Image sprite;

    [HideInInspector] public bool visited;
    [HideInInspector] public bool selectable;
    [HideInInspector] public int distance;
    [HideInInspector] public Tile parent;
    [HideInInspector] public bool current;

    private List<Tile> neighbors = new List<Tile>();

    // Start is called before the first frame update
    void Start()
    {
        currentState = TileState.Normal;
    }

    public void SetState(TileState st)
    {
        currentState = st;
        switch (currentState)
        {
            case TileState.Normal:
                sprite.color = Color.white;
                break;
            case TileState.ActiveSource:
                sprite.color = new Color(0f, 0.5f, 1f);
                break;
            case TileState.Source:
                sprite.color = Color.cyan;
                break;
            case TileState.Move:
                sprite.color = Color.blue;
                break;
            case TileState.Switch:
                sprite.color = Color.blue;
                break;
            case TileState.ActiveTarget:
                sprite.color = new Color(0.81f, 0.01f, 0.01f);
                break;
            case TileState.Target:
                sprite.color = Color.red;
                break;
            case TileState.AllyTarget:
                sprite.color = Color.green;
                break;
            case TileState.TargetReticle:
                sprite.color = new Color(0.45f, 0f, 0.02f);
                break;
            case TileState.AllyTargetReticle:
                sprite.color = new Color(0.13f, 0.5f, 0.19f);
                break;
            default:
                currentState = TileState.Normal;
                sprite.color = Color.white;
                break;
        }
    }

    public TileState GetState()
    {
        return currentState;
    }


    public void RefreshTile()
    {
        familiarOccupant = null;
        hazardOccupant = null;
        SetState(TileState.Normal);
    }

    public List<Tile> GetNeighbors()
    {
        return neighbors;
    }

    public void AddNeighbors(Tile t)
    {
        neighbors.Add(t);
    }

    public void Reset()
    {
        current = false;
        //target = false;
        selectable = false;
        //walkable = true;

        visited = false;
        parent = null;
        distance = 0;
        SetState(TileState.Normal);
    }
}
