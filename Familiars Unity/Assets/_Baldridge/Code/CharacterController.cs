using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState { Normal, Interacting }
public class CharacterController : MonoBehaviour
{
    private const float MOVE_SPEED = 6f;
    public LayerMask barrierLayer;
    public LayerMask grassLayer;

    [SerializeField] private LayerMask dashLayerMask;

    public event Action OnEncountered;

    private Rigidbody2D rigidbody2D;
    private Vector3 moveDir;
    private bool isDashButtonDown;

    [SerializeField] float noEncounterPeriod = 2f;
    bool noEncounter;

    [HideInInspector] public PlayerState state = PlayerState.Normal;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        Debug.Log(this);
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if(Input.GetKey(KeyCode.W))
        {
            moveY = +1f;
        }

        if(Input.GetKey(KeyCode.S))
        {
            moveY = -1f;
        }

        if(Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
        }

        if(Input.GetKey(KeyCode.D))
        {
            moveX = +1f;
        }

        moveDir = new Vector3(moveX, moveY).normalized;

        

        if(Input.GetKeyDown(KeyCode.Space))
        {
            isDashButtonDown = true;
        }
    }

    private void FixedUpdate()
    {
        if (state == PlayerState.Normal)
        {
            rigidbody2D.velocity = moveDir * MOVE_SPEED;

            if (isDashButtonDown)
            {
                float dashAmount = 4f;
                Vector3 dashPosition = transform.position + moveDir * dashAmount;

                RaycastHit2D raycastHit2d = Physics2D.Raycast(transform.position, moveDir, dashAmount, dashLayerMask);
                if (raycastHit2d.collider != null)
                {
                    dashPosition = raycastHit2d.point;
                }

                rigidbody2D.MovePosition(dashPosition);
                isDashButtonDown = false;
            }

            if (rigidbody2D.velocity.magnitude > 0)
            {
                CheckForEncounters();
            }
        }
        else if (state == PlayerState.Interacting)
        {
            rigidbody2D.velocity = new Vector2(0, 0);
        }
    }

    private void CheckForEncounters()
    {
        if (!noEncounter)
        {
            if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
            {
                if (UnityEngine.Random.Range(1, 101) <= 5)
                {
                    if (OnEncountered != null)
                    {
                        OnEncountered();
                    }
                }
            }
        }
    }

    public void SetEncounterCooldown()
    {
        noEncounter = true;

        StartCoroutine(EncounterCooldown());
    }

    IEnumerator EncounterCooldown()
    {
        yield return new WaitForSeconds(noEncounterPeriod);
        noEncounter = false;
    }
}
