using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] bool playerField;
    [SerializeField] bool mainField;
    [SerializeField] Field connectedField;
    [SerializeField] Tile[] board;

    /*      Note - Player side board is as follows vv   |||   Enemy side board is as follows
     *       * ---------*----------*-------- *                    * ---------*----------*--------- *
     *      ||          |          |         ||                  ||          |          |          ||
     *      ||  (2, 0)  |  (1, 0)  |  (0, 0) ||                  ||  (0, 0)  |  (1, 0)  |  (2, 0)  ||      
     *      ||          |          |         ||                  ||          |          |          ||
     *       *----------|----------|---------*                    * ---------|----------|----------*
     *      ||          |          |         ||                  ||          |          |          ||
     *      ||  (2, 1)  |  (1, 1)  |  (0, 1) ||                  ||  (0, 1)  |  (1, 1)  |  (2, 1)  ||      
     *      ||          |          |         ||                  ||          |          |          ||
     *       *----------|----------|---------*                    * ---------|----------|----------*
     *      ||          |          |         ||                  ||          |          |          ||
     *      ||  (2, 2)  |  (1, 2)  |  (0, 2) ||                  ||  (0, 2)  |  (1, 2)  |  (2, 2)  ||      
     *      ||          |          |         ||                  ||          |          |          ||
     *       * ---------*----------*-------- *                    * ---------|----------|--------- *
     */

    public Tile GetTile(int x, int y)
    {
        return GetTile(x * 3 + y);
    }

    public Tile GetTile(int pos)
    {
        return board[pos];
    }
    
    public void GatherNeighbors()
    {
        Tile _t;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                _t = GetTile(i, j); 
                _t.Reset();

                // Grab the neighbor to the right
                if (i > 0)
                {
                    _t.AddNeighbors(GetTile(i - 1, j));
                }
                // Grab the neighbor to the left
                if (i < 2)
                {
                    _t.AddNeighbors(GetTile(i + 1, j));
                }

                // Grab the neighbor below
                if (j < 2)
                {
                    _t.AddNeighbors(GetTile(i, j + 1));
                }
                // Grab the neighbor above
                if (j > 0)
                {
                    _t.AddNeighbors(GetTile(i, j - 1));
                }
            }
        }
    }

    public void SetFieldPattern(PatternBase pattern, TileState tileState)
    {
        bool[] _patternSets = pattern.Active;
        for (int _p = 0; _p < _patternSets.Length; _p++)
        {
            if (_patternSets[_p])
            {
                GetTile(_p).SetState(tileState);
            }
            else
            {
                GetTile(_p).SetState(TileState.Normal);
            }

        }
    }
}
