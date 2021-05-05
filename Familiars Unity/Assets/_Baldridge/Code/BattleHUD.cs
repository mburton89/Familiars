using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HUDDisplay { Hidden, Visible, Active }
public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] GameObject statusBar;
    [SerializeField] HpBar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color dazColor;
    [SerializeField] Color imoColor;

    HUDDisplay currentDisplay = HUDDisplay.Hidden;

    Familiar _familiar;
    Dictionary<ConditionID, Color> statusColors;
    //
    // Placements for stuff;
    float visibleY;
    float hiddenY;
    float activeY;

    RectTransform rTransform;

    private void Start()
    {
        rTransform = this.gameObject.GetComponent<RectTransform>();
        visibleY = rTransform.localPosition.y;
        hiddenY = visibleY;
        activeY = visibleY;
        //Display(HUDDisplay.Hidden);
    }

    public void SetData(Familiar familiar)
    {
        _familiar = familiar;

        nameText.text = familiar.Base.Name;
        SetLevel();
        hpBar.SetHP((float) familiar.HP / familiar.MaxHp);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.daz, dazColor },
            {ConditionID.imo, imoColor }
        };

        SetStatusText();
        _familiar.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if (_familiar.Status?.Name == "Burn" || _familiar.Status?.Name == "Poison" || _familiar.Status?.Name == "Dazed")
        {
            statusBar.SetActive(true);
            statusText.text = _familiar.Status.Name.ToUpper();
            statusText.color = statusColors[_familiar.Status.Id];
        }
        else
        {
            statusBar.SetActive(false);
            statusText.text = "";
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _familiar.Level;
    }

    public IEnumerator UpdateHP()
    {
        if (_familiar.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float)_familiar.HP / _familiar.MaxHp);
            _familiar.HpChanged = false;
        }
    }

    public void Active(bool active)
    {
        this.gameObject.SetActive(active);
    }
}
