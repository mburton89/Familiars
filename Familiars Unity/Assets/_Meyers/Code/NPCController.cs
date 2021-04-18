using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] Dialog dialog;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    bool interactable;
    CharacterController player;

    FamiliarParty familiarParty;
    bool trainer;

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

            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, player, () =>
            {
                idleTimer = 0f;
                state = NPCState.Idle;
            }));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            player = other.gameObject.GetComponent<CharacterController>();
            interactable = true;
            if (trainer)
            {
                Interact(player.gameObject);
                DialogManager.Instance.StartBattle(familiarParty);
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

