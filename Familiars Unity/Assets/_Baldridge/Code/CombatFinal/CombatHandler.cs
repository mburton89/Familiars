using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum CombatState { Start, ActionSelection, FamiliarSelection, AttackSelection, MoveSelection, TargetSelection,
    SwitchSelection, SwitchTarget, CatchSelection, PerformAttack, Busy }

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

    [SerializeField] FamiliarPartyManager familiarPartyManager;
    #endregion

    public event Action<bool> OnBattleOver;

    public List<CombatUnit> playerTeam;
    public List<CombatUnit> enemyTeam;

    // Units from both sides
    public List<GameObject> playerActive;
    public List<GameObject> playerBench;
    public List<GameObject> enemyActive;
    public List<GameObject> enemyBench;

    // Music

    [SerializeField] GameMusic music;

    CombatState nextState;

    int actions = 3;

    // Familiar Switching Stuff

    Familiar switchTarget;
    Familiar switchTarget2;

    int switchTargetPosition;
    int switchPlacementPosition;

    bool switchingIn;

    // Attack Preview Stuff

    int currentAttackPreview;
    int maxAttackPreview;

    // Targetting Stuffs

    int currentPosition;
    int currentTargetPosition;
    int currentActionPosition;
    int currentAttackPosition;

    Attack currentAttack;
    Field currentField;

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
        GameController.Instance.SetCombat();
        StartBattle();
    }

    public void StartBattle()
    {
        combatState = CombatState.Start;

        targets = new List<CombatUnit>();

        // Create the currently used Familiars for both teams
        CombatUnit _curUnit;
        GameObject _fam;

        Tile _t;
        int _x, _y, iteration;

        #region Initialization
        List<Familiar> _aliveParty = CurrentFamiliarsController.Instance.GetHealthyFamiliars(CurrentFamiliarsController.Instance.playerFamiliars);
        int _count = _aliveParty.Count;
        for (int i = 0; i < Mathf.Min(3, _count); i++)
        {
            _fam = Instantiate(combatUnitPrefab, canvas.gameObject.transform);
            _curUnit = _fam.GetComponent<CombatUnit>();
            _curUnit.Familiar = _aliveParty[i];

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
            _curUnit.Hud = playerHUDs[i];
            _curUnit.Hud.SetData(_curUnit.Familiar);
            _curUnit.Hud.Active(true);

            playerTeam.Add(_curUnit);
            Debug.Log("Player Initial -- Name: " + _curUnit.Familiar.Base.Name + " ID: " + _curUnit.Familiar.RandomID);
            //playerTeam.Add(_curUnit);
        }

        _count = CurrentFamiliarsController.Instance.enemyFamiliars.Count;
        for (int i = 0; i < Mathf.Min(3, _count); i++)
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
            _curUnit.Hud = enemyHUDs[i];
            _curUnit.Hud.SetData(_curUnit.Familiar);
            _curUnit.Hud.Active(true);

            enemyTeam.Add(_curUnit);
            Debug.Log("Enemy Initial -- Name: " + _curUnit.Familiar.Base.Name + " ID: " + _curUnit.Familiar.RandomID);
            //enemyTeam.Add(_curUnit);
        }
        #endregion
        playerField.GatherNeighbors();
        enemyField.GatherNeighbors();

        ActionSelection();
    }

    #region State Shifters
    void FamiliarSelection()
    {
        combatState = CombatState.FamiliarSelection;

        //currentActionPosition = 0;
        currentAttackPosition = 0;
        currentAttack = null;
        selectedFamiliar = null;

        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableDialogText(true);
        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableAttackSelector(false);

        if (currentActionPosition == 0)
        {
            dialogMenu.SetDialog("Select a familiar to attack with.");
        }
        else if (currentActionPosition == 1)
        {
            dialogMenu.SetDialog("Select a familiar to move.");
        }
        navigator.SetActive(true);
    }

    void ActionSelection()
    {
        combatState = CombatState.ActionSelection;

        playerField.ClearTiles();
        enemyField.ClearTiles();

        familiarPartyManager.ClosePanel();

        dialogMenu.EnableDialogText(false);
        dialogMenu.EnableAttackSelector(false);
        dialogMenu.EnableActionSelector(true);

        DisplayFamiliars(true);

        navigator.SetActive(false);
    }

    void AttackSelection()
    {
        combatState = CombatState.AttackSelection;

        currentAttack = null;
        currentAttackPreview = 0;
        maxAttackPreview = 0;

        dialogMenu.EnableDialogText(false);
        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableAttackSelector(true);

        targets.Clear();

        currentAttack = selectedFamiliar.Familiar.Attacks[currentAttackPosition];
        maxAttackPreview = currentAttack.Base.SourceArray.Length - 1;
        dialogMenu.UpdateAttackSelection(currentAttackPosition, currentAttack);

        if (currentAttack.Base.Target == AttackTarget.Ally)
        {
            currentField = playerField;
        }
        else if (currentAttack.Base.Target == AttackTarget.Enemy)
        {
            currentField = enemyField;
        }

        StartCoroutine(AdvanceAttackPreview());
    }

    void MoveSelection()
    {
        combatState = CombatState.MoveSelection;

        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableDialogText(true);

        dialogMenu.SetDialog($"Select a blue tile to move {selectedFamiliar.Familiar.Base.Name} to.");

        selectedFamiliar.SetCurrentTile(playerField.GetTile(currentPosition));
        List<Tile> _t = selectedFamiliar.FindSelectableTiles(TileState.Move, selectedFamiliar.Familiar.Base.Movement);

        for (int i = 0; i < _t.Count; i++)
        {
            _t[i].SetState(TileState.Move);
        }

        navigator.SetActive(true);
    }

    void TargetSelection()
    {
        combatState = CombatState.TargetSelection;

        currentAttack = selectedFamiliar.Familiar.Attacks[currentAttackPosition];

        currentTargetPosition = currentAttack.Base.TargetOriginPosition;
        for (int i = 0; i < currentAttack.Base.SourceArray.Length; i++)
        {
            if (currentAttack.Base.SourceArray[i].Active[selectedFamiliar.x * 3 + selectedFamiliar.y])
            {
                currentAttackPreview = i;
            }
        }
        if (currentAttack.Base.Target == AttackTarget.Ally)
        {
            playerField.SetFieldPattern(currentAttack.Base.TargetArray[currentAttackPreview], TileState.AllyTarget);
        }
        else if (currentAttack.Base.Target == AttackTarget.Enemy)
        {
            playerField.SetFieldPattern(currentAttack.Base.SourceArray[currentAttackPreview], TileState.ActiveSource);
            enemyField.SetFieldPattern(currentAttack.Base.TargetArray[currentAttackPreview], TileState.ActiveTarget);
        }

        navigator.SetActive(true);

        upperBoundX = currentAttack.Base.UpperX;
        upperBoundY = currentAttack.Base.UpperY;
        lowerBoundX = currentAttack.Base.LowerX;
        lowerBoundY = currentAttack.Base.LowerY;
    }

    void SwitchSelection()
    {
        combatState = CombatState.SwitchSelection;

        navigator.SetActive(false);

        DisplayFamiliars(false);
        navigator.SetActive(false);
        familiarPartyManager.OpenPanel(playerField);

        familiarPartyManager.UnlockPanels();
    }

    void SwitchTarget()
    {
        combatState = CombatState.SwitchTarget;

        if (switchingIn)
        {
            navigator.SetActive(true);
            navigator.SetLocation(familiarPartyManager.GetTile(4));
        }

        familiarPartyManager.SetPanel(switchTargetPosition, PanelState.Selected);
        familiarPartyManager.LockPanel(switchTargetPosition);
    }

    void Switching()
    {
        combatState = CombatState.Busy;

        dialogMenu.EnableDialogText(true);
        dialogMenu.EnableActionSelector(false);

        navigator.SetActive(false);

        familiarPartyManager.UnlockPanels();
        familiarPartyManager.ClosePanel();

        DisplayFamiliars(true);

        CombatUnit _cu = null;
        Familiar _new = null;
        if (switchingIn)
        {
            _cu = FindUnit(switchTarget2);
            _new = switchTarget;
        }
        else
        {
            _cu = FindUnit(switchTarget);
            _new = switchTarget2;
        }

        StartCoroutine(PlayerSwitchFamiliar(_cu, _new));
    }


    void CatchSelection()
    {
        combatState = CombatState.CatchSelection;

        dialogMenu.EnableActionSelector(false);
        dialogMenu.EnableDialogText(true);

        navigator.SetActive(true);

        dialogMenu.SetDialog($"Select an enemy familiar to attempt to catch.");
    }

    void StartPlayerAttack()
    {
        playerField.ClearTiles();
        enemyField.ClearTiles();

        dialogMenu.EnableAttackSelector(false);
        dialogMenu.EnableDialogText(true);

        navigator.SetActive(false);

        StartCoroutine(PlayerAttack());
    }

    void EnemyAttack()
    {
        combatState = CombatState.PerformAttack;

        playerField.ClearTiles();
        enemyField.ClearTiles();

        navigator.SetActive(false);

        StartCoroutine(PerformEnemyAttack());
    }
    #endregion

    void Update()
    {
        switch (combatState)
        {
            case CombatState.ActionSelection:
                HandlePlayerAction();
                break;
            case CombatState.FamiliarSelection:
                HandlePlayerSelection();
                break;
            case CombatState.AttackSelection:
                HandlePlayerAttack();
                break;
            case CombatState.MoveSelection:
                HandlePlayerMove();
                break;
            case CombatState.TargetSelection:
                HandlePlayerTargeting();
                break;
            case CombatState.SwitchSelection:
                HandlePlayerSwitch();
                break;
            case CombatState.SwitchTarget:
                HandlePlayerSwitchTarget();
                break;
            case CombatState.CatchSelection:
                HandlePlayerCatch();
                break;
        }
    }

    #region Update Handlers

    void HandlePlayerAction()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PlayNoise(music.buttonClick1SFX);
            if (currentActionPosition < 4)
                ++currentActionPosition;

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PlayNoise(music.buttonClick1SFX);
            if (currentActionPosition > 0)
                --currentActionPosition;
        }

        dialogMenu.UpdateActionSelection(currentActionPosition);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            PlayNoise(music.buttonClick3SFX);
            if (currentActionPosition == 0 || currentActionPosition == 1)
            {
                // Fight
                //dialogMenu.SetAttackNames(selectedFamiliar.Familiar.Attacks);
                //AttackSelection();
                FamiliarSelection();
            }
            else if (currentActionPosition == 2)
            {
                SwitchSelection();
            }
            else if (currentActionPosition == 3)
            {
                CatchSelection();
            }
            else if (currentActionPosition == 4)
            {
                OnBattleOver(true);
            }
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
                PlayNoise(music.buttonClick1SFX);
                currentPosition += 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPosition > 2)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition -= 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentPosition % 3 != 2)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition++;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentPosition % 3 != 0)
            {
                PlayNoise(music.buttonClick1SFX);
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
                if (_combatUnit.Familiar.CanAct)
                {
                    PlayNoise(music.buttonClick3SFX);
                    selectedFamiliar = _combatUnit;
                    if (currentActionPosition == 0)
                    {
                        dialogMenu.SetAttackNames(selectedFamiliar.Familiar.Attacks);
                        AttackSelection();
                    }
                    else if (currentActionPosition == 1)
                    {
                        MoveSelection();
                    }
                    //ActionSelection();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayNoise(music.buttonClick2SFX);
            ActionSelection();
        }

    }
    #region Player Attack
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
            PlayNoise(music.buttonClick1SFX);
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
                PlayNoise(music.buttonClick3SFX);
                TargetSelection();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            FamiliarSelection();
        }
    }
    #endregion
    #region Player Targeting

    void HandlePlayerTargeting()
    {
        #region Navigation
        if (currentAttack.Base.Target == AttackTarget.Enemy)
        {
            // Move left (add switching to supp. board later)
            if (currentAttack.Base.AttackStyle == AttackStyle.Area || currentAttack.Base.AttackStyle == AttackStyle.Target)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (currentTargetPosition < (upperBoundX * 3) && selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets.Active[currentTargetPosition + 3])
                    {
                        PlayNoise(music.buttonClick1SFX);
                        currentTargetPosition += 3;
                    }
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (currentTargetPosition > (lowerBoundX + 2) && selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets.Active[currentTargetPosition - 3])
                    {
                        PlayNoise(music.buttonClick1SFX);
                        currentTargetPosition -= 3;
                    }
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (currentTargetPosition % 3 < upperBoundY && selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets.Active[currentTargetPosition + 1])
                    {
                        PlayNoise(music.buttonClick1SFX);
                        currentTargetPosition++;
                    }
                }
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (currentTargetPosition % 3 > lowerBoundY && selectedFamiliar.Familiar.Attacks[currentAttackPosition].Base.Targets.Active[currentTargetPosition - 1])
                    {
                        PlayNoise(music.buttonClick1SFX);
                        currentTargetPosition--;
                    }
                }
            }

            enemyField.SetFieldPattern(currentAttack.Base.TargetArray[currentAttackPreview], TileState.ActiveTarget);
            enemyField.SetFieldTargetingReticle(currentAttack.Base.TargetingReticleArray[currentAttackPreview], TileState.TargetReticle, currentTargetPosition);
        }
        else if (currentAttack.Base.Target == AttackTarget.Ally)
        {
            // Move left (add switching to supp. board later)
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (currentTargetPosition < 6)
                {
                    PlayNoise(music.buttonClick1SFX);
                    currentTargetPosition += 3;
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (currentTargetPosition > 2)
                {
                    PlayNoise(music.buttonClick1SFX);
                    currentTargetPosition -= 3;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (currentTargetPosition % 3 != 2)
                {
                    PlayNoise(music.buttonClick1SFX);
                    currentTargetPosition++;
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (currentTargetPosition % 3 != 0)
                {
                    PlayNoise(music.buttonClick1SFX);
                    currentTargetPosition--;
                }
            }

            playerField.SetFieldPattern(currentAttack.Base.TargetArray[currentAttackPreview], TileState.ActiveAllyTarget);
            playerField.SetFieldTargetingReticle(currentAttack.Base.TargetingReticleArray[currentAttackPreview], TileState.AllyTargetReticle, currentTargetPosition);
        }

        //navigator.SetLocation(enemyField.GetTile(currentTargetPosition));
        #endregion
        #region Targetting
        if (Input.GetKeyDown(KeyCode.Z))
        {
            bool valid = false;
            // Assign Targets
            switch (currentAttack.Base.AttackStyle)
            {
                case AttackStyle.Target:
                    if (currentField.GetTile(currentTargetPosition).familiarOccupant != null && currentAttack.Base.Targets.Active[currentTargetPosition])
                    {
                        targets.Add(currentField.GetTile(currentTargetPosition).familiarOccupant);
                        valid = true;
                    }
                    break;
                case AttackStyle.Projectile:
                    List<Tile> _t = MatchingTiles(currentField, currentAttack.Base.TargetArray[currentAttackPreview], currentAttack.Base.EligibleOrigins);
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
                                _checkingTile = currentField.GetTile(_position);
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
                        if (currentField.GetTile(i).familiarOccupant != null && currentField.GetTile(i).GetState() == TileState.TargetReticle)
                        {
                            valid = true;
                            targets.Add(currentField.GetTile(i).familiarOccupant);
                        }
                    }
                    break;
                case AttackStyle.AreaStatic:
                    for (int i = 0; i < 9; i++)
                    {
                        if (currentField.GetTile(i).familiarOccupant != null && currentAttack.Base.TargetArray[currentAttackPreview].Active[i])
                        {
                            valid = true;
                            targets.Add(currentField.GetTile(i).familiarOccupant);
                        }
                    }
                    break;
            }

            if (valid)
            {
                PlayNoise(music.buttonClick3SFX);
                StartPlayerAttack();
            }


        }
        #endregion

        if (Input.GetKeyDown(KeyCode.X))
        {
            AttackSelection();
        }
    }
    #endregion

    void HandlePlayerMove()
    {
        #region Navigator
        // Move left (add switching to supp. board later)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentPosition < 6)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition += 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPosition > 2)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition -= 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentPosition % 3 != 2)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition++;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentPosition % 3 != 0)
            {
                PlayNoise(music.buttonClick1SFX);
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
                PlayNoise(music.buttonClick3SFX);
                selectedFamiliar.SetCurrentTile(_temp);

                StartCoroutine(PlayerMoving());
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            FamiliarSelection();
        }
    }


    void HandlePlayerSwitch()
    {
        #region Navigation
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (switchTargetPosition > 2)
            {
                PlayNoise(music.buttonClick1SFX); switchTargetPosition -= 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (switchTargetPosition < 3 && switchTargetPosition + 3 < CurrentFamiliarsController.Instance.playerFamiliars.Count)
            {
                PlayNoise(music.buttonClick1SFX);
                switchTargetPosition += 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (switchTargetPosition > 0)
            {
                PlayNoise(music.buttonClick1SFX);
                switchTargetPosition--;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (switchTargetPosition < CurrentFamiliarsController.Instance.playerFamiliars.Count - 1)
            {
                PlayNoise(music.buttonClick1SFX);
                switchTargetPosition++;
            }
        }

        familiarPartyManager.SetPanel(switchTargetPosition, PanelState.Hover);
        #endregion


        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (CurrentFamiliarsController.Instance.playerFamiliars[switchTargetPosition].HP > 0)
            {
                PlayNoise(music.buttonClick3SFX);
                if (switchTargetPosition <= 2)
                {
                    switchingIn = false;
                }
                else
                {
                    switchingIn = true;

                }
                switchTarget = CurrentFamiliarsController.Instance.playerFamiliars[switchTargetPosition];
                SwitchTarget();
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            ActionSelection();
        }
    }

    void HandlePlayerSwitchTarget()
    {
        // Switching in -- first selecting a familiar that is not in battle
        // and choosing a familiar that is in battle for it to replace.
        if (switchingIn)
        {
            #region Navigation

            // Move left (add switching to supp. board later)
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (switchPlacementPosition < 6)
                {
                    PlayNoise(music.buttonClick1SFX);
                    switchPlacementPosition += 3;
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (switchPlacementPosition > 2)
                {
                    PlayNoise(music.buttonClick1SFX);
                    switchPlacementPosition -= 3;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (switchPlacementPosition % 3 != 2)
                {
                    PlayNoise(music.buttonClick1SFX);
                    switchPlacementPosition++;
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (switchPlacementPosition % 3 != 0)
                {
                    PlayNoise(music.buttonClick1SFX);
                    switchPlacementPosition--;
                }
            }

            navigator.SetLocation(familiarPartyManager.GetTile(switchPlacementPosition));
            #endregion

            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (playerField.GetTile(switchPlacementPosition).familiarOccupant != null)
                {
                    PlayNoise(music.buttonClick3SFX);
                    switchTarget2 = playerField.GetTile(switchPlacementPosition).familiarOccupant.Familiar;
                    Switching();
                }
            }
        }
        // Switching out -- first selecting a familiar that is already in battle,
        // then choosing a new familiar to take it's place
        else
        {
            #region Navigation
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (switchTargetPosition > 2)
                {
                    PlayNoise(music.buttonClick1SFX);
                    switchTargetPosition -= 3;
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (switchTargetPosition < 3 && switchTargetPosition + 3 < CurrentFamiliarsController.Instance.playerFamiliars.Count)
                {
                    PlayNoise(music.buttonClick1SFX);
                    switchTargetPosition += 3;
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (switchTargetPosition > 0)
                {
                    PlayNoise(music.buttonClick1SFX);
                    switchTargetPosition--;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (switchTargetPosition < CurrentFamiliarsController.Instance.playerFamiliars.Count - 1)
                {
                    PlayNoise(music.buttonClick1SFX);
                    switchTargetPosition++;
                }
            }

            familiarPartyManager.SetPanel(switchTargetPosition, PanelState.Hover);
            #endregion

            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (switchTargetPosition > 2 && CurrentFamiliarsController.Instance.playerFamiliars[switchTargetPosition].HP > 0)
                {
                    PlayNoise(music.buttonClick3SFX);
                    switchTarget2 = CurrentFamiliarsController.Instance.playerFamiliars[switchTargetPosition];
                    Switching();
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.X))
        {
            SwitchSelection();
        }
    }

    void HandlePlayerCatch()
    {
        #region Navigation
        // Move left (add switching to supp. board later)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentPosition > 2)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition -= 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentPosition < 6)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition += 3;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentPosition % 3 != 2)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition++;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentPosition % 3 != 0)
            {
                PlayNoise(music.buttonClick1SFX);
                currentPosition--;
            }
        }

        navigator.SetLocation(enemyField.GetTile(currentPosition));
        #endregion

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (enemyField.GetTile(currentPosition).familiarOccupant != null)
            {
                PlayNoise(music.buttonClick3SFX);
                StartCoroutine(AttemptCatching(enemyField.GetTile(currentPosition).familiarOccupant));
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            ActionSelection();
        }
    }
    #endregion

    IEnumerator PlayerMoving()
    {
        combatState = CombatState.PerformAttack;

        yield return dialogMenu.TypeDialog($"{selectedFamiliar.Familiar.Base.Name} has moved!");

        actions--;
        yield return new WaitForSeconds(0.3f);
        if (actions > 0)
        {
            ActionSelection();
        }
        else
        {
            EnemyAttack();
        }
    }

    /*  ======================================================================
     *   Different Attack Coroutines 
     *  ====================================================================== */
    #region Attack Coroutines
    IEnumerator PlayerAttack()
    {
        combatState = CombatState.PerformAttack;

        var attack = currentAttack;
        yield return RunAttack(selectedFamiliar, targets, attack);

        actions--;
        yield return new WaitForSeconds(1f);
        if (actions > 0)
        {
            ActionSelection();
        }
        else
        {
            EnemyAttack();
        }
    }

    IEnumerator PerformEnemyAttack()
    {
        combatState = CombatState.PerformAttack;

        for (int i = 0; i < 3; i++)
        {
            CombatUnit _enemyUnit = EnemyAI.Instance.FindUnit(enemyTeam, AILevel.Wild);//enemyTeam[UnityEngine.Random.Range(0, enemyTeam.Count)];

            if (_enemyUnit == null)
            {
                break;
            }

            // This is when we would have to verify a valid target, attack, and all that such AI decision making and stuff
            AttackAction action = EnemyAI.Instance.FindAttack(_enemyUnit, playerField, AILevel.Wild);
            Attack attack = action.Attack;
            if (attack != null)
            {
                Debug.Log(action.Targets.Count);
                yield return RunAttack(_enemyUnit, action.Targets, attack);
            }
            else
            {
                _enemyUnit.SetCurrentTile(action.Location);
                yield return dialogMenu.TypeDialog($"{_enemyUnit.Familiar.Base.Name} has moved!");
            }
        }

        yield return EndOfTurn();
    }

    IEnumerator RunAttack(CombatUnit sourceUnit, List<CombatUnit> targetUnit, Attack attack)
    {
        bool canRunMove = sourceUnit.Familiar.OnBeforeAttack();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Familiar);
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Familiar);

        yield return dialogMenu.TypeDialog($"{sourceUnit.Familiar.Base.Name} used {attack.Base.Name}");
        PlayNoise(sourceUnit.Familiar.Base.AttackSound);

        List<CombatUnit> hitTargets = CheckIfAttackHits(attack, sourceUnit, targetUnit);
        if (hitTargets.Count > 0)
        {
            if (hitTargets.Count < targetUnit.Count)
            {
                yield return dialogMenu.TypeDialog($"Some targets dodged {sourceUnit.Familiar.Base.Name}'s attack.");
            }
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            // Probably replace w/ PerformAttack() when adding multi-targetting (probably)
            for (int i = 0; i < hitTargets.Count; i++)
            {
                if (attack.Base.Category == AttackCategory.Status)
                {
                    yield return RunAttackEffects(sourceUnit, hitTargets[i], attack.Base.Effects, attack.Base.Target);
                }
                else
                {
                    var damageDetails = hitTargets[i].Familiar.TakeDamage(attack, sourceUnit.Familiar);
                    hitTargets[i].PlayHitAnimation();

                    PlayNoise(hitTargets[i].Familiar.Base.AttackSound);
                    yield return hitTargets[i].Hud.UpdateHP();
                    yield return ShowDamageDetails(damageDetails);
                }

                if (attack.Base.Secondaries != null && attack.Base.Secondaries.Count > 0 && hitTargets[i].Familiar.HP > 0)
                {
                    foreach (var secondary in attack.Base.Secondaries)
                    {
                        var rnd = UnityEngine.Random.Range(1, 101);
                        if (rnd <= secondary.Chance)
                            yield return RunAttackEffects(sourceUnit, hitTargets[i], secondary, secondary.Target);
                    }
                }


                if (targetUnit[i].Familiar.HP <= 0)
                {
                    yield return ResolveFainting(targetUnit[i]);
                }
            }
        }
        else
        {
            yield return dialogMenu.TypeDialog($"{sourceUnit.Familiar.Base.Name}'s attack missed.");
        }

        sourceUnit.Familiar.OnAfterAttack();

    }

    IEnumerator RunAttackEffects(CombatUnit source, CombatUnit target, AttackEffects effects, AttackTarget attackTarget)
    {
        Debug.Log("[CombatHandler.cs/RunAttackEffects()] Running Attack Effects.");


        // Stat Boosting
        if (effects.Boosts != null)
        {
            target.Familiar.ApplyBoosts(effects.Boosts);
        }

        // Status Condition
        if (effects.Status != ConditionID.none)
        {
            target.Familiar.SetStatus(effects.Status);
        }

        // Volatile Status Condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.Familiar.SetVolatileStatus(effects.VolatileStatus);
        }

        if (effects.Movement.move)
        {
            Field _field = playerField;
            if (target.isPlayerUnit)
                _field = playerField;
            else if (!target.isPlayerUnit)
                _field = enemyField;

            Tile _t = target.GetCurrentTile(false);
            // back 0
            if (effects.Movement.direction == 0)
            {
                int cur = 1;
                int max = effects.Movement.squares + 1;
                for (cur = 1; cur < max; cur++)
                {
                    if (_field.GetTile((target.x + cur) * 3 + target.y).familiarOccupant != null)
                    {
                        break;
                    }
                    _t = _field.GetTile((target.x + cur) * 3 + target.y);
                }

            }

            target.SetCurrentTile(_t);
            //target.SetCurrentTile

            // forward 2
        }

        yield return ShowStatusChanges(target.Familiar);
    }

    List<CombatUnit> CheckIfAttackHits(Attack attack, CombatUnit source, List<CombatUnit> target)
    {
        if (attack.Base.AlwaysHits)
            return target;

        List<CombatUnit> hitTargets = new List<CombatUnit>();

        float moveAccuracy = attack.Base.Accuracy;

        int accuracy = source.Familiar.StatBoosts[Stat.Accuracy];
        int evasion;
        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };
        for (int i = 0; i < target.Count; i++)
        {
            evasion = target[i].Familiar.StatBoosts[Stat.Evasion];

            if (accuracy > 0)
                moveAccuracy *= boostValues[accuracy];
            else
                moveAccuracy /= boostValues[-accuracy];

            if (evasion > 0)
                moveAccuracy /= boostValues[evasion];
            else
                moveAccuracy *= boostValues[-evasion];

            if (UnityEngine.Random.Range(1, 101) <= moveAccuracy)
            {
                hitTargets.Add(target[i]);
            }
        }

        return hitTargets;
    }

    IEnumerator ShowStatusChanges(Familiar familiar)
    {
        while (familiar.StatusChanges.Count > 0)
        {
            var message = familiar.StatusChanges.Dequeue();
            yield return dialogMenu.TypeDialog(message);
        }
    }
    #endregion

    IEnumerator ResolveFainting(CombatUnit unit)
    {
        yield return dialogMenu.TypeDialog($"{unit.Familiar.Base.Name} fainted");
        unit.PlayFaintAnimation();
        yield return new WaitForSeconds(1f);

        Tile _tile = null;
        if (unit.isPlayerUnit)
        {
            _tile = playerField.GetTile(unit.x * 3 + unit.y);

            // Available to switch?
            if (CurrentFamiliarsController.Instance.GetHealthyFamiliars(CurrentFamiliarsController.Instance.playerFamiliars).Count >= 3)
            {
                yield return SwitchOnFaint(unit);
            }
            else
            {
                _tile.familiarOccupant = null;

                playerTeam.Remove(unit);

                for (int i = 0; i < playerTeam.Count; i++)
                {
                    playerTeam[i].teamPosition = i;
                }

                for (int i = 0; i < 3; i++)
                {
                    if (i < playerTeam.Count)
                    {
                        playerTeam[i].Hud = playerHUDs[i];
                        playerTeam[i].Hud.SetData(playerTeam[i].Familiar);
                        playerTeam[i].Hud.Active(true);
                    }
                    else
                    {
                        playerHUDs[i].Active(false);
                    }
                }

                Destroy(unit.gameObject);
                CheckEndBattle();
            }
        }
        else
        {
            // EXP Gain
            int expYield = unit.Familiar.Base.ExpYield;
            int enemyLevel = unit.Familiar.Level;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel) / 7);

            // Give EXP to the boys
            List<Familiar> _aliveParty = CurrentFamiliarsController.Instance.GetHealthyFamiliars(CurrentFamiliarsController.Instance.playerFamiliars);
            int val = Mathf.Min(_aliveParty.Count, 3);
            for (int i = 0; i < val; i++)
            {
                CombatUnit _p = playerTeam[i];
                _p.Familiar.EXP += expGain / val;

                while (_p.Familiar.CheckForLevelUp())
                {
                    _p.Hud.SetLevel();
                    yield return dialogMenu.TypeDialog($"{_p.Familiar.Base.Name} grew to level {_p.Familiar.Level}");

                    // Try to learn a new move
                    var newAttack = _p.Familiar.GetLearnableAttackAtCurrLevel();
                    if (newAttack != null)
                    {
                        if (_p.Familiar.Attacks.Count < FamiliarBase.MaxNumOfAttacks)
                        {
                            _p.Familiar.LearnMove(newAttack);
                            yield return dialogMenu.TypeDialog($"{_p.Familiar.Base.Name} has learned a new attack -- {newAttack.Base.Name}!");
                        }
                        else
                        {
                            // TODO: Option to forget a move
                        }
                    }
                }
            }

            // Level Up


            _tile = enemyField.GetTile(unit.x * 3 + unit.y);

            // Available to switch?
            if (CurrentFamiliarsController.Instance.GetHealthyFamiliars(CurrentFamiliarsController.Instance.enemyFamiliars).Count >= 3)
            {
                yield return SwitchOnFaint(unit);
            }
            else
            {
                _tile.familiarOccupant = null;
                enemyTeam.Remove(unit);

                for (int i = 0; i < enemyTeam.Count; i++)
                {
                    enemyTeam[i].teamPosition = i;
                }

                for (int i = 0; i < 3; i++)
                {
                    if (i < enemyTeam.Count)
                    {
                        enemyTeam[i].Hud = enemyHUDs[i];
                        enemyTeam[i].Hud.SetData(enemyTeam[i].Familiar);
                        enemyTeam[i].Hud.Active(true);
                    }
                    else
                    {
                        enemyHUDs[i].Active(false);
                    }
                }

                Destroy(unit.gameObject);
                CheckEndBattle();
            }
        }

        yield return new WaitForSeconds(0.75f);
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

    IEnumerator EndOfTurn()
    {
        for (int i = 0; i < playerTeam.Count; i++)
        {
            playerTeam[i].Familiar.OnAfterTurn();
            yield return ShowStatusChanges(playerTeam[i].Familiar);
            yield return playerTeam[i].Hud.UpdateHP();

            if (playerTeam[i].Familiar.HP <= 0)
            {
                yield return ResolveFainting(playerTeam[i]);
            }
        }

        for (int i = 0; i < enemyTeam.Count; i++)
        {
            enemyTeam[i].Familiar.OnAfterTurn();
            yield return ShowStatusChanges(enemyTeam[i].Familiar);
            yield return enemyTeam[i].Hud.UpdateHP();

            if (enemyTeam[i].Familiar.HP <= 0)
            {
                yield return ResolveFainting(enemyTeam[i]);
            }
        }

        //playerTeam.ForEach(f => f.Familiar.OnAfterTurn());
        //enemyTeam.ForEach(f => f.Familiar.OnAfterTurn());
        actions = 3;
        ActionSelection();
    }

    IEnumerator AttemptCatching(CombatUnit unit)
    {
        yield return dialogMenu.TypeDialog($"Attempting to catch {unit.Familiar.Base.Name}...");

        int currentUnits = enemyTeam.Count;
        float hpPercent = unit.Familiar.HP / unit.Familiar.MaxHp * 100;
        bool afflicted = unit.Familiar.Status != null;
        int bonus = 0;
        if (afflicted) bonus = 30;
        bonus += (int)(100f - hpPercent);

        unit.PlayCatchFlashAnimation();
        yield return new WaitForSeconds(0.5f);

        int threshold = 50 + ((currentUnits - 1) * 20) - bonus;
        Debug.Log("[CombatHandler.cs/AttemptCatching()] threshold = " + threshold);
        if (UnityEngine.Random.Range(0, 100) > threshold)
        {
            // Successful Catching
            yield return dialogMenu.TypeDialog("... and it worked!");

            unit.PlayCatchSucceedAnimation();
            yield return new WaitForSeconds(0.5f);

            PlayerParty.Instance.familiars.Add(unit.Familiar);

            Tile _tile = enemyField.GetTile(unit.x * 3 + unit.y);
            _tile.familiarOccupant = null;
            enemyTeam.Remove(unit);

            for (int i = 0; i < enemyTeam.Count; i++)
            {
                enemyTeam[i].teamPosition = i;
            }

            for (int i = 0; i < 3; i++)
            {
                if (i < enemyTeam.Count)
                {
                    enemyTeam[i].Hud = enemyHUDs[i];
                    enemyTeam[i].Hud.SetData(enemyTeam[i].Familiar);
                    enemyTeam[i].Hud.Active(true);
                }
                else
                {
                    enemyHUDs[i].Active(false);
                }
            }

            yield return new WaitForSeconds(0.75f);
            Destroy(unit.gameObject);
            CheckEndBattle();
        }
        else
        {
            // Unsuccessful Catching
            yield return dialogMenu.TypeDialog("... but it refused!");

            unit.PlayCatchFailedAnimation();
            yield return new WaitForSeconds(0.5f);
        }

        actions--;
        yield return new WaitForSeconds(1f);
        if (actions > 0)
        {
            ActionSelection();
        }
        else
        {
            EnemyAttack();
        }

    }

    IEnumerator PlayerSwitchFamiliar(CombatUnit unit, Familiar familiar)
    {
        yield return SwitchFamiliar(unit, familiar);

        actions--;
        yield return new WaitForSeconds(1f);
        if (actions > 0)
        {
            ActionSelection();
        }
        else
        {
            EnemyAttack();
        }
    }

    IEnumerator SwitchOnFaint(CombatUnit unit)
    {
        unit.PlayFaintReturn();
        if (unit.isPlayerUnit)
        {
            yield return SwitchFamiliar(unit, CurrentFamiliarsController.Instance.GetHealthyFamiliars(CurrentFamiliarsController.Instance.playerFamiliars).LastOrDefault());
        }
        else
        {
            yield return SwitchFamiliar(unit, CurrentFamiliarsController.Instance.GetHealthyFamiliars(CurrentFamiliarsController.Instance.enemyFamiliars).LastOrDefault());
        }
    }

    IEnumerator SwitchFamiliar(CombatUnit unit, Familiar newFamiliar)
    {
        yield return dialogMenu.TypeDialog($"Switching {unit.Familiar.Base.Name} and {newFamiliar.Base.Name}.");

        unit?.PlayReturnAnimation();
        yield return new WaitForSeconds(0.5f);

        List<Familiar> _newList = null;
        if (unit.isPlayerUnit)
        {
            _newList = CurrentFamiliarsController.Instance.playerFamiliars;
        }
        else
        {
            _newList = CurrentFamiliarsController.Instance.enemyFamiliars;
        }

        int tIndex = _newList.IndexOf(newFamiliar);
        _newList[_newList.IndexOf(unit.Familiar)] = newFamiliar;
        _newList[tIndex] = unit.Familiar;
        unit.Familiar = newFamiliar;
        unit.Setup();
        unit.PlayEntranceAnimation();
        unit.Hud.SetData(unit.Familiar);
        yield return new WaitForSeconds(0.5f);
    }

    #region Helper Functions
    void PlayNoise(AudioClip audio)
    {
        audioSource.clip = audio;
        audioSource.Play();
    }

    IEnumerator AdvanceAttackPreview()
    {
        yield return new WaitForSeconds(1.2f);
        if (combatState == CombatState.AttackSelection)
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
            CurrentFamiliarsController.Instance.playerFamiliars.ForEach(f => f.OnBattleOver());
            OnBattleOver(false);
        }
        if (enemyTeam.Count <= 0)
        {
            OnBattleOver(true);
        }
    }

    void DisplayFamiliars(bool display)
    {
        for (int i = 0; i < playerTeam.Count; i++)
        {
            if (playerTeam[i] != null)
            {
                playerTeam[i].Display(display);
            }
        }

        for (int i = 0; i < enemyTeam.Count; i++)
        {
            if (enemyTeam[i] != null)
            {
                enemyTeam[i].Display(display);
            }
        }
    }

    #region Find Unit
    CombatUnit FindUnit(Familiar fam)
    {
        return FindUnit(fam, playerField);
    }
    CombatUnit FindUnit(Familiar fam, Field field)
    {
        for (int i = 0; i < 9; i++)
        {
            if (field.GetTile(i).familiarOccupant?.Familiar == fam) return field.GetTile(i).familiarOccupant;
        }
        return null;
    }
    #endregion
    #endregion
}
