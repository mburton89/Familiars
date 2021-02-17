using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatUnit : MonoBehaviour
{
    [SerializeField] FamiliarBase _base;
    [SerializeField] int level;
    public bool isPlayerUnit;
    public int teamPosition;


    [SerializeField] Image sprite;

    [SerializeField] public int x;
    [SerializeField] public int y;


    List<Tile> selectableTiles = new List<Tile>();
    Stack<Tile> path = new Stack<Tile>();

    Tile currentTile;

    public Familiar Familiar { get; set; }

    public void Setup()
    {
        Familiar = new Familiar(_base, level);

        sprite.sprite = Familiar.Base.FamiliarSprite;

        if (isPlayerUnit)
        {
            this.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            this.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public Tile GetCurrentTile()
    {
        currentTile.current = true;
        return currentTile;
    }

    public void SetCurrentTile(Tile t)
    {
        if (currentTile != null) currentTile.familiarOccupant = null;
        currentTile = t;
        this.gameObject.transform.position = currentTile.gameObject.transform.position;
        currentTile.familiarOccupant = this;
    }

    public void FindSelectableTiles(TileState s, int range)
    {
        GetCurrentTile();

        Queue<Tile> process = new Queue<Tile>();
        
        process.Enqueue(currentTile);
        currentTile.visited = true;
        // currentNode.parent = null;

        while (process.Count > 0)
        {
            Tile t = process.Dequeue();

            selectableTiles.Add(t);
            t.selectable = true;

            t.SetState(s);

            if (t.distance < range)
            {
                //Tile[] _additions = new Node[4];
                List<Tile> _additions = t.GetNeighbors();

                foreach (Tile _t in _additions)
                {
                    if (_t != null)
                    {
                        if (!(_t.visited || _t.familiarOccupant != null))
                        {
                            _t.parent = t;
                            _t.visited = true;
                            _t.distance = 1 + t.distance;
                            process.Enqueue(_t);
                        }
                    }
                }
            }
        }
    }
}
