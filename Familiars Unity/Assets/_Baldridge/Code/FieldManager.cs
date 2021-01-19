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

    public Tile GetTile(int t)
    {
        return field[t];
    }

    public void ResetTiles()
    {
        for (int _i = 0; _i < field.Length; _i++)
        {
            field[_i].SetState(Tile.State.Normal);
        }
    }
}
