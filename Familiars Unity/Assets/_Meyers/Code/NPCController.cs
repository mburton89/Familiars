using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] protected Dialog dialog;


    protected NPCState state;
    protected float idleTimer = 0f;
    protected int currentPattern = 0;

    protected bool interactable;
    protected CharacterController player;



    void Awake()
    {
        state = NPCState.Idle;
    }

    protected void Update()
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
        }
    }

    protected void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            player = null;
            interactable = false;
        }
    }
}

public enum NPCState {Dialog, Idle }

