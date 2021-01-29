// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace CreativeSpore.RPGConversationEditor
{
    /// <summary>
    /// Contains all data for a dialog box
    /// </summary>
    [System.Serializable]
    public class Dialog
    {
        public const string k_defaultText = "Enter Text...";
        public int id { get { return m_id; } }
        public Vector2 position { get { return rect.center; } set { rect.center = value; } }
        public float width { get { return rect.width; } }
        public float height { get { return rect.height; } }
        public Vector2 size { get { return rect.size; } }
        public Sprite portrait;
        public string name;
        public Vector2 textScrollPos = Vector2.zero;
        public Rect rect;
        public Color color;
        public UnityEvent onEnter = null;
        public UnityEvent onExit = null;
        public bool inheritName;

        public string[] sentences
        {
            get { if (m_sentences == null || m_sentences.Length == 0) SetSentence(0, ""); return m_sentences; }
        }

        [SerializeField, TextArea(1, 5)]
        private string[] m_sentences;

        [System.Serializable]
        public class DialogAction
        {
            public bool hidden;
            public string name;
            public int targetDialogId;
            public UnityEvent onSubmit;
            public UnityEvent onPreProcess;
            public bool visible { get { return !this.hidden; } set { this.hidden = !value; } }
            public DialogAction()
            {
                this.name = "new action";
                this.targetDialogId = -1;
                this.hidden = false;
                this.onSubmit = null;
                this.onPreProcess = null;
            }

            public override string ToString()
            {
                return "DialogAction " + name;
            }

            public void CloneNonAlloc(DialogAction dest)
            {
                dest.name = this.name;
                dest.targetDialogId = this.targetDialogId;
                dest.hidden = this.hidden;
                dest.onSubmit = this.onSubmit;
                dest.onPreProcess = this.onPreProcess;
            }
        }
        public List<DialogAction> dialogActions = new List<DialogAction>();

        public enum Type
        {
            TextDialog,
            Condition,
            Action
        }
        public Type type = Type.TextDialog;

        [SerializeField]
        private int m_id;

        public Dialog(int id) : this()
        {
            this.m_id = id;
            this.m_sentences = new string[] { k_defaultText };
        }

        public Dialog()
        {
            rect = new Rect(100, -250, 200, 100);
            color = (Color)new Color32(194, 194, 194, 255);
        }

        public string GetSentence(int index)
        {
            if (m_sentences != null && index < m_sentences.Length)
                return m_sentences[index];
            else
                return "";
        }

        public void SetSentence(int index, string value)
        {
            if (m_sentences == null || index >= m_sentences.Length)
                System.Array.Resize(ref m_sentences, index+1);
            m_sentences[index] = value;
        }
    }
}
