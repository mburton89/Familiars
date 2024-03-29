﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Music", menuName = "Familiar/Music")]
public class GameMusic : ScriptableObject
{
    public static GameMusic Instance;

    public AudioClip mapMusic;
    public AudioClip mapMusic2;
    public AudioClip mapMusic3;

    public AudioClip menuMusic1;
    public AudioClip menuMusic2;
    public AudioClip menuMusic3;

    public AudioClip angeredmcSFX;
    public AudioClip curiousmcSFX;
    public AudioClip hmmmcSFX;
    public AudioClip huhmcSFX;
    public AudioClip scaredmcSFX;

    public AudioClip attackmvSFX;
    public AudioClip evilchucklemvSFX;
    public AudioClip hmmmvSFX;
    public AudioClip injuredshockmvSFX;
    public AudioClip injuredmvSFX;
    public AudioClip shockmvSFX;
    public AudioClip sighmvSFX;
    public AudioClip smirkmvSFX;

    public AudioClip injuredgvSFX;
    public AudioClip injured2gvSFX;
    public AudioClip evilchucklegvSFX;

    public AudioClip attackfrSFX;
    public AudioClip laughfrSFX;
    public AudioClip shockfrSFX;
    public AudioClip hmmfrSFX;
    public AudioClip injuredfrSFX;

    public AudioClip buttonClickMetalSFX;
    public AudioClip buttonClick1SFX;
    public AudioClip buttonClick2SFX;
    public AudioClip buttonClick3SFX;
    public AudioClip menuNotificationSFX;
    public AudioClip transition1SFX;
    public AudioClip transitionSFX2;

    public AudioClip commonFight;
    public AudioClip commonFight2;

    public AudioClip rivaltwoTheme;
    public AudioClip rivaltwoTheme2;

    public AudioClip gameOverTheme;


    void Awake()
    {
        Instance = this;
    }
}
