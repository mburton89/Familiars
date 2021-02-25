using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControllerOverworld : MonoBehaviour
{
    [SerializeField] CharacterController playerController;

    GameState state;
    Vector3 playerPosition;
    
    int battleScreen = 2;

    private void Start()
    {
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SceneManager.LoadScene(battleScreen);
        }
    }
}
