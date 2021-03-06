﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField] bool playerField;
    [SerializeField] bool mainField;
    [SerializeField] Field connectedField;
    [SerializeField] Tile[] board;

    [SerializeField] PatternBase allTiles;

    /*      Note - Player side board is as follows vv   |||   Enemy side board is as follows
     *       * ---------*----------*-------- *                    * ---------*----------*--------- *
     *      ||          |          |         ||                  ||          |          |          ||
     *      ||  (2, 0)  |  (1, 0)  |  (0, 0) ||                  ||  (0, 0)  |  (1, 0)  |  (2, 0)  ||      
     *      ||          |          |    0    ||                  ||          |          |          ||
     *       *----------|----------|---------*                    * ---------|----------|----------*
     *      ||          |          |         ||                  ||          |          |          ||
     *      ||  (2, 1)  |  (1, 1)  |  (0, 1) ||                  ||  (0, 1)  |  (1, 1)  |  (2, 1)  ||      
     *      ||          |          |    1    ||                  ||          |          |          ||
     *       *----------|----------|---------*                    * ---------|----------|----------*
     *      ||          |          |         ||                  ||          |          |          ||
     *      ||  (2, 2)  |  (1, 2)  |  (0, 2) ||                  ||  (0, 2)  |  (1, 2)  |  (2, 2)  ||      
     *      ||          |          |         ||                  ||          |          |          ||
     *       * ---------*----------*-------- *                    * ---------*----------*--------- *
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
        SetFieldPattern(pattern, tileState, allTiles, TileState.Normal);
    }

    public void SetFieldPattern(PatternBase foregroundPattern, TileState tileState, PatternBase backgroundPattern, TileState secondaryState)
    {
        bool[] _foregroundPatternSets = foregroundPattern.Active;
        bool[] _backgroundPatternSets = backgroundPattern.Active;

        for (int _p = 0; _p < _backgroundPatternSets.Length; _p++)
        {
            if (_backgroundPatternSets[_p])
            {
                GetTile(_p).SetState(secondaryState);
            }
            else
            {
                GetTile(_p).SetState(TileState.Normal);
            }
        }

        for (int _p = 0; _p < _foregroundPatternSets.Length; _p++)
        {
            if (_foregroundPatternSets[_p])
            {
                GetTile(_p).SetState(tileState);
            }
        }
    }

    public void SetFieldTargetingReticle(PatternBase pattern, TileState st)
    {
        SetFieldTargetingReticle(pattern, st, 4);
    }

    public void SetFieldTargetingReticle(PatternBase pattern, TileState st, int centerPos)
    {
        bool[] _patternSets = pattern.Active;
        for (int _p = 0; _p < _patternSets.Length; _p++)
        {
            // Check to see if the shifted array would go out of bounds
            if (!(_p - (4 - centerPos) > _patternSets.Length || _p - (4 - centerPos) < 0))
            {
                if (_patternSets[_p])
                {
                    GetTile(_p - (4 - centerPos)).SetState(st);
                }
            }
        }
    }

    public void ClearTiles()
    {
        for (int _i = 0; _i < board.Length; _i++)
        {
            GetTile(_i).Reset();
        }
    }

    public List<CombatUnit> AICheckAreaAttack(int position, PatternBase tiles)
    {
        List<CombatUnit> _targets = new List<CombatUnit>();

        for (int _p = 0; _p < 9; _p++)
        {
            if (!(_p - (4 - position) > 9 || _p - (4 - position) < 0))
            {
                if (tiles.Active[_p] && GetTile(_p).familiarOccupant != null)
                {
                    _targets.Add(GetTile(_p).familiarOccupant);
                }
            }
        }

        return _targets;
    }
}
