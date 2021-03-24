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
        SceneManager.sceneLoaded += OnSceneLoaded;
        
    }

    void StartBattle()
    {
        state = GameState.Battle;

        var playerParty = PlayerParty.Instance;
        var wildFamiliars = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildFamiliars();
        CurrentFamiliarsController.Instance.UpdateEnemyFamiliars(wildFamiliars);
        playerPosition = playerController.gameObject.transform.position;
        
        SceneManager.LoadScene(battleScreen);

        player.SetActive(false);
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
