﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PanelState { Normal, Hover, Selected };
public class PartyPanel : MonoBehaviour
{
    public HpBar hpBar;

    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Image background;

    [SerializeField] Color regularColor;
    [SerializeField] Color hoverColor;
    [SerializeField] Color selectedColor;
    [SerializeField] Color standardBackgroundColor;
    [SerializeField] Color faintedBackgroundColor;

    bool locked;

    public void UpdateDisplay(Familiar familiar)
    {
        nameText.text = familiar.Base.Name;
        levelText.text = "Lvl: " + familiar.Level;

        hpBar.SetHP((float) familiar.HP / familiar.MaxHp);

        if (familiar.HP == 0)
        {
            background.color = faintedBackgroundColor;
        }
        else
        {
            background.color = standardBackgroundColor;
        }
    }

    public void SetState(PanelState state)
    {
        if (!locked)
        {
            switch (state)
            {
                case PanelState.Normal:
                    this.gameObject.GetComponent<Image>().color = regularColor;
                    break;
                case PanelState.Hover:
                    this.gameObject.GetComponent<Image>().color = hoverColor;
                    break;
                case PanelState.Selected:
                    this.gameObject.GetComponent<Image>().color = selectedColor;
                    break;
            }
        }
    }

    public void Lock(bool l)
    {
        locked = l;
    }
}
