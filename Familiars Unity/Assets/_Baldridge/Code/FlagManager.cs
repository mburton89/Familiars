using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public static FlagManager Instance;

    [SerializeField] int storyCount;
    [SerializeField] int trainersCount;
    public List<bool> StoryFlags { get; private set; }
    public List<bool> TrainerFlags { get; private set; }

    private void Awake()
    {
        Instance = this;
        StoryFlags = new List<bool>();
        TrainerFlags = new List<bool>();
        for (int i = 0; i < storyCount; i++)
        {
            StoryFlags.Add(false);
        }

        for (int i = 0; i < trainersCount; i++)
        {
            TrainerFlags.Add(false);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    public void SetStoryFlag(int index, bool val = true)
    {
        Debug.Log("[FlagManager.cs] Story set");
        StoryFlags[index] = val;
    }

    public void SetTrainerFlag(int index, bool val = true)
    {
        Debug.Log("[FlagManager.cs] Trainer set");
        TrainerFlags[index] = val;
    }

}
