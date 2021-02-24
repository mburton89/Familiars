using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle }

public class GameController : MonoBehaviour
{
    [SerializeField] CombatHandler battleSystem;
    //[SerializeField] Camera worldCamera;
    GameState state;
    
    // Scene Transitions
    int currentWorldScreen = 1;
    
    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        DontDestroyOnLoad(this.gameObject);
    }

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;

        SceneManager.LoadScene(currentWorldScreen);
    }
    
    // Update is called once per frame
    void Update()
    {

    }
}
