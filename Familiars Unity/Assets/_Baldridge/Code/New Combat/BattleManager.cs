using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BattleStates { Begin, PlayerSelection, PlayerMove, PlayerAction, PlayerTargeting, EnemyTurn, Busy}
public class BattleManager : MonoBehaviour
{
    [SerializeField] GameObject nodePrefab;
    [SerializeField] Transform origin;
    [SerializeField] Selector selector;
    public int boardHeight;
    public int boardWidth;

    public Unit[] playerTeam;
    public Unit[] enemyTeam;

    [HideInInspector] public Node[,] board;

    private int selectorX;
    private int selectorY;

    private int currentAttack;
    private bool newAttack = false;

    private int previousNodeX;
    private int previousNodeY;

    private Unit selectedUnit;

    private BattleStates state = BattleStates.Begin;

    // UI Stuff
    [SerializeField] GameObject familiarDetails;
    [SerializeField] BattleDialogBox dialogBox;


    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        GameObject _newNode;
        Vector3 _newPosition;
        board = new Node[boardWidth, boardHeight];
        Node _node;

        state = BattleStates.Begin;
        for (int _x = 0; _x < boardWidth; _x++)
        {
            for (int _y = 0; _y < boardHeight; _y++)
            {
                _newPosition = new Vector3(origin.position.x + (1.5f * _x), origin.position.y + (1.5f * _y), -1);
                _newNode = Instantiate(nodePrefab, _newPosition, Quaternion.identity);

                _node = _newNode.GetComponent<Node>();
                _node.position = new Vector2((float)_x, (float)_y);

                board[_x, _y] = _node;
            }
        }

        // Assign neighbors
        for (int _xx = 0; _xx < boardWidth; _xx++)
        {
            for (int _yy = 0; _yy < boardHeight; _yy++)
            {
                board[_xx, _yy].GatherNeighbors();
            }
        }

        // Place Units
        Vector3 _newPos;
        foreach (Unit _unit in playerTeam)
        {
            board[_unit.x, _unit.y].occupant = _unit;
            _newPos = board[_unit.x, _unit.y].gameObject.transform.position;
            _unit.transform.position = new Vector3(_newPos.x, _newPos.y, -2);
            _unit.Setup();
        }

        foreach (Unit _unit in enemyTeam)
        {
            board[_unit.x, _unit.y].occupant = _unit;
            _newPos = board[_unit.x, _unit.y].gameObject.transform.position;
            _unit.transform.position = new Vector3(_newPos.x, _newPos.y, -2);
            _unit.Setup();
        }


        PlayerSelection();

    }

    #region State Initializers
    private void PlayerSelection()
    {
        state = BattleStates.PlayerSelection;

        selectedUnit = null;
        currentAttack = 0;

        selector.gameObject.SetActive(true);
        WipeNodes();

        dialogBox.EnableAttackSelector(false);
        dialogBox.EnableDialogText(false);

        bool finishTurn = true;
        foreach (Unit _u in playerTeam)
        {
            if (!_u.turnComplete) finishTurn = false;
        }

        if (finishTurn)
        {
            EnemyTurn();
        }
    }

    private void PlayerMove()
    {
        state = BattleStates.PlayerMove;
        
        selectedUnit.GetCurrentNode();
        selectedUnit.FindSelectableNodes(NodeState.Move, selectedUnit.Familiar.Base.Movement);

        familiarDetails.SetActive(true);
        dialogBox.EnableAttackSelector(false);
        //selector.gameObject.SetActive(false);
    }

    private void PlayerAction()
    {
        state = BattleStates.PlayerAction;

        newAttack = true;
        //Start spawning UI elements
        familiarDetails.SetActive(false);
        dialogBox.EnableAttackSelector(true);
        dialogBox.SetAttackNames(selectedUnit.Familiar.Attacks);
    }

    private void PlayerTargeting()
    {
        state = BattleStates.PlayerTargeting;
    }

    private void EnemyTurn()
    {
        state = BattleStates.EnemyTurn;

        dialogBox.EnableDialogText(true);
        StartCoroutine(dialogBox.TypeDialog("It is now the enemy's turn"));
    }
    #endregion

    private void Update()
    {
        switch (state)
        {
            case BattleStates.PlayerSelection:
                HandlePlayerSelection();
                break;
            case BattleStates.PlayerMove:
                HandlePlayerMove();
                break;
            case BattleStates.PlayerAction:
                HandlePlayerAction();
                break;
            case BattleStates.PlayerTargeting:
                HandlePlayerTargeting();
                break;
            case BattleStates.EnemyTurn:
                HandleEnemyTurn();
                break;
        }
    }

    #region State Handler
    private void HandlePlayerSelection()
    {
        NavigateGrid();
        
        Unit _unit = board[selectorX, selectorY].occupant;

        if (_unit != null)
        {
            familiarDetails.SetActive(true);
            familiarDetails.GetComponent<BattleHUD>().SetData(_unit.Familiar);

            if (Input.GetKeyDown(KeyCode.Z))
            {
            
                if (_unit.isPlayerUnit)
                {
                    if (!_unit.turnComplete)
                    {
                        selectedUnit = _unit;
                        PlayerMove();
                    }
                }
            }
        }
        else
        {
            familiarDetails.SetActive(false);
        }
    }

    private void HandlePlayerMove()
    {
        NavigateGrid();
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Node _node = board[selectorX, selectorY];
            if (_node.state == NodeState.Move)
            {
                Vector2 _newPos = new Vector2(_node.transform.position.x, _node.transform.position.y);

                selectedUnit.GetCurrentNode().occupant = null;
                _node.occupant = selectedUnit;
                selectedUnit.previousNode = board[selectorX, selectorY];
                selectedUnit.SetCurrentNode(_node);

                selectedUnit.x = selectorX;
                selectedUnit.y = selectorY;
                
                previousNodeX = selectorX;
                previousNodeY = selectorY;

                selectedUnit.hasMoved = true;
                selectedUnit.gameObject.transform.position = new Vector3(_newPos.x, _newPos.y, selectedUnit.gameObject.transform.position.z);

                WipeNodes();

                PlayerAction();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayerSelection();
        }
        //Node _n = board[selectedUnit.x, selectedUnit.y];
    }

    private void HandlePlayerAction()
    {
        #region Menu Control
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAttack < selectedUnit.Familiar.Attacks.Count - 1)
            {
                currentAttack++;
                newAttack = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAttack > 0)
            {
                currentAttack--;
                newAttack = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAttack < selectedUnit.Familiar.Attacks.Count - 2)
            {
                currentAttack += 2;
                newAttack = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAttack > 1)
            {
                currentAttack -= 2;
                newAttack = true;
            }

        }
        #endregion

        dialogBox.UpdateAttackSelection(currentAttack, selectedUnit.Familiar.Attacks[currentAttack]);
        
        if (newAttack)
        {
            WipeNodes();

            switch (selectedUnit.Familiar.Attacks[currentAttack].Base.AttackStyle)
            {
                case AttackStyle.Target:
                    selectedUnit.FindSelectableNodes(NodeState.Target, selectedUnit.Familiar.Attacks[currentAttack].Base.Range, false);
                    break;
                case AttackStyle.Launch:
                    selectedUnit.FindSelectableNodes(NodeState.Target, selectedUnit.Familiar.Attacks[currentAttack].Base.Range, 1, false);
                    selectedUnit.FindSelectableNodes(NodeState.Target, selectedUnit.Familiar.Attacks[currentAttack].Base.Range, 2, false);
                    selectedUnit.FindSelectableNodes(NodeState.Target, selectedUnit.Familiar.Attacks[currentAttack].Base.Range, 3, false);
                    selectedUnit.FindSelectableNodes(NodeState.Target, selectedUnit.Familiar.Attacks[currentAttack].Base.Range, 4, false);
                    break;
            }

            newAttack = false;
        }
        
        if (Input.GetKeyDown(KeyCode.Z))
        {

            PlayerTargeting();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            Vector2 _newPos = new Vector2(board[previousNodeX, previousNodeY].transform.position.x, board[previousNodeX, previousNodeY].transform.position.y);

            selectedUnit.x = previousNodeX;
            selectedUnit.y = previousNodeY;

            selectedUnit.SetCurrentNode(selectedUnit.previousNode);

            selectedUnit.hasMoved = false;
            selectedUnit.gameObject.transform.position = new Vector3(_newPos.x, _newPos.y, selectedUnit.gameObject.transform.position.z);

            WipeNodes();

            PlayerMove();
            
        }
    }

    private void HandlePlayerTargeting()
    {
        NavigateGrid();


        Unit _unit = board[selectorX, selectorY].occupant;
        if (_unit != null)
        {
            // Make variable and check to see if the attack should target allies or enemies later
            if (!_unit.isPlayerUnit && board[selectorX, selectorY].state == NodeState.Target)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    _unit.Familiar.TakeDamage(DamageCalculator(selectedUnit, _unit, selectedUnit.Familiar.Attacks[currentAttack]));
                    selectedUnit.turnComplete = true;

                    PlayerSelection();
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            PlayerAction();
        }

    }

    private void HandleEnemyTurn()
    {
        Debug.Log("It is now the enemy turn.");
    }

    #endregion
    private int DamageCalculator(Unit user, Unit target, Attack attack)
    {
        float _modifierPhys = 1; // change on crit?
        float _modifierMag = 1; // type effectiveness


        float _physical = _modifierPhys * ((user.Familiar.Attack + attack.Base.Power) - target.Familiar.Defense);
        float _magical = _modifierMag * ((user.Familiar.SpAttack + attack.Base.Magic) - target.Familiar.SpDefense);

        float levelDifference = (((user.Familiar.Level - target.Familiar.Level) / 100) + 1);

        float total = levelDifference * (_physical + _magical);
        
        return Mathf.Max(1, (int) total);
    }

    private void NavigateGrid()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (selectorX > 0)
                selectorX--;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (selectorX < boardWidth - 1)
                selectorX++;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (selectorY < boardHeight - 1)
                selectorY++;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (selectorY > 0)
                selectorY--;
        }
        
        selector.SetPosition(board[selectorX, selectorY]);
    }
    
    private void WipeNodes()
    {
        foreach(Node n in board)
        {
            n.Reset();
        }
    }
}
