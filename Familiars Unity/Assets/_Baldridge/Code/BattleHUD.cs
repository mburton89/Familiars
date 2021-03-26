using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HUDDisplay { Hidden, Visible, Active }
public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HpBar hpBar;

    HUDDisplay currentDisplay = HUDDisplay.Hidden;

    Familiar _familiar;

    // Placements for stuff;
    float visibleY;
    float hiddenY;
    float activeY;

    private void Start()
    {
        visibleY = this.gameObject.transform.position.y;
        hiddenY = visibleY - 20f;
        activeY = visibleY + 5f;
        Display(HUDDisplay.Hidden);
    }

    public void SetData(Familiar familiar)
    {
        _familiar = familiar;

        nameText.text = familiar.Base.Name;
        levelText.text = "Lvl " + familiar.Level;
        hpBar.SetHP((float) familiar.HP / familiar.MaxHp);
    }

    public IEnumerator UpdateHP()
    {
        yield return hpBar.SetHPSmooth((float) _familiar.HP / _familiar.MaxHp);
    }

    public void Display(HUDDisplay display)
    {
        StartCoroutine(ShowHUD(display));
    }

    IEnumerator ShowHUD(HUDDisplay display)
    {
        float currentPos = this.gameObject.transform.position.y;
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
}
