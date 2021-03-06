using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle }

public class GameControllerOverworld : MonoBehaviour
{
    public static GameControllerOverworld Instance;

    [SerializeField] CharacterController playerController;

    GameState state;
    Vector3 playerPosition;

    int worldScreen = 1;
    int battleScreen = 2;

    bool haveBattled;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;

        playerController.OnEncountered += StartBattle;
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

        SceneManager.LoadScene(battleScreen);

        //playerController.gameObject.SetActive(false);
        //battleSystem.gameObject.SetActive(true);

        //CombatHandler.Instance.StoreParties(playerParty, wildFamiliar);
    }

    void EndBattle(bool win)
    {
        // we're just basically gonna assume a victory for now since other wise would need you to go back to one of those stations
        state = GameState.FreeRoam;

        SceneManager.LoadScene(worldScreen);
        
    }

    public void SetCombat()
    {
        CombatHandler.Instance.OnBattleOver += EndBattle;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SceneManager.LoadScene(battleScreen);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsWorldScene(scene.buildIndex))
        {
            GameObject _p = GameObject.Find("Player");
            if (_p != null)
            {
                playerController.gameObject.transform.position = playerPosition;
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
