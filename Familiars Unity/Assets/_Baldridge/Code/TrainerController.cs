using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : NPCController
{
    [SerializeField] Dialog battleCompleteDialog;
    [SerializeField] int trainerID = 0;
    // Trainer stuff

    FamiliarParty familiarParty;
    bool autoInteract;

    protected new void Awake()
    {
        state = NPCState.Idle;
        familiarParty = gameObject.GetComponent<FamiliarParty>();
    }

    void Start()
    {
        if (!FlagManager.Instance.TrainerFlags[trainerID])
        {
            autoInteract = true;
        }
        else
        {
            autoInteract = false;
        }
    }

    public new void Interact(GameObject player)
    {
        this.player.state = PlayerState.Interacting;
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            //GameController.Instance.LookTowards(initiator.position);

            if (!FlagManager.Instance.TrainerFlags[trainerID])
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(dialog, player, () =>
                {
                    idleTimer = 0f;
                    state = NPCState.Idle;
                    GameController.Instance.StartTrainerBattle(familiarParty.familiars, this);
                }));
            }
            else
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(battleCompleteDialog, player, () =>
                {
                    idleTimer = 0f;
                    state = NPCState.Idle;
                }));
            }
        }
    }

    public int GetTrainerID()
    {
        return trainerID;
    }

    protected new void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject.GetComponent<CharacterController>();
            interactable = true;
            if (autoInteract)
            { 
                Interact(player.gameObject);
            }
        }
    }

}
