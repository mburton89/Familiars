using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    [SerializeField] Dialog dialog;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    void Awake()
    {
        state = NPCState.Idle;
    }

    public void Interact(GameObject player)
    {
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
            other.gameObject.GetComponent<CharacterController>().state = PlayerState.Interacting;
            Interact(other.gameObject);
        }
    }

}

public enum NPCState {Dialog, Idle }

