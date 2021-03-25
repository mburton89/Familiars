using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Music", menuName = "Familiar/Music")]
public class GameMusic : ScriptableObject
{
    public static GameMusic Instance;

    public AudioClip mapMusic;
    public AudioClip mapMusic2;

    void Awake()
    {
        Instance = this;
    }
}
