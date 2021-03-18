using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] FamiliarBase _base;
    [SerializeField] int level;
    public bool isPlayerUnit;
    //[SerializeField] GameObject miniHealthPrefab;

    [SerializeField]public int x;
    [SerializeField]public int y;
    
    private BattleManager battleManager;
    private GameObject canvas;
    //private HpBar miniHPBar;

    List<Node> selectableNodes = new List<Node>();
    Stack<Node> path = new Stack<Node>();

    Node currentNode;
    public Node previousNode;
    public bool moving = false;
    public bool hasMoved;
    public bool turnComplete;

    public Familiar Familiar { get; set; }

    public void Setup()
    {
        Familiar = new Familiar(_base, level);
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();

        if (isPlayerUnit)
        {
            this.transform.localScale = new Vector3(1, 1, 1);

        }
        else
        {
            this.transform.localScale = new Vector3(-1, 1, 1);
        }

        this.GetComponent<SpriteRenderer>().sprite = Familiar.Base.FamiliarSprite;
        canvas = GameObject.Find("Canvas");
    }

    public Node GetCurrentNode()
    {
        currentNode = battleManager.board[x, y];
        currentNode.current = true;
        return currentNode;
    }

    public void SetCurrentNode(Node n)
    {
        currentNode = n;
    }

    public Node GetTargetNode(GameObject target)
    {
        Node node = null;

        return node;
    }

    public void FindSelectableNodes(NodeState state, int range)
    {
        FindSelectableNodes(state, range, 0, true);
    }

    public void FindSelectableNodes(NodeState state, int range, bool excludeOccupants)
    {
        FindSelectableNodes(state, range, 0, excludeOccupants);
    }

    // 0 - breath, 1 - north, 2 - east, 3 - south, 4 - west, 5 - cardinal directions
    public void FindSelectableNodes(NodeState state, int range, int directions, bool excludeOccupants)
    {
        GetCurrentNode();

        Queue<Node> process = new Queue<Node>();

        process.Enqueue(currentNode);
        currentNode.visited = true;
        // currentNode.parent = null;

        while (process.Count > 0)
        {
            Node n = process.Dequeue();

            selectableNodes.Add(n);
            n.selectable = true;

            n.SetState(state);

            if (n.distance < range)
            {
                Node[] _additions = new Node[4];

                #region Adding Nodes
                if (directions == 0)
                {
                    _additions = n.GetNeighbors();
                }
                if (directions == 1)
                {
                    _additions[0] = n.North();
                }
                if (directions == 2)
                {
                    _additions[1] = n.East();
                }
                if (directions == 3)
                {
                    _additions[2] = n.South();
                }
                if (directions == 4)
                {
                    _additions[3] = n.West();
                }
                #endregion
                foreach (Node _n in _additions)
                {
                    if (_n != null)
                    {
                        if (excludeOccupants)
                        {
                            if (!(_n.visited || _n.occupant != null))
                            {
                                _n.parent = n;
                                _n.visited = true;
                                _n.distance = 1 + n.distance;
                                process.Enqueue(_n);
                            }
                        }
                        else
                        {
                            if (!_n.visited)
                            {
                                _n.parent = n;
                                _n.visited = true;
                                _n.distance = 1 + n.distance;
                                process.Enqueue(_n);
                            }
                        }
                    }
                }
            }
        }
    }
    

    public void MoveToNode(Node node)
    {
        path.Clear();
        node.target = true;
        moving = true;

        Node next = node;
        while (next != null)
        {
            path.Push(next);
            next = next.parent;
        }
    }
}
