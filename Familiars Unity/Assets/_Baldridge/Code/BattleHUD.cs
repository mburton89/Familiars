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
    [SerializeField] HpBar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color dazColor;
    [SerializeField] Color imoColor;

    HUDDisplay currentDisplay = HUDDisplay.Hidden;

    Familiar _familiar;
    Dictionary<ConditionID, Color> statusColors;

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
        levelText.text = "Lvl " + familiar.Level;
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
        if (_familiar.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _familiar.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_familiar.Status.Id];
        }
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

    /*
    public void Display(HUDDisplay display)
    {
        StartCoroutine(ShowHUD(display));
    }

    IEnumerator ShowHUD(HUDDisplay display)
    {
        float currentPos = rTransform.localPosition.y;
        float newPos = hiddenY;
        
        switch (display)
        {
            case HUDDisplay.Hidden:
                newPos = hiddenY;
                break;
            case HUDDisplay.Visible:
                newPos = visibleY;
                break;
            case HUDDisplay.Active:
                newPos = activeY;
                break;
        }
        float changeAmt = currentPos - newPos;

        while (currentPos - newPos > Mathf.Epsilon)
        {
            currentPos -= changeAmt * Time.deltaTime;
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, currentPos);
            yield return null;
        }
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, newPos);
    }
    */
}
