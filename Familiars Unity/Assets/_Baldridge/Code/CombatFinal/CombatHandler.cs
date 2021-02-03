using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CombatState { Start, PlayerSelection, PlayerAction, PlayerAttack, PlayerMove, PlayerTargeting, EnemyAttack, Busy }

public class CombatHandler : MonoBehaviour
{
    [HideInInspector] public CombatState combatState = CombatState.Start;
    [HideInInspector] public CombatUnit selectedFamiliar;

    [SerializeField] Field playerField;
    [SerializeField] Field enemyField;

    [SerializeField] GameObject combatUnitPrefab;

    #region All of the HUD elements that get turned on/off
    //Health Bars
    [SerializeField] GameObject playerHUDs;
    [SerializeField] GameObject enemyHUDs;

    //Navigator
    [SerializeField] Navigator navigator;

    // Dialog Menus
    [SerializeField] BattleDialogBox dialogMenu;

    #endregion

    public CombatUnit[] playerTeam;
    public CombatUnit[] enemyTeam;

    // Units from both sides
    public GameObject[] playerActive;
    public GameObject[] playerBench;
    public GameObject[] enemyActive;
    public GameObject[] enemyBench;

    int currentPosition;
    int currentTargetPosition;
    int currentAction;
    int currentAttack;

    void Start()
    {
        combatState = CombatState.Start;

        // Create the UI
        playerHUDs.SetActive(true);
        enemyHUDs.SetActive(true);
        

        // Create the currently used Familiars for both teams
        CombatUnit _curUnit;
        float _count = (float)playerTeam.Length;
        Tile _t;

        for (int i = 0; i < Mathf.Min(3f, _count); i++)
        {
            _curUnit = playerTeam[i];
            _t = playerField.GetTile(_curUnit.x, _curUnit.y);
            _t.familiarOccupant = _curUnit;
            _curUnit.SetCurrentTile(_t);
            _curUnit.gameObject.transform.position = _t.gameObject.transform.position;
            _curUnit.Setup();
            //GameObject _fam = Instantiate(combatUnitPrefab, _tileLocation.position, Quaternion.identity);
            //_fam.GetComponent<CombatUnit>().
        }

        _count = (float)enemyTeam.Length;
        for (int i = 0; i < Mathf.Min(3f, _count); i++)
        {
            _curUnit = enemyTeam[i];
            _t = enemyField.GetTile(_curUnit.x, _curUnit.y);
            _t.familiarOccupant = _curUnit;
            _curUnit.SetCurrentTile(_t);
            _curUnit.gameObject.transform.position = _t.gameObject.transform.position;
            _curUnit.Setup();
        }

        playerField.GatherNeighbors();

        PlayerSelection();
    }

    #region State Shifters
    void PlayerSelection()
    {
        combatState = CombatState.PlayerSelection;
    }

    void PlayerAction()
    {
        combatState = CombatState.PlayerAction;

        dialogMenu.EnableDialogText(false);
        dialogMenu.EnableActionSelector(true);
    }

    void PlayerAttack()
    {
        combatState = CombatState.PlayerAttack;

        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableAttackSelector(true);
    }

    void PlayerMove()
    {
        combatState = CombatState.PlayerMove;
        
        selectedFamiliar.SetCurrentTile(playerField.GetTile(currentPosition));
        selectedFamiliar.FindSelectableTiles(TileState.Move, selectedFamiliar.Familiar.Base.Movement);
    }

    void PlayerTargeting()
    {

    }
    #endregion

    void Update()
    {
        switch (combatState)
        {
            case CombatState.PlayerSelection:
                HandlePlayerSelection();
                break;
            case CombatState.PlayerAction:
                HandlePlayerAction();
                break;
            case CombatState.PlayerAttack:
                HandlePlayerAttack();
                break;
            case CombatState.PlayerMove:
                HandlePlayerMove();
                break;
            case CombatState.PlayerTargeting:
                break;

        }
    }

    void HandlePlayerSelection()
    {
        #region Navigation
        // Move left (add switching to supp. board later)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentPosition < 6)
            {
                currentPosition += 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPosition > 2)
            {
                currentPosition -= 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentPosition % 3 != 2)
            {
                currentPosition++;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentPosition % 3 != 0)
            {
                currentPosition--;
            }
        }

        navigator.SetLocation(playerField.GetTile(currentPosition));
        #endregion


        if (Input.GetKeyDown(KeyCode.Z))
        {
            CombatUnit _combatUnit = playerField.GetTile(currentPosition).familiarOccupant;
            if (_combatUnit != null)
            {
                selectedFamiliar = _combatUnit;
                PlayerAction();
            }
        }

    }

    void HandlePlayerAction()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction < 4)
                ++currentAction;

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogMenu.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                // Fight
                dialogMenu.SetAttackNames(selectedFamiliar.Familiar.Attacks);
                PlayerAttack();
            }
            else if (currentAction == 1)
            {
                // Move
                PlayerMove();
            }
        }
    }

    void HandlePlayerAttack()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAttack < 4)
                currentAttack++;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAttack > 0)
                currentAttack--;
        }

        dialogMenu.UpdateAttackSelection(currentAttack, selectedFamiliar.Familiar.Attacks[currentAttack]);

        //playerTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Sources, TileState.Source);
        //enemyTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Targets, TileState.Target);
        //enemyTeam.SetFieldTargetingRecticle(selectedFamiliar.Familiar.Attacks[currentAttack].Base.TargetingReticle, TileState.TargetReticle);

        if (Input.GetKeyDown(KeyCode.Z))
        {

        }
    }

    void HandlePlayerMove()
    {
        // Move left (add switching to supp. board later)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentPosition < 6)
            {
                currentPosition += 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPosition > 2)
            {
                currentPosition -= 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentPosition % 3 != 2)
            {
                currentPosition++;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentPosition % 3 != 0)
            {
                currentPosition--;
            }
        }
        
        navigator.SetLocation(playerField.GetTile(currentPosition));
    }
}
