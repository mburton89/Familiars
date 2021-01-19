using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HpBar hpBar;

    public void SetData(Familiar familiar)
    {
        nameText.text = familiar.Base.Name;
        levelText.text = "Lvl " + familiar.Level;
        hpBar.SetHP((float) familiar.HP / familiar.MaxHp);
    }
}
