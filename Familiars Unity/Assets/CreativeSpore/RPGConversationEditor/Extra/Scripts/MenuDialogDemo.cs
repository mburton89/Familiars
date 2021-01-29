// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace CreativeSpore
{
	public class MenuDialogDemo : MonoBehaviour 
	{
        public Text FloatingText;

        private Vector3 m_startTextPosition;
        void Start () 
		{
            m_startTextPosition = FloatingText.rectTransform.position;
            FloatingText.color = default(Color);
        }
		
        public void ShowFloatingText(string text)
        {
            FloatingText.rectTransform.position = m_startTextPosition;
            FloatingText.text = text;
            FloatingText.color = Color.cyan;
            StopAllCoroutines();
            StartCoroutine(FloatingTextCO());
        }

        IEnumerator FloatingTextCO()
        {
            float a = 2f;
            while(a > 0f)
            {
                a -= Time.deltaTime * 2f;
                Color c = FloatingText.color;
                c.a = Mathf.Clamp01(a);
                FloatingText.color = c;
                FloatingText.rectTransform.localPosition += Vector3.up * Time.deltaTime * 50f;
                yield return null;
            }
        }
	}
}
