using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum CombatState { Start, PlayerSelection, PlayerAction, PlayerAttack, PlayerMove, PlayerTargeting, EnemyAttack, Busy }

public class CombatHandler : MonoBehaviour
{
    public static CombatHandler Instance;
    [HideInInspector] public CombatState combatState = CombatState.Start;
    [HideInInspector] public CombatUnit selectedFamiliar;
    
    [SerializeField] AudioSource audioSource;

    [SerializeField] Field playerField;
    [SerializeField] Field enemyField;

    [SerializeField] GameObject combatUnitPrefab;
    [SerializeField] Canvas canvas;

    #region All of the HUD elements that get turned on/off
    //Health Bars
    [SerializeField] BattleHUD[] playerHUDs;
    [SerializeField] BattleHUD[] enemyHUDs;

    //Navigator
    [SerializeField] Navigator navigator;

    // Dialog Menus
    [SerializeField] BattleDialogBox dialogMenu;

    #endregion

    public event Action<bool> OnBattleOver;

    public List<CombatUnit> playerTeam;
    public List<CombatUnit> enemyTeam;

    // Units from both sides
    public List<GameObject> playerActive;
    public List<GameObject> playerBench;
    public List<GameObject> enemyActive;
    public List<GameObject> enemyBench;

    CombatState nextState;

    int actions = 3;
    
    // Targetting Stuffs

    int currentPosition;
    int currentTargetPosition;
    int currentActionPosition;
    int currentAttackPosition;

    Attack currentAttack;

    int upperBoundX = 2;
    int upperBoundY = 2;
    int lowerBoundX = 0;
    int lowerBoundY = 0;

    List<CombatUnit> targets;

    FamiliarParty playerParty;
    List<Familiar> wildFamiliars;

    private void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(this.gameObject);
    }

    public void StoreParties(FamiliarParty playerParty, List<Familiar> wildFamiliars)
    {
        this.playerParty = playerParty;
        this.wildFamiliars = wildFamiliars;
        StartCoroutine(BeginBattle());
    }

    private void Start()
    {
        StartBattle();
    }

    IEnumerator BeginBattle()
    {
        yield return new WaitForSeconds(1f);
        StartBattle();
    }

    public void StartBattle()
    {
        combatState = CombatState.Start;
        
        targets = new List<CombatUnit>();
        // Create the UI
        //playerHUDs.SetActive(true);
        //enemyHUDs.SetActive(true);
        

        // Create the currently used Familiars for both teams
        CombatUnit _curUnit;
        GameObject _fam;
        float _count = (float)playerTeam.Count;
        Tile _t;
        int _x, _y;
        int iteration;

        for (int i = 0; i < Mathf.Min(3f, _count); i++)
        {
            _fam = Instantiate(combatUnitPrefab, canvas.gameObject.transform);
            _curUnit = _fam.GetComponent<CombatUnit>();
            _curUnit.Familiar = CurrentFamiliarsController.Instance.playerFamiliars[i];

            _x = UnityEngine.Random.Range(0, 3);
            _y = UnityEngine.Random.Range(0, 3);

            //_curUnit = playerTeam[i];
            _t = playerField.GetTile(_x, _y);
            iteration = 0;
            while (_t.familiarOccupant != null)
            {
                iteration++;
                if ((_x * 3 + _y) + iteration > 8)
                {
                    iteration -= 8;
                }
                _t = playerField.GetTile((_x * 3 + _y) + iteration);
            }
            _t.familiarOccupant = _curUnit;
            _curUnit.SetCurrentTile(_t);
            _curUnit.gameObject.transform.position = _t.gameObject.transform.position;
            _curUnit.Setup();
            _curUnit.teamPosition = i;
            playerHUDs[i].SetData(_curUnit.Familiar);
            //GameObject _fam = Instantiate(combatUnitPrefab, _tileLocation.position, Quaternion.identity);
            //_fam.GetComponent<CombatUnit>().
        }

        _count = (float)enemyTeam.Count;
        for (int i = 0; i < Mathf.Min(3f, _count); i++)
        {
            _fam = Instantiate(combatUnitPrefab, canvas.gameObject.transform);
            _curUnit = _fam.GetComponent<CombatUnit>();
            _curUnit.Familiar = CurrentFamiliarsController.Instance.enemyFamiliars[i];

            _x = UnityEngine.Random.Range(0, 3);
            _y = UnityEngine.Random.Range(0, 3);


            _t = enemyField.GetTile(_curUnit.x, _curUnit.y);
            iteration = 0;
            while (_t.familiarOccupant != null)
            {
                iteration++;
                if ((_x * 3 + _y) + iteration > 8)
                {
                    iteration -= 8;
                }
                _t = enemyField.GetTile((_x * 3 + _y) + iteration);
            }
            _t.familiarOccupant = _curUnit;
            _curUnit.SetCurrentTile(_t);
            _curUnit.gameObject.transform.position = _t.gameObject.transform.position;
            _curUnit.isPlayerUnit = false;
            _curUnit.Setup();
            _curUnit.teamPosition = i;
            enemyHUDs[i].SetData(_curUnit.Familiar);
        }

        playerField.GatherNeighbors();
        enemyField.GatherNeighbors();

        PlayerSelection();
    }

    #region State Shifters
    void PlayerSelection()
    {
        combatState = CombatState.PlayerSelection;

        currentActionPosition = 0;
        currentAttackPosition = 0;
        currentAttack = null;
        selectedFamiliar = null;

        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableActionSelector(false);
    }

    void ActionSelection()
    {
        combatState = CombatState.PlayerAction;

        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableDialogText(false);
        dialogMenu.EnableAttackSelector(false);
        dialogMenu.EnableActionSelector(true);
        
    }

    void AttackSelection()
    {
        combatState = CombatState.PlayerAttack;

        currentAttack = null;

        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableAttackSelector(true);

        targets.Clear();
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

        currentAttack = selectedFamiliar.Familiar.Attacks[currentAttackPosition];

        currentTargetPosition = currentAttack.Base.OriginPosition;
        playerField.SetFieldPattern(currentAttack.Base.Sources, TileState.Source);
        enemyField.SetFieldPattern(currentAttack.Base.Targets, TileState.Target);

        upperBoundX = currentAttack.Base.UpperX;
        upperBoundY = currentAttack.Base.UpperY;
        lowerBoundX = currentAttack.Base.LowerX;
        lowerBoundY = currentAttack.Base.LowerY;
    }

    void StartPlayerAttack()
    {
        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableAttackSelector(false);
        dialogMenu.EnableDialogText(true);
        StartCoroutine(PlayerAttack());
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
                Debug.Log("[CombatHandler.cs] HandlePlayerSelection, Selected Familiar: " + selectedFamiliar.Familiar.Base.Name);
                ActionSelection();
            }
        }

    }

    void HandlePlayerAction()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentActionPosition < 3)
                ++currentActionPosition;

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentActionPosition > 0)
                --currentActionPosition;
        }

        dialogMenu.UpdateActionSelection(currentActionPosition);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentActionPosition == 0)
            {
                // Fight
                dialogMenu.SetAttackNames(selectedFamiliar.Familiar.Attacks);
                AttackSelection();
            }
            else if (currentActionPosition == 1)
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
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAttackPosition > 0)
                currentAttackPosition--;
            else
                currentAttackPosition = selectedFamiliar.Familiar.Attacks.Count - 1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAttackPosition < selectedFamiliar.Familiar.Attacks.Count - 1)
                currentAttackPosition++;
            else
                currentAttackPosition = 0;
        }

        currentAttack = selectedFamiliar.Familiar.Attacks[currentAttackPosition];
        dialogMenu.UpdateAttackSelection(currentAttackPosition, currentAttack);
        
        playerField.SetFieldPattern(currentAttack.Base.Sources, TileState.Source);
        enemyField.SetFieldPattern(currentAttack.Base.Targets, TileState.Target);
        
        // Target Display
        switch(currentAttack.Base.AttackStyle)
        {
            case AttackStyle.Target:
                enemyField.SetFieldTargetingReticle(currentAttack.Base.TargetingReticle, TileState.TargetReticle);
                break;
            case AttackStyle.Projectile:
                break;
            case AttackStyle.Area:
                enemyField.SetFieldTargetingReticle(currentAttack.Base.TargetingReticle, TileState.TargetReticle);
                break;
            case AttackStyle.AreaStatic:
                enemyField.SetFieldTargetingReticle(currentAttack.Base.Targets, TileState.TargetReticle);
                break;
        }


        //playerTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Sources, TileState.Source);
        //enemyTeam.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttack].Base.Targets, TileState.Target);
        //enemyTeam.SetFieldTargetingRecticle(selectedFamiliar.Familiar.Attacks[currentAttack].Base.TargetingReticle, TileState.TargetReticle);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            // If the attack can be used at the current position
            if (currentAttack.Base.Sources.Active[currentPosition])
            {
                PlayerTargeting();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ActionSelection();
        }
    }

    void HandlePlayerTargeting()
    {
        #region Navigation
        // Move left (add switching to supp. board later)
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentTargetPosition < (upperBoundX * 3) && selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets.Active[currentTargetPosition + 3])
            {
                currentTargetPosition += 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentTargetPosition > (lowerBoundX + 2) && selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets.Active[currentTargetPosition - 3])
            {
                currentTargetPosition -= 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentTargetPosition % 3 < upperBoundY && selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets.Active[currentTargetPosition + 1])
            {
                currentTargetPosition++;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentTargetPosition % 3 > lowerBoundY && selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets.Active[currentTargetPosition - 1])
            {
                currentTargetPosition--;
            }
        }

        enemyField.SetFieldPattern(selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets, TileState.Target);
        enemyField.SetFieldTargetingReticle(selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.TargetingReticle, TileState.TargetReticle, currentTargetPosition);
        //navigator.SetLocation(enemyField.GetTile(currentTargetPosition));
        #endregion

        if (Input.GetKeyDown(KeyCode.Z))
        {
            bool valid = false;
            // Assign Targets
            switch (currentAttack.Base.AttackStyle)
            {
                case AttackStyle.Target:
                    if (enemyField.GetTile(currentTargetPosition).familiarOccupant != null && currentAttack.Base.Targets.Active[currentTargetPosition])
                    {
                        targets.Add(enemyField.GetTile(currentTargetPosition).familiarOccupant);
                        valid = true;
                    }
                    break;
                case AttackStyle.Projectile:
                    Tile _checkingTile = enemyField.GetTile(currentAttack.Base.ProjectileOrigin);
                    int _position = currentAttackPosition;
                    bool end;
                    while (_checkingTile != null)
                    {
                        end = false;
                        if (_checkingTile.familiarOccupant != null)
                        {
                            targets.Add(_checkingTile.familiarOccupant);
                            valid = true;
                            end = true;
                            break;
                        }

                        switch (currentAttack.Base.Direction)
                        {
                            case 0:
                                if (_position >= 6)
                                {
                                    end = true;
                                    break;
                                }
                                else _position += 3; 
                                break;

                        }
                        if (!end)
                        {
                            _checkingTile = enemyField.GetTile(_position);
                        }
                        else
                        {
                            _checkingTile = null;
                        }
                    }

                    break;
                case AttackStyle.Area:
                    for (int i = 0; i < 9; i++)
                    {
                        if (enemyField.GetTile(i).familiarOccupant != null && enemyField.GetTile(i).GetState() == TileState.TargetReticle)
                        {
                            valid = true;
                            targets.Add(enemyField.GetTile(i).familiarOccupant);
                        }
                    }
                    break;
                case AttackStyle.AreaStatic:
                    for (int i = 0; i < 9; i++)
                    {
                        if (enemyField.GetTile(i).familiarOccupant != null && currentAttack.Base.Targets.Active[i])
                        {
                            valid = true;
                            targets.Add(enemyField.GetTile(i).familiarOccupant);
                        }
                    }
                    break;
            }

            if (valid) StartPlayerAttack();

            
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            AttackSelection();
        }
    }

    void HandlePlayerMove()
    {
        #region Navigator
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
            Tile _temp = playerField.GetTile(currentPosition);
            if (_temp.GetState() == TileState.Move)
            {
                selectedFamiliar.SetCurrentTile(_temp);

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
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ActionSelection();
        }
    }

    IEnumerator PlayerAttack()
    {
        combatState = CombatState.Busy;

        var attack = currentAttack;
        yield return dialogMenu.TypeDialog($"{selectedFamiliar.Familiar.Base.Name} used {attack.Base.Name}");
        PlayNoise(selectedFamiliar.Familiar.Base.AttackSound);

        selectedFamiliar.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        // Probably replace w/ PerformAttack() when adding multi-targetting (probably)
        for (int i = 0; i < targets.Count; i++)
        {
            var damageDetails = targets[i].Familiar.TakeDamage(attack, selectedFamiliar.Familiar);
            targets[i].PlayHitAnimation();
            PlayNoise(targets[i].Familiar.Base.AttackSound);
            yield return enemyHUDs[targets[i].teamPosition].UpdateHP();
            yield return ShowDamageDetails(damageDetails);

            if (damageDetails.Fainted)
            {
                yield return dialogMenu.TypeDialog($"{targets[i].Familiar.Base.Name} fainted");
                targets[i].PlayFaintAnimation();

                yield return new WaitForSeconds(2f);
                OnBattleOver(true);
            }
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
            CombatUnit _enemyUnit = enemyTeam[UnityEngine.Random.Range(0, 3)];

            // This is when we would have to verify a valid target, attack, and all that such AI decision making and stuff
            var attack = _enemyUnit.Familiar.GetRandomAttack();
            yield return dialogMenu.TypeDialog($"{_enemyUnit.Familiar.Base.Name} used {attack.Base.Name}");
            PlayNoise(_enemyUnit.Familiar.Base.AttackSound);

            _enemyUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            CombatUnit _target = playerTeam[UnityEngine.Random.Range(0, 3)];

            _target.PlayHitAnimation();
            var damageDetails = _target.Familiar.TakeDamage(attack, _enemyUnit.Familiar);
            PlayNoise(_target.Familiar.Base.AttackSound);
            yield return playerHUDs[_target.teamPosition].UpdateHP();
            yield return ShowDamageDetails(damageDetails);

            if (damageDetails.Fainted)
            {
                yield return dialogMenu.TypeDialog($"{_target.Familiar.Base.Name} fainted");
                _target.PlayFaintAnimation();

                yield return new WaitForSeconds(2f);
                var nextFamiliar = playerParty.GetHealthyFamiliar();
                if (nextFamiliar != null)
                {
                    /* playerUnit.Setup(nextFamiliar);
                     * playerHud.SetData(nextFamiliar);
                     * 
                     * dialogBox.SetMoveNames(nextFamiliar.Moves);
                     * 
                     * yield return dialogBox.TypeDialog($"Go {nextFamiliar.Base.Name}!");
                     * 
                     * */
                }
                else
                {
                    OnBattleOver(false);
                }
            }
        }

        actions = 3;
        PlayerSelection();
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogMenu.TypeDialog("It's a critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogMenu.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogMenu.TypeDialog("It's not very effective!");


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
    
    void PlayNoise(AudioClip audio)
    {
        audioSource.clip = audio;
        audioSource.Play();
    }
}
