// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace CreativeSpore.RPGConversationEditor
{
    /// <summary>
    /// This component inherits from UnityEngine.UI.Text to add support for filled text to simulate a typewriting effect. 
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("RPG Conversation Editor/TypewriterText", 10)]
    public class TypewriterText : UnityEngine.UI.Text
    {
        public delegate void OnFilledTextDelagate();
        public OnFilledTextDelagate OnFilledTextEvent;

        /// <summary>
        /// If true, the text will be updated with unscaled time.
        /// </summary>
        public bool unscaledTime = true;

        /// <summary>
        /// The text fill amount percent with 0 for 0% and 1 for 100%
        /// </summary>
        public float fillAmount
        {
            get { return m_fillAmount; }
            set
            {
                float newValue = Mathf.Clamp01(value);
                if (newValue != m_fillAmount)
                {
                    m_fillAmount = newValue;
                    if ( m_fillAmount == 1f && OnFilledTextEvent != null)
                        OnFilledTextEvent.Invoke();
                    if (m_audioSource)
                    {
                        if (m_fillAmount >= 1f)
                            m_audioSource.Stop();
                        else if(!m_audioSource.isPlaying)
                            m_audioSource.Play();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the number of displayed characters according to the current fillAmount
        /// </summary>
        public int typedCharCount
        {
            get { return Mathf.FloorToInt(m_fillAmount * m_charCount); }
            set
            {
                fillAmount = value / m_charCount;
            }
        }

        /// <summary>
        /// The audioclip played while typing the text (while fillAmount != 1f)
        /// </summary>
        public AudioClip TypingSound { get { return m_typingSound; } set { m_typingSound = value; } }

        [SerializeField, Tooltip("The typing speed in characters per second.")]
        private float m_typingSpeed = 30f;
        [SerializeField, Range(0f, 1f), Tooltip("The text fill amount percent with 0 for 0% and 1 for 100%.")]
        private float m_fillAmount = 1f;
        [SerializeField, Tooltip("The audioclip played while typing the text(while fillAmount != 1f)")]
        private AudioClip m_typingSound;

        private int m_charCount;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            fillAmount = 1;
        }
#endif

        private AudioSource m_audioSource;
        protected override void Start()
        {
            base.Start();
            m_audioSource = GetComponent<AudioSource>();
            UpdateGeometry();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying)
            {
                fillAmount = 0f;
                if (m_typingSound && m_audioSource)
                {
                    m_audioSource.clip = m_typingSound;
                    m_audioSource.Play();
                }
            }
        }


        void Update()
        {
            if (Application.isPlaying)
            {
                if (m_charCount > 0 && fillAmount < 1f)
                {
                    fillAmount += (1f / m_charCount) * m_typingSpeed * (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                }
            }
            UpdateGeometry();
        }

        public void FillText()
        {
            fillAmount = 1f;
        }

        private static List<UIVertex> s_vbo = new List<UIVertex>();
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            s_vbo.Clear();
            UIVertex uiVertex = new UIVertex();
            for (int i = 0; i < toFill.currentVertCount; ++i)
            {
                toFill.PopulateUIVertex(ref uiVertex, i);
                s_vbo.Add(uiVertex);
            }
            _OnFillVBO(s_vbo);
            for (int i = 0; i < toFill.currentVertCount; ++i)
            {
                toFill.SetUIVertex(s_vbo[i], i);
            }
        }

        bool _OnFillVBO(List<UIVertex> vbo)
        {
            if (font == null)
                return false;

            int charCount = 0;
            for (int i = 0; i < vbo.Count; ++i)
            {
                //The extra check if for special character when using richtext tags
                if (i % 4 == 0 && i < vbo.Count - 4 && vbo[i + 4].position.x != vbo[i].position.x)
                    ++charCount;
                UIVertex uiv = vbo[i];
                // typewriter effect
                if (charCount > typedCharCount && fillAmount < 1f)
                    uiv.color = new Color(0f, 0f, 0f, 0f);
                vbo[i] = uiv;
            }
            bool textHasChanged = m_charCount != charCount;
            m_charCount = charCount;
            return textHasChanged;
        }               
    }
}
