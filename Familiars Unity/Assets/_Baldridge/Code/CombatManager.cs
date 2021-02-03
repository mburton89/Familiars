using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public enum BattleState { Start, PlayerSelection, PlayerAction, PlayerAttack, PlayerTargeting, EnemyAttack, Busy }

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
    [SerializeField] BattleSelector battleTargetSelector;

    BattleState state;
    int currentPosition = 4;
    int currentTargetPosition = 4;
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

        battleTargetSelector.gameObject.SetActive(false);
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
        battleSelector.gameObject.SetActive(false);
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

    void PlayerTargeting()
    {
        state = BattleState.PlayerTargeting;
        battleTargetSelector.gameObject.SetActive(true);
        dialogBox.EnableAttackSelector(false);
    }

    IEnumerator PerformPlayerAttacks()
    {
        state = BattleState.Busy;

        var attack = selectedFamiliar.Familiar.Attacks[currentAttack];
        yield return dialogBox.TypeDialog($"{selectedFamiliar.Familiar.Base.Name} used {attack.Base.Name}");
        yield return new WaitForSeconds(1f);
        bool isFainted = enemyUnits[0].Familiar.TakeDamage(attack, selectedFamiliar.Familiar);
        
        if (isFainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnits[0].Familiar.Base.Name} fainted");
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyAttack;

        var attack = enemyUnits[0].Familiar.GetRandomAttack();
        yield return dialogBox.TypeDialog($"{enemyUnits[0].Familiar.Base.Name} used {attack.Base.Name}");
        yield return new WaitForSeconds(1f);
        bool isFainted = playerUnits[0].Familiar.TakeDamage(attack, enemyUnits[0].Familiar);
        yield return playerHUD.UpdateHP();

        if (isFainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnits[0].Familiar.Base.Name} fainted");
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
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
            case BattleState.PlayerTargeting:
                HandleTargetSelection();
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

        playerTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Sources, TileState.Source);
        enemyTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Targets, TileState.Target);
        enemyTeam.SetFieldTargetingRecticle(selectedFamiliar.Familiar.Attacks[currentAttack].Base.TargetingReticle, TileState.TargetReticle);

        if (Input.GetKeyDown(KeyCode.Z))
        {

            // Once all actions are selected
            dialogBox.EnableAttackSelector(false);
            dialogBox.EnableDialogText(false);

            PlayerTargeting();

            // Select a target
            //StartCoroutine(PerformPlayerAttacks());
        }
    }

    void HandleTargetSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (!(currentTargetPosition == 2 || currentTargetPosition == 5 || currentTargetPosition == 8))
            {
                if (enemyTeam.field[currentTargetPosition + 1].currentState == TileState.Target)
                    currentTargetPosition++;
            }
                
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (!(currentTargetPosition == 0 || currentTargetPosition == 3 || currentTargetPosition == 6))
            {
                if (enemyTeam.field[currentTargetPosition - 1].currentState == TileState.Target)
                    currentTargetPosition--;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentTargetPosition > 2)
            {
                if (enemyTeam.field[currentTargetPosition - 3].currentState == TileState.Target)
                    currentTargetPosition -= 3;
            }
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentTargetPosition < 6)
            {
                if (enemyTeam.field[currentTargetPosition + 3].currentState == TileState.Target)
                    currentTargetPosition += 3;
            }
        }

        battleTargetSelector.SetPosition(currentTargetPosition);


        enemyTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Targets, TileState.Target);
        enemyTeam.SetFieldTargetingRecticle(selectedFamiliar.Familiar.Attacks[currentAttack].Base.TargetingReticle, TileState.TargetReticle, currentTargetPosition);
    }
}*/