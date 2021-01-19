using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattleState { Start, PlayerSelection, PlayerAction, PlayerAttack, EnemyAttack, Busy }

public class CombatManager : MonoBehaviour
{
    //[HideInInspector] public BattleState battleState = BattleState.Initialize;
    //[HideInInspector] public CombatAvatar selectedFamiliar;

    //private BattleMenuState battleMenuState = BattleMenuState.None;

    [SerializeField] public FieldManager playerTeam;
    [SerializeField] public FieldManager enemyTeam;
    [SerializeField] private GameObject[] menus;

    [SerializeField] private GameObject combatAvatarPrefab;

    public int actions = 3;
    [SerializeField] private GameObject[] actionsDisplay;


    [SerializeField] List<CombatAvatar> playerUnits;
    [SerializeField] List<CombatAvatar> enemyUnits;
    [SerializeField] BattleHUD playerHUD;

    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] BattleSelector battleSelector;

    BattleState state;
    int currentPosition;
    int currentAction;
    int currentAttack;

    CombatAvatar selectedFamiliar;

    private void Start()
    {
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        for (int _i = 0; _i < playerUnits.Count; _i++)
        {
            playerUnits[_i].Setup();
        }
        for (int _j = 0; _j < enemyUnits.Count; _j++)
        {
            enemyUnits[_j].Setup();
        }

        playerHUD.SetData(playerUnits[0].Familiar);

        yield return StartCoroutine(dialogBox.TypeDialog($"A wild thing has appeared"));

        yield return new WaitForSeconds(1f);

        PlayerSelection();
    }
    
    void PlayerSelection()
    {
        state = BattleState.PlayerSelection;
        dialogBox.EnableActionSelector(false);
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.EnableDialogText(false);
        dialogBox.EnableActionSelector(true);
    }

    void PlayerAttack()
    {
        state = BattleState.PlayerAttack;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableAttackSelector(true);
        playerHUD.gameObject.SetActive(false);
    }

    private void Update()
    {
        switch (state)
        {
            case BattleState.PlayerSelection:
                HandleSelectorSelection();
                break;
            case BattleState.PlayerAction:
                HandleActionSelection();
                break;
            case BattleState.PlayerAttack:
                HandleAttackSelection();
                break;
        }
    }

    void HandleSelectorSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (!(currentPosition == 2 || currentPosition == 5 || currentPosition == 8))
                currentPosition++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (!(currentPosition == 0 || currentPosition == 3 || currentPosition == 6))
                currentPosition--;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPosition > 2)
                currentPosition -= 3;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentPosition < 6)
                currentPosition += 3;
        }

        battleSelector.SetPosition(currentPosition);

        if (playerTeam.field[currentPosition].familiarOccupant != null)
        {
            playerHUD.gameObject.SetActive(true);
            playerHUD.SetData(playerTeam.field[currentPosition].familiarOccupant.Familiar);

            if (Input.GetKeyDown(KeyCode.Z))
            {
                selectedFamiliar = playerTeam.field[currentPosition].familiarOccupant;
                PlayerAction();
            }
        }
        else
        {
            playerHUD.gameObject.SetActive(false);
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < actionsDisplay.Length)
                ++currentAction;

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                dialogBox.SetAttackNames(selectedFamiliar.Familiar.Attacks);
                PlayerAttack();
            }
            else if (currentAction == 0)
            {
                // Move
            }
        }
    }

    void HandleAttackSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAttack < playerUnits[0].Familiar.Attacks.Count - 1)
            {
                currentAttack++;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAttack > 0)
            {
                currentAttack--;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAttack < playerUnits[0].Familiar.Attacks.Count - 2)
            {
                currentAttack += 2;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAttack > 1)
            {
                currentAttack -= 2;
            }

        }

        dialogBox.UpdateAttackSelection(currentAttack, selectedFamiliar.Familiar.Attacks[currentAttack]);
    }

}