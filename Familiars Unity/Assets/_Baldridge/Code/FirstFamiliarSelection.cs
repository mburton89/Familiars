using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstFamiliarSelection : MonoBehaviour
{
    [SerializeField] List<FirstSelectionPanel> selectionPanels;

    int position = 1;
    CharacterController player;

    private void Awake()
    {
        //gameObject.SetActive(false);
        player = PlayerParty.Instance.gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        player.state = PlayerState.Interacting;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (position == 0) position = selectionPanels.Count - 1; else position--;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (position == selectionPanels.Count - 1) position = 0; else position++;
        }

        for (int i = 0; i < selectionPanels.Count; i++)
        {
            if (position == i)
            {
                selectionPanels[i].SetCurrent(true);
            }
            else
            {
                selectionPanels[i].SetCurrent(false);
            }
            
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            PlayerParty.Instance.familiars.Add(selectionPanels[position].Familiar);
            CurrentFamiliarsController.Instance.UpdatePlayerFamiliars(PlayerParty.Instance.familiars);
            player.state = PlayerState.Normal;

            PlayerPrefs.SetInt("TownToForest", 1);

            Destroy(this.gameObject);
        }
    }
}
