using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject creditsPage;

    public void PlayGame()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        GameController.Instance.StartGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowCredits(bool show)
    {
        creditsPage.SetActive(show);
    }
}
