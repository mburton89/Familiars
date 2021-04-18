using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle }

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [SerializeField] GameObject playerPrefab;

    GameObject player;

    CharacterController playerController;
    NPCController currentNPC;

    GameState state;
    Vector3 playerPosition;

    int worldScreen = 1;
    int battleScreen = 2;

    bool haveBattled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

        ConditionsDB.Init();
        
    }

    void StartBattle()
    {
        state = GameState.Battle;

        var playerParty = PlayerParty.Instance;
        var wildFamiliars = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildFamiliars();
        CurrentFamiliarsController.Instance.UpdateEnemyFamiliars(wildFamiliars);
        playerPosition = playerController.gameObject.transform.position;
        
        SceneManager.LoadSceneAsync(battleScreen);

        player.SetActive(false);
    }

    public void StartTrainerBattle(List<Familiar> trainerFamiliars, NPCController trainer)
    {
        state = GameState.Battle;

        var playerParty = PlayerParty.Instance;
        CurrentFamiliarsController.Instance.UpdateEnemyFamiliars(trainerFamiliars);

        playerPosition = playerController.gameObject.transform.position;
        currentNPC = trainer;
        
        SceneManager.LoadSceneAsync(battleScreen);
    }

    void EndBattle(bool win)
    {
        // we're just basically gonna assume a victory for now since other wise would need you to go back to one of those stations
        state = GameState.FreeRoam;

        SceneManager.LoadSceneAsync(worldScreen);

        if (win)
        {
            if (currentNPC != null)
            {
                currentNPC.completeBattle = true;
                currentNPC = null;
            }
        }

        player.SetActive(true);
        playerController.SetEncounterCooldown();
    }

    public void SetCombat()
    {
        CombatHandler.Instance.OnBattleOver += EndBattle;
    }

    public void StartGame()
    {
        SceneManager.LoadSceneAsync(worldScreen);
        player = Instantiate(playerPrefab, new Vector3(0, 0, 1), Quaternion.identity);
        GameObject _p = GameObject.Find("Player");
        if (_p != null)
        {
            Destroy(_p);
        }
        player.name = "Player";
        playerController = player.GetComponent<CharacterController>();
        player.transform.position = new Vector3(0, -2, 0);
        playerController.OnEncountered += StartBattle;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartGame();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsWorldScene(scene.buildIndex))
        {
            GameObject _p = GameObject.Find("Player");
            if (_p != null)
            {
                //_p.transform.position = new Vector3(10, 10, 1);
            }

        }
    }

    bool IsWorldScene(int scene)
    {
        if (scene == 1)
        {
            return true;
        }
        return false;
    }
}
