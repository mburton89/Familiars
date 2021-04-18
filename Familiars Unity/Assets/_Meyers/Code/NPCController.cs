using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog battleCompleteDialog;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    bool interactable;
    CharacterController player;

    // Trainer stuff

    FamiliarParty familiarParty;
    bool trainer;
    [HideInInspector] public bool completeBattle; 

    void Awake()
    {
        state = NPCState.Idle;

        familiarParty = gameObject.GetComponent<FamiliarParty>();
        if (familiarParty != null)
        {
            trainer = true;
            Debug.Log("There be trainers here!");
        }
    }

    private void Update()
    {
        if (interactable && Input.GetKeyDown(KeyCode.Z) && player.state == PlayerState.Normal)
        {
            Interact(player.gameObject);
        }
    }

    public void Interact(GameObject player)
    {
        this.player.state = PlayerState.Interacting;
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            //GameController.Instance.LookTowards(initiator.position);

            if (trainer)
            {
                if (!completeBattle)
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
            else
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(dialog, player, () =>
                {
                    idleTimer = 0f;
                    state = NPCState.Idle;
                }));
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject.GetComponent<CharacterController>();
            interactable = true;
            if (trainer && !completeBattle)
            {
                Interact(player.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            player = null;
            interactable = false;
        }
    }
}

public enum NPCState {Dialog, Idle }

