using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;
    [SerializeField] Text dialogText;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject attackSelector;
    [SerializeField] GameObject attackDetails;
    [SerializeField] GameObject attackScreen; 

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> attackTexts;

    [SerializeField] Text usesText;
    [SerializeField] Text styleText;
    [SerializeField] Text typeText;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
    }

    public void EnableDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableAttackSelector(bool enabled)
    {
        attackScreen.SetActive(enabled);
        attackSelector.SetActive(enabled);
        attackDetails.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int _i = 0; _i < actionTexts.Count; _i++)
        {
            if (_i == selectedAction)
                actionTexts[_i].color = highlightedColor;
            else
                actionTexts[_i].color = Color.black;
        }
    }

    public void UpdateAttackSelection(int selectedAttack, Attack attack)
    {
        for (int _i = 0; _i < attackTexts.Count; _i++)
        {
            if (_i == selectedAttack)
                attackTexts[_i].color = highlightedColor;
            else
                attackTexts[_i].color = Color.black;
        }

        usesText.text = $"Uses: {attack.Uses}/{attack.Base.Uses}";
        styleText.text = attack.Base.AttackStyle.ToString();
        typeText.text = attack.Base.Type.ToString();
    }

    public void SetAttackNames(List<Attack> attacks)
    {
        for (int _i = 0; _i < attackTexts.Count; _i++)
        {
            if (_i < attacks.Count)
            {
                attackTexts[_i].text = attacks[_i].Base.Name;
            }
            else
            {
                attackTexts[_i].text = "-";
            }
        }
    }
}
