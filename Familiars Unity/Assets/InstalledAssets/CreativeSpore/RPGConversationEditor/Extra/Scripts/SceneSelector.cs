// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CreativeSpore
{
	public class SceneSelector : MonoBehaviour 
	{
        public Text SceneText;
        public CanvasGroup LevelIntroPanel;
        public float LevelIntroTime = 5f;
        private float m_timer;

        public void GoToNextScene()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % sceneCount);
        }

        public void GoToPrevScene()
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex - 1 + sceneCount) % sceneCount);
        }

        void Start () 
		{
            string sceneName = SceneManager.GetActiveScene().name;
            sceneName = sceneName.Substring(26, sceneName.Length - 26); // removes the "[RPG Conversation Editor] prefix"
            SceneText.text = sceneName;
            Time.timeScale = 0f;
            m_timer = LevelIntroTime;
            StartCoroutine(FadeIn(LevelIntroPanel));
        }
		
		void Update () 
		{
            if (m_timer > 0f)
            {
                m_timer -= Time.unscaledDeltaTime;
                if(m_timer <= 0f)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeOut(LevelIntroPanel));
                    return;
                }
            }
            if(Input.GetMouseButtonDown(0) && LevelIntroPanel.alpha == 1f)
            {
                StartCoroutine(FadeOut(LevelIntroPanel));
            }
		}

        IEnumerator FadeIn(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;
            LevelIntroPanel.gameObject.SetActive(true);
            while (canvasGroup.alpha < 1f)
            {
                yield return new WaitForEndOfFrame();
                canvasGroup.alpha += Time.unscaledDeltaTime;
            }
        }

        IEnumerator FadeOut(CanvasGroup canvasGroup)
        {
            while (canvasGroup.alpha > 0f)
            {
                yield return new WaitForEndOfFrame();
                canvasGroup.alpha -= Time.unscaledDeltaTime;
            }
            LevelIntroPanel.gameObject.SetActive(false);
            Time.timeScale = 1f;
        }

    }
}
