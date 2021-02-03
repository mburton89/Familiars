using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    [SerializeField] private Tile origin;
    public Tile[] field;

    public bool playerTeam;

    private void Start()
    {
        Tile _t;

        // Create a Tile list to gradually add new tiles to list
        for (int i = 0; i < field.Length; i++)  
        {
            _t = field[i];

            _t.x = i / 3;
            _t.y = i % 3;
            //Gather Up Neighbors
            if (!(i == 0 || i == 3 || i == 6))
            {
                _t.AddNeighbors(field[i - 1]);
            }
            // Gather Forward (Toward Center) Neighbors
            if (i > 2)
            {
                _t.AddNeighbors(field[i - 3]);
            }
            // Gather Down Neighbors
            if (!(i == 2 || i == 5 || i == 8))
            {
                _t.AddNeighbors(field[i + 1]);
            }
            // Gather Back (Away from Center) Neighbors
            if (i < 6)
            {
                _t.AddNeighbors(field[i + 3]);
            }
        }
    }

    public void SetFieldPattern(PatternBase pattern, TileState st)
    {
        bool[] _patternSets = pattern.Active;
        for (int _p = 0; _p < _patternSets.Length; _p++)
        {
            if (_patternSets[_p])
            {
                field[_p].SetState(st);
            }
            else
            {
                field[_p].SetState(TileState.Normal);
            }
            
        }
    }


    public void SetFieldTargetingRecticle(PatternBase pattern, TileState st)
    {
        SetFieldTargetingRecticle(pattern, st, 4);
    }

    public void SetFieldTargetingRecticle(PatternBase pattern, TileState st, int centerPos)
    {
        bool[] _patternSets = pattern.Active;
        for (int _p = 0; _p < _patternSets.Length; _p++)
        {
            // Check to see if the shifted array would go out of bounds
            if (!(_p - (4 - centerPos) > _patternSets.Length || _p - (4 - centerPos) < 0))
            {
                if (_patternSets[_p])
                {
                    field[_p - (4 - centerPos)].SetState(st);
                }
            }
        }
    }

    public Tile GetTile(int t)
    {
        return field[t];
    }

    public void ResetTiles()
    {
        for (int _i = 0; _i < field.Length; _i++)
        {
            field[_i].SetState(TileState.Normal);
        }
    }
}
