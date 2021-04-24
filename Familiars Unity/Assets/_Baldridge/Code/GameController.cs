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
    int currentNPC = -1;

    GameState state;
    Vector3 playerPosition;

    int worldScreen = 1;
    int battleScreen = 2;

    public bool versusTrainer;

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

    public void StartTrainerBattle(List<Familiar> trainerFamiliars, TrainerController trainer)
    {
        state = GameState.Battle;

        var playerParty = PlayerParty.Instance;
        CurrentFamiliarsController.Instance.UpdateEnemyFamiliars(trainerFamiliars);

        playerPosition = playerController.gameObject.transform.position;
        currentNPC = trainer.GetTrainerID();
        versusTrainer = true;
        SceneManager.LoadSceneAsync(battleScreen);
    }

    void EndBattle(bool win)
    {
        // we're just basically gonna assume a victory for now since other wise would need you to go back to one of those stations
        state = GameState.FreeRoam;

        Debug.Log("[GameController.cs/EndBattle():1] currentNPC = " + currentNPC);
        Debug.Log("[GameController.cs] " + win);
        if (win)
        {
            Debug.Log("[GameController.cs/EndBattle():2] currentNPC = " + currentNPC);
            if (currentNPC != -1)
            {
                Debug.Log("[GameController.cs] currentNPC not null, setting TrainerFlag");
                FlagManager.Instance.SetTrainerFlag(currentNPC, true);
                currentNPC = -1;
                versusTrainer = false;
            }
        }
        Debug.Log("[GameController.cs/EndBattle():3] currentNPC = " + currentNPC);

        SceneManager.LoadSceneAsync(worldScreen);

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
        player.GetComponent<SpriteRenderer>().enabled = false;
        playerController.cm.enabled = false;
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
                _p.GetComponent<SpriteRenderer>().enabled = true;
                _p.GetComponent<CharacterController>().cm.enabled = true;
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
