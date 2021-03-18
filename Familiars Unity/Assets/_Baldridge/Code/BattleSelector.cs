using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSelector : MonoBehaviour
{
    [SerializeField] FieldManager field;
    int position;

    public void SetPosition(int pos)
    {
        this.transform.position = field.field[pos].gameObject.transform.position;
        position = pos;
    }
}
