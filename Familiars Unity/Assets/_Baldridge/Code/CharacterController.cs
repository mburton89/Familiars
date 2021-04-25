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
    public Camera cm;

    public event Action OnEncountered;

    private Rigidbody2D rb2D;
    private Vector3 moveDir;
    private bool isDashButtonDown;

    [SerializeField] float noEncounterPeriod = 2f;
    bool inGrass = false;
    bool noEncounter;

    [HideInInspector] public PlayerState state = PlayerState.Normal;

    [SerializeField] Animator animator;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        Debug.Log(this);
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if(Input.GetKey(KeyCode.UpArrow))
        {
            moveY = +1f;
        }

        if(Input.GetKey(KeyCode.DownArrow))
        {
            moveY = -1f;
        }

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1f;
        }

        if(Input.GetKey(KeyCode.RightArrow))
        {
            moveX = +1f;
        }

        moveDir = new Vector3(moveX, moveY).normalized;

        animator.SetFloat("Horizontal", moveX);
        animator.SetFloat("Vertical", moveY);
        animator.SetFloat("Speed", moveDir.sqrMagnitude);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            isDashButtonDown = true;
        }
    }

    private void FixedUpdate()
    {
        if (state == PlayerState.Normal)
        {
            rb2D.velocity = moveDir * MOVE_SPEED;

            if (isDashButtonDown)
            {
                float dashAmount = 4f;
                Vector3 dashPosition = transform.position + moveDir * dashAmount;

                RaycastHit2D raycastHit2d = Physics2D.Raycast(transform.position, moveDir, dashAmount, dashLayerMask);
                if (raycastHit2d.collider != null)
                {
                    dashPosition = raycastHit2d.point;
                }

                rb2D.MovePosition(dashPosition);
                isDashButtonDown = false;
            }

            if (rb2D.velocity.magnitude > 0)
            {
                CheckForEncounters();
            }
        }
        else if (state == PlayerState.Interacting)
        {
            rb2D.velocity = new Vector2(0, 0);
        }
    }

    private void CheckForEncounters()
    {
        if (!noEncounter)
        {
            if (inGrass)
            {
                int _r = UnityEngine.Random.Range(1, 101);
                Debug.Log("Grass, " + _r);
                if (_r <= 5)
                {
                    OnEncountered?.Invoke();
                }
            }
        }
    }

    public void SetPosition(Vector3 position)
    {
        this.gameObject.transform.position = position;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Grass")
        {
            inGrass = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Grass")
        {
            inGrass = false;
        }
    }
}
