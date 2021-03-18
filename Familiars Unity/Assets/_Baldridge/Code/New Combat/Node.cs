using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum NodeState { Normal, Move, Target }

public class Node : MonoBehaviour
{
    private BattleManager battleManager;
    private SpriteRenderer spriteRenderer;

    [HideInInspector] public Vector2 position; 
    [HideInInspector] public NodeState state = NodeState.Normal;

    [HideInInspector] public bool current = false;
    [HideInInspector] public bool target = false;
    [HideInInspector] public bool selectable = false;
    [HideInInspector] public bool walkable = true;

    // Breadth First Search Variales
    [HideInInspector] public bool visited = false;
    [HideInInspector] public Node parent = null;
    [HideInInspector] public int distance = 0;


    public Unit occupant;

    private Node[] neighbors = new Node[4]; // [N, E, S, W]

    // Gets the Battle Manager
    private void Awake()
    {
        battleManager = GameObject.Find("BattleManager").GetComponent<BattleManager>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void SetState(NodeState _state)
    {
        state = _state;
        switch (state)
        {
            case NodeState.Normal:
                spriteRenderer.color = Color.white;
                selectable = false;
                break;
            case NodeState.Move:
                spriteRenderer.color = Color.blue;
                selectable = true;
                break;
            case NodeState.Target:
                spriteRenderer.color = Color.red;
                selectable = true;
                break;
        }

    }

    public void Reset()
    {
        current = false;
        target = false;
        selectable = false;
        walkable = true;

        visited = false;
        parent = null;
        distance = 0;
        SetState(NodeState.Normal);
    }

    public void GatherNeighbors()
    {
        Reset();

        // North Tiles
        if (position.y < battleManager.boardHeight - 1)
        {
            neighbors[0] = battleManager.board[(int)position.x, (int)position.y + 1];
        }
        else { neighbors[0] = null; }

        // East Tiles
        if (position.x < battleManager.boardWidth - 1)
        {
            neighbors[1] = battleManager.board[(int)position.x + 1, (int)position.y];
        }
        else { neighbors[1] = null; }

        // South Tiles
        if (position.y > 0)
        {
            neighbors[2] = battleManager.board[(int)position.x, (int)position.y - 1];
        }
        else { neighbors[2] = null; }

        // West Tiles
        if (position.x > 0)
        {
            neighbors[3] = battleManager.board[(int)position.x - 1, (int)position.y];
        }
        else { neighbors[3] = null; }
    }

    public Node[] GetNeighbors()
    {
        return neighbors;
    }

    public Node North()
    {
        return neighbors[0];
    }

    public Node East()
    {
        return neighbors[1];
    }

    public Node South()
    {
        return neighbors[2];
    }

    public Node West()
    {
        return neighbors[3];
    }
}
