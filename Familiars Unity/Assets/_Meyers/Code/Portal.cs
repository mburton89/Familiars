using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    CharacterController player;
    public void OnPlayerTriggered(CharacterController player)
    {
        this.player = player;
        Debug.Log("[Portal.cs/OnPlayerTriggered()]");
        StartCoroutine(SwitchScene());
    }

    Fader fader;
    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("[Portal.cs/SwitchScene()] Start of Switch Scene");

        //GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);
        Debug.Log("1");
       yield return SceneManager.LoadSceneAsync(sceneToLoad);
        Debug.Log("2");
        Debug.Log("[Portal.cs/SwitchScene()] before destPortal");
        var destPortal = FindObjectsOfType<Portal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        Debug.Log("[Portal.cs/SwitchScene()] before setPosition");
        player.SetPosition(destPortal.SpawnPoint.position);
        Debug.Log("[Portal.cs/SwitchScene()] after setPosition");
        if (destPortal != null)
        {
            Debug.Log(destPortal.SpawnPoint.position);
        }
        else
        {
            Debug.Log("very much not pog");
        }
        Debug.Log("[Portal.cs/SwitchScene()] before fadeout");
        yield return fader.FadeOut(0.5f);
        //GameController.Instance.PauseGame(false);


        Debug.Log("[Portal.cs/SwitchScene()] End of Switch Scene");
        Destroy(this.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.tag == "Player")
        {
            Debug.Log("if we got here");
            OnPlayerTriggered(other.gameObject.GetComponent<CharacterController>());
        }

    }

    public Transform SpawnPoint => spawnPoint;
}

public enum DestinationIdentifier { A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z }
