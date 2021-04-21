using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FamiliarPartyManager : MonoBehaviour
{
    [SerializeField] Field playerField;

    [SerializeField] List<PartyPanel> partyPanels;

    private void Start()
    {
        for (int i = 0; i < partyPanels.Count; i++)
        {
            partyPanels[i].gameObject.SetActive(false);
        }
    }

    public void OpenPanel()
    {
        this.gameObject.SetActive(true);
        DisplayPanels();
    }

    public void ClosePanel()
    {
        this.gameObject.SetActive(false);
    }

    public void SetPanel(int position, PanelState state)
    {
        for (int i = 0; i < partyPanels.Count; i++)
        {
            partyPanels[i].SetState(PanelState.Normal);
        }
        partyPanels[position].SetState(state);
    }

    public Tile GetTile(int pos)
    {
        return playerField.GetTile(pos);
    }

    private void DisplayPanels()
    {
        for (int i = 0; i < PlayerParty.Instance.familiars.Count; i++)
        {
            partyPanels[i].gameObject.SetActive(true);
            partyPanels[i].UpdateDisplay(PlayerParty.Instance.familiars[i]);
        }
    }
}
