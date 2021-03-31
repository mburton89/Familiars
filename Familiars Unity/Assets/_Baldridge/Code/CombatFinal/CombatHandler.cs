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

    // Attack Preview Stuff

    int currentAttackPreview;
    int maxAttackPreview;
    
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

    private void Start()
    {
        Instance = this;
        GameControllerOverworld.Instance.SetCombat();
        StartBattle();
    }

    public void StartBattle()
    {
        combatState = CombatState.Start;
        
        targets = new List<CombatUnit>();

        // Create the currently used Familiars for both teams
        CombatUnit _curUnit;
        GameObject _fam;
        float _count = (float)playerTeam.Count;
        Tile _t;
        int _x, _y, iteration;

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
            _curUnit.x = _t.x;
            _curUnit.y = _t.y;
            playerHUDs[i].SetData(_curUnit.Familiar);

            playerTeam[i] = _curUnit;
            //playerTeam.Add(_curUnit);
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

            enemyTeam[i] = _curUnit;
            //enemyTeam.Add(_curUnit);
        }

        playerField.GatherNeighbors();
        enemyField.GatherNeighbors();

        FamiliarSelection();
    }

    #region State Shifters
    void FamiliarSelection()
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
        currentAttackPreview = 0;
        maxAttackPreview = 0;

        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableAttackSelector(true);

        targets.Clear();

        currentAttack = selectedFamiliar.Familiar.Attacks[currentAttackPosition];
        maxAttackPreview = currentAttack.Base.SourceArray.Length - 1;
        dialogMenu.UpdateAttackSelection(currentAttackPosition, currentAttack);

        StartCoroutine(AdvanceAttackPreview());
    }

    void MoveSelection()
    {
        combatState = CombatState.PlayerMove;

        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableDialogText(true);

        selectedFamiliar.SetCurrentTile(playerField.GetTile(currentPosition));
        List<Tile> _t = selectedFamiliar.FindSelectableTiles(TileState.Move, selectedFamiliar.Familiar.Base.Movement);
        for (int i = 0; i < _t.Count; i++)
        {
            _t[i].SetState(TileState.Move);
        }
    }

    void TargetSelection()
    {
        combatState = CombatState.PlayerTargeting;

        currentAttack = selectedFamiliar.Familiar.Attacks[currentAttackPosition];

        currentTargetPosition = currentAttack.Base.TargetOriginPosition;
        for (int i = 0; i < currentAttack.Base.SourceArray.Length; i++)
        {
            if (currentAttack.Base.SourceArray[i].Active[selectedFamiliar.x * 3 + selectedFamiliar.y])
            {
                currentAttackPreview = i;
            }
        }
        playerField.SetFieldPattern(currentAttack.Base.SourceArray[currentAttackPreview], TileState.ActiveSource);
        enemyField.SetFieldPattern(currentAttack.Base.TargetArray[currentAttackPreview], TileState.ActiveTarget);

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

        playerField.ClearTiles();
        enemyField.ClearTiles();

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

    #region Update Handlers
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
                MoveSelection();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            FamiliarSelection();
        }
    }

    void HandlePlayerAttack()
    {
        int _button = 0;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _button++;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _button--;
        }
        
        if (CheckButton(_button))
        {
            currentAttack = selectedFamiliar.Familiar.Attacks[currentAttackPosition];
            currentAttackPreview = 0;
            maxAttackPreview = currentAttack.Base.SourceArray.Length - 1;
            dialogMenu.UpdateAttackSelection(currentAttackPosition, currentAttack);
        }

        playerField.SetFieldPattern(currentAttack.Base.SourceArray[currentAttackPreview], TileState.ActiveSource, currentAttack.Base.Sources, TileState.Source);
        enemyField.SetFieldPattern(currentAttack.Base.TargetArray[currentAttackPreview], TileState.ActiveTarget, currentAttack.Base.Targets, TileState.Target);
        enemyField.SetFieldTargetingReticle(currentAttack.Base.TargetingReticleArray[currentAttackPreview], TileState.TargetReticle);

        
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // If the attack can be used at the current position
            if (currentAttack.Base.Sources.Active[currentPosition])
            {
                TargetSelection();
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
        if (currentAttack.Base.AttackStyle == AttackStyle.Area || currentAttack.Base.AttackStyle == AttackStyle.Target)
        {
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
        }

        enemyField.SetFieldPattern(currentAttack.Base.TargetArray[currentAttackPreview], TileState.ActiveTarget);
        enemyField.SetFieldTargetingReticle(currentAttack.Base.TargetingReticleArray[currentAttackPreview], TileState.TargetReticle, currentTargetPosition);
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
                    List<Tile> _t = MatchingTiles(enemyField, currentAttack.Base.TargetArray[currentAttackPreview], currentAttack.Base.EligibleOrigins);
                    if (_t.Count == 1)
                    {
                        Tile _checkingTile = _t[0];
                        
                        int _position = (_checkingTile.x * 3) + _checkingTile.y;
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

                StartCoroutine(PlayerMoving());
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ActionSelection();
        }
    }
    #endregion

    IEnumerator PlayerMoving()
    {
        combatState = CombatState.Busy;

        yield return dialogMenu.TypeDialog($"{selectedFamiliar.Familiar.Base.Name} has moved!");

        actions--;
        yield return new WaitForSeconds(0.3f);
        if (actions > 0)
        {
            FamiliarSelection();
        }
        else
        {
            EnemyAttack();
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
            if (attack.Base.Category == AttackCategory.Status)
            {
                var effects = attack.Base.Effects;
                if (effects.Boosts != null)
                {
                    if (attack.Base.Target == AttackTarget.Ally)
                    {
                        selectedFamiliar.Familiar.ApplyBoosts(effects.Boosts);
                    }
                    else if (attack.Base.Target == AttackTarget.Enemy)
                    {
                        targets[i].Familiar.ApplyBoosts(effects.Boosts);
                    }
                }
            }
            else
            {
                var damageDetails = targets[i].Familiar.TakeDamage(attack, selectedFamiliar.Familiar);
                targets[i].PlayHitAnimation();

                PlayNoise(targets[i].Familiar.Base.AttackSound);
                yield return enemyHUDs[targets[i].teamPosition].UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }
            

            if (targets[i].Familiar.HP <= 0)
            {
                yield return ResolveFainting(targets[i]);
            }
        }

        actions--;
        yield return new WaitForSeconds(1f);
        if (actions > 0)
        {
            FamiliarSelection();
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
            CombatUnit _enemyUnit = enemyTeam[UnityEngine.Random.Range(0, enemyTeam.Count)];

            // This is when we would have to verify a valid target, attack, and all that such AI decision making and stuff
            AttackAction action = EnemyAI.Instance.FindAttack(_enemyUnit, playerField, AILevel.Wild);
            Attack attack = action.Attack;
            if (attack != null)
            {
                yield return dialogMenu.TypeDialog($"{_enemyUnit.Familiar.Base.Name} used {attack.Base.Name}");
                PlayNoise(_enemyUnit.Familiar.Base.AttackSound);

                _enemyUnit.PlayAttackAnimation();
                yield return new WaitForSeconds(1f);

                //CombatUnit _target = playerTeam[UnityEngine.Random.Range(0, 3)];
                List<CombatUnit> _targets = action.Targets;

                for (int j = 0; j < _targets.Count; j++)
                {
                    _targets[j].PlayHitAnimation();
                    var damageDetails = _targets[j].Familiar.TakeDamage(attack, _enemyUnit.Familiar);
                    PlayNoise(_targets[j].Familiar.Base.AttackSound);
                    yield return playerHUDs[_targets[j].teamPosition].UpdateHP();
                    yield return ShowDamageDetails(damageDetails);

                    if (damageDetails.Fainted)
                    {
                        yield return ResolveFainting(_targets[j]);
                    }
                }
            }
            else
            {
                _enemyUnit.SetCurrentTile(action.Location);
                yield return dialogMenu.TypeDialog($"{_enemyUnit.Familiar.Base.Name} has moved!");
            }
        }

        actions = 3;
        FamiliarSelection();
    }

    IEnumerator ResolveFainting(CombatUnit unit)
    {
        yield return dialogMenu.TypeDialog($"{unit.Familiar.Base.Name} fainted");
        unit.PlayFaintAnimation();

        Tile _tile = null;
        if (unit.isPlayerUnit)
        {
            _tile = playerField.GetTile(unit.x * 3 + unit.y);
            _tile.familiarOccupant = null;
            playerTeam.Remove(unit);

            for (int i = 0; i < 9; i++)
            {
                Debug.Log("[CombatHandler.cs/ResolveFainted()] Player Field position " + i + ": " + playerField.GetTile(i).familiarOccupant);
            }
        }
        else
        {
            _tile = enemyField.GetTile(unit.x * 3 + unit.y);
            _tile.familiarOccupant = null;
            enemyTeam.Remove(unit);

            for (int i = 0; i < 9; i++)
            {
                Debug.Log("[CombatHandler.cs/ResolveFainted()] Enemy Field position " + i + ": " + enemyField.GetTile(i).familiarOccupant);
            }
        }
        
        yield return new WaitForSeconds(0.75f);
        Destroy(unit.gameObject);
        CheckEndBattle();
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

    IEnumerator AdvanceAttackPreview()
    {
        yield return new WaitForSeconds(1.2f);
        if (combatState == CombatState.PlayerAttack)
        {
            if (currentAttackPreview == maxAttackPreview)
            {
                currentAttackPreview = 0;
            }
            else currentAttackPreview++;
            yield return AdvanceAttackPreview();
        }
    }

    bool CheckButton(int dir)
    {
        if (dir != 0)
        {
            if (dir > 0)
            {
                if (currentAttackPosition < selectedFamiliar.Familiar.Attacks.Count - 1)
                    currentAttackPosition++;
                else
                    currentAttackPosition = 0;
            }
            else if (dir < 0)
            {
                if (currentAttackPosition > 0)
                    currentAttackPosition--;
                else
                    currentAttackPosition = selectedFamiliar.Familiar.Attacks.Count - 1;
            }
            return true;
        }
        
        return false;
    }

    List<Tile> MatchingTiles(Field field, PatternBase pattern1, PatternBase pattern2)
    {
        List<Tile> tiles = new List<Tile>();

        for (int i = 0; i < pattern1.Active.Length; i++)
        {
            if (pattern1.Active[i] && pattern2.Active[i])
            {
                tiles.Add(field.GetTile(i));
            }
        }
        return tiles;
    }

    void CheckEndBattle()
    {
        if (playerTeam.Count <= 0)
        {
            OnBattleOver(false);
        }
        if (enemyTeam.Count <= 0)
        {
            OnBattleOver(true);
        }
    }
}
