using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle }

public class GameControllerOverworld : MonoBehaviour
{
    public static GameControllerOverworld Instance;

    [SerializeField] GameObject playerPrefab;

    GameObject player;

    CharacterController playerController;

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

        //playerController = GameObject.Find("Player").GetComponent<CharacterController>();
        SceneManager.sceneLoaded += OnSceneLoaded;

        //playerController.OnEncountered += StartBattle;
    }

    void StartBattle()
    {
        state = GameState.Battle;
        // battleSystem.gameObject.SetActive(true);
        // worldCamera.gameObject.SetActive(false);

        var playerParty = PlayerParty.Instance;
        var wildFamiliars = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildFamiliars();
        CurrentFamiliarsController.Instance.UpdateEnemyFamiliars(wildFamiliars);
        playerPosition = playerController.gameObject.transform.position;

        Debug.Log("[GameController] start battle position: " + playerPosition);
        Debug.Log("[GameController] start battle position of player: " + playerController.gameObject.transform.position);
        SceneManager.LoadScene(battleScreen);

        player.SetActive(false);
        //playerController.gameObject.SetActive(false);
        //battleSystem.gameObject.SetActive(true);

        //CombatHandler.Instance.StoreParties(playerParty, wildFamiliar);
    }

    void EndBattle(bool win)
    {
        // we're just basically gonna assume a victory for now since other wise would need you to go back to one of those stations
        state = GameState.FreeRoam;

        SceneManager.LoadScene(worldScreen);
        player.SetActive(true);
        playerController.SetEncounterCooldown();
        
        
    }

    public void SetCombat()
    {
        CombatHandler.Instance.OnBattleOver += EndBattle;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("[GameController] Starting game...");
            SceneManager.LoadScene(worldScreen);
            player = Instantiate(playerPrefab, new Vector3(0, 0, 1), Quaternion.identity);
            GameObject _p = GameObject.Find("Player");
            if (_p != null)
            {
                Destroy(_p);
            }
            player.name = "Player";
            playerController = player.GetComponent<CharacterController>();
            playerController.OnEncountered += StartBattle;
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

            Debug.Log("[GameController] end battle playerPosition: " + playerPosition);
            Debug.Log("[GameController] end battle position of player: " + _p.transform.position);
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
