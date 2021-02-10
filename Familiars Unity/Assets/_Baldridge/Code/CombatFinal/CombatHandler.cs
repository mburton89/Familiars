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
    [SerializeField] BattleHUD[] playerHUDs;
    [SerializeField] BattleHUD[] enemyHUDs;

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

    CombatState nextState;

    int actions = 3;

    int currentPosition;
    int currentTargetPosition;
    int currentAction;
    int currentAttack;

    void Start()
    {
        combatState = CombatState.Start;

        // Create the UI
        //playerHUDs.SetActive(true);
        //enemyHUDs.SetActive(true);
        

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
            _curUnit.teamPosition = i;
            playerHUDs[i].SetData(_curUnit.Familiar);
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
            _curUnit.teamPosition = i;
            enemyHUDs[i].SetData(_curUnit.Familiar);
        }

        playerField.GatherNeighbors();

        PlayerSelection();
    }

    #region State Shifters
    void PlayerSelection()
    {
        combatState = CombatState.PlayerSelection;

        currentAction = 0;
        currentAttack = 0;
        selectedFamiliar = null;

        dialogMenu.EnableActionSelector(false);
    }

    void PlayerAction()
    {
        combatState = CombatState.PlayerAction;

        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableDialogText(false);
        dialogMenu.EnableAttackSelector(false);
        dialogMenu.EnableActionSelector(true);
        
    }

    void PlayerAttack()
    {
        combatState = CombatState.PlayerAttack;

        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableAttackSelector(true);

        Debug.Log("[CombatHandler] PlayerAttack()");
    }

    void PlayerMove()
    {
        combatState = CombatState.PlayerMove;
        
        selectedFamiliar.SetCurrentTile(playerField.GetTile(currentPosition));
        selectedFamiliar.FindSelectableTiles(TileState.Move, selectedFamiliar.Familiar.Base.Movement);
    }

    void PlayerTargeting()
    {
        combatState = CombatState.PlayerTargeting;

        currentTargetPosition = 4;
        playerField.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Sources, TileState.Source);
        enemyField.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Targets, TileState.Target);
        Debug.Log("[CombatHandler] PlayerTargetting()");
    }

    void StartPlayerAttack()
    {
        Debug.Log("[CombatHandler] StartPlayerAttack()");
        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableAttackSelector(false);
        dialogMenu.EnableDialogText(true);
        StartCoroutine(PerformPlayerAttack());


        Debug.Log("[CombatHandler] StartPlayerAttack() End");
    }

    void EnemyAttack()
    {
        combatState = CombatState.EnemyAttack;

        StartCoroutine(PerformEnemyAttack());
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
                HandlePlayerTargeting();
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
            if (currentAction < 3)
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

        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayerSelection();
        }
    }

    void HandlePlayerAttack()
    {
        Debug.Log("[CombatHandler] HandlePlayerAttack() navigation");
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAttack > 0)
                currentAttack--;
            else
                currentAttack = selectedFamiliar.Familiar.Attacks.Count - 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAttack < selectedFamiliar.Familiar.Attacks.Count - 1)
                currentAttack++;
            else
                currentAttack = 0;
        }

        dialogMenu.UpdateAttackSelection(currentAttack, selectedFamiliar.Familiar.Attacks[currentAttack]);

        Debug.Log("[CombatHandler] HandlePlayerAttack() field updater");
        playerField.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Sources, TileState.Source);
        enemyField.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Targets, TileState.Target);
        
        // Target Display
        switch(selectedFamiliar.Familiar.Attacks[currentAttack].Base.AttackStyle)
        {
            case AttackStyle.Target:
                enemyField.SetFieldTargetingRecticle(selectedFamiliar.Familiar.Attacks[currentAttack].Base.TargetingReticle, TileState.TargetReticle);
                break;
        }


        //playerTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Sources, TileState.Source);
        //enemyTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Targets, TileState.Target);
        //enemyTeam.SetFieldTargetingRecticle(selectedFamiliar.Familiar.Attacks[currentAttack].Base.TargetingReticle, TileState.TargetReticle);

        Debug.Log("[CombatHandler] HandlePlayerAttack() inputs");
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // If the attack can be used at the current position
            if ( selectedFamiliar.Familiar.Attacks[currentAttack].Base.Sources.Active[currentPosition])
            {
                PlayerTargeting();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayerAction();
        }
    }

    void HandlePlayerTargeting()
    {
        #region Navigation
        // Move left (add switching to supp. board later)
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentTargetPosition < 6)
            {
                currentTargetPosition += 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentTargetPosition > 2)
            {
                currentTargetPosition -= 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentTargetPosition % 3 != 2)
            {
                currentTargetPosition++;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentTargetPosition % 3 != 0)
            {
                currentTargetPosition--;
            }
        }

        navigator.SetLocation(enemyField.GetTile(currentTargetPosition));
        #endregion

        if (Input.GetKeyDown(KeyCode.Z))
        {
            // switch based on the attack style (later)
            if (enemyField.GetTile(currentTargetPosition).familiarOccupant != null && selectedFamiliar.Familiar.Attacks[currentAttack].Base.Targets.Active[currentTargetPosition])
            {
                nextState = CombatState.PlayerSelection;
                StartPlayerAttack();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayerAttack();
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

    IEnumerator PerformPlayerAttack()
    {
        combatState = CombatState.Busy;

        // Will need to adjust to suit multi-target attacks
        CombatUnit enemy = enemyField.GetTile(currentTargetPosition).familiarOccupant;

        var attack = selectedFamiliar.Familiar.Attacks[currentAttack];
        yield return dialogMenu.TypeDialog($"{selectedFamiliar.Familiar.Base.Name} used {attack.Base.Name}");
        yield return new WaitForSeconds(1f);


        // Probably replace w/ PerformAttack() when adding multi-targetting (probably)
        bool _isFainted = enemy.Familiar.TakeDamage(attack, selectedFamiliar.Familiar);
        yield return enemyHUDs[enemy.teamPosition].UpdateHP();

        if (_isFainted)
        {
            yield return dialogMenu.TypeDialog($"{enemy.Familiar.Base.Name} fainted");
        }

        actions--;
        if (actions > 0)
        {
            PlayerSelection();
        }
        else
        {
            EnemyAttack();
        }   
    }

    IEnumerator PerformEnemyAttack()
    {
        combatState = CombatState.EnemyAttack;

        for (int i = 0; i < 3; i++)
        {
            CombatUnit _enemyUnit = enemyTeam[Random.Range(0, 3)];

            // This is when we would have to verify a valid target, attack, and all that such AI decision making and stuff
            var attack = _enemyUnit.Familiar.GetRandomAttack();
            yield return dialogMenu.TypeDialog($"{_enemyUnit.Familiar.Base.Name} used {attack.Base.Name}");
            yield return new WaitForSeconds(1f);

            CombatUnit _target = playerTeam[Random.Range(0, 3)];

            bool isFainted = _target.Familiar.TakeDamage(attack, _enemyUnit.Familiar);
            yield return playerHUDs[_target.teamPosition].UpdateHP();

            if (isFainted)
            {
                yield return dialogMenu.TypeDialog($"{_target.Familiar.Base.Name} fainted");
            }
        }

        actions = 3;
        PlayerSelection();
    }

    void PerformAttack(Attack attack, CombatUnit user, CombatUnit target)
    {
        CombatUnit[] _targets = new CombatUnit[1];
        _targets[0] = target;
        //PerformAttack(attack, user, _targets);
    }

    void PerformAttack(Attack attack, CombatUnit user, CombatUnit [] targets)
    {

    }

}
