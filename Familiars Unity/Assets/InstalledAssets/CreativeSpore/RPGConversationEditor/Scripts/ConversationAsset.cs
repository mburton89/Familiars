// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace CreativeSpore.RPGConversationEditor
{
	public class ConversationAsset : ScriptableObject 
	{        
        public List<ConversationData> conversations = new List<ConversationData>();
	}

    [System.Serializable]
    public class ConversationData
    {
        public List<Dialog> dialogList = new List<Dialog>();
        public int startDialogId = -1;
        public string name = "";
        [System.Serializable]
        public class Events
        {
            [Tooltip("UnityEvent invoked when conversation is started")]
            public UnityEvent onConversationStart = null;
            [Tooltip("UnityEvent invoked when conversation is ended")]
            public UnityEvent onConversationEnd = null;
        }
        public Events events = new Events();

        protected int m_overriddenStartDialogId = -1;

        public ConversationData() : this("New Conversation"){}
        public ConversationData(string name)
        {
            this.m_idCnt = 0;
            AddDialog();
            this.startDialogId = 0;
            this.name = name;
        }

        public override string ToString()
        {
            return "[ConversationData] name: " + name + "; dialogCount: " + dialogList.Count;
        }

        public Dialog StartDialog
        {
            get { return FindDialogById(m_overriddenStartDialogId >= 0? m_overriddenStartDialogId : startDialogId); }
            set { startDialogId = value.id; }
        }

        public void OverrideStartDialog(int dialogId)
        {
            m_overriddenStartDialogId = dialogId;
        }

        public Dialog AddDialog()
        {
            int id = GetFreeId();
            Dialog dlg = new Dialog(id);
            dialogList.Add(dlg);
            return dlg;
        }

        public bool RemoveDialog(Dialog dialog)
        {
            return dialogList.Remove(dialog);
        }

        public bool RemoveDialog(int id)
        {
            return dialogList.RemoveAll(d => d.id == id) > 0;
        }

        public Dialog FindDialogById(int id)
        {
            return dialogList.Find(d => d.id == id);
        }

        private int[] m_randomIndexArray;
        private int m_randomIndexCounter;
        public Dialog GetRandomDialog(bool nonRepeating = true)
        {
            if (dialogList.Count == 0)
                return null;
            if (dialogList.Count == 1)
                return dialogList[0];
            if (nonRepeating)
            {
                if(m_randomIndexArray == null || m_randomIndexArray.Length != dialogList.Count)
                {
                    System.Array.Resize(ref m_randomIndexArray, dialogList.Count);
                    for (int i = 0; i < m_randomIndexArray.Length; ++i)
                        m_randomIndexArray[i] = i;
                    m_randomIndexCounter = m_randomIndexArray.Length;
                }

                int randIdx = Random.Range(0, m_randomIndexCounter);
                int randIdx2 = m_randomIndexArray[randIdx];
                //DebugRandomNonRepeating(randIdx);
                --m_randomIndexCounter;
                if (m_randomIndexCounter == 0)
                    m_randomIndexCounter = m_randomIndexArray.Length - 1; //NOTE: this is set to Length - 1 to discard the current selected item
                m_randomIndexArray[randIdx] = m_randomIndexArray[m_randomIndexCounter];
                m_randomIndexArray[m_randomIndexCounter] = randIdx2;
                return dialogList[randIdx2];
            }
            else
            {
                return dialogList[Random.Range(0, dialogList.Count)];
            }
        }

        private void DebugRandomNonRepeating(int selected)
        {
            string sDebug = "|";
            for(int i = 0; i < m_randomIndexArray.Length; ++i)
            {
                string sNum = m_randomIndexArray[i].ToString();
                if (i == selected)
                    sNum = "<color=blue>" + sNum + "</color>";
                if( i >= m_randomIndexCounter)
                    sNum = "<color=red>" + sNum + "</color>";
                sDebug += sNum + "|";
            }
            Debug.Log(sDebug + " ----> " + m_randomIndexArray[selected]);
        }

        private int m_idCnt = 0;
        private int GetFreeId()
        {
            while (FindDialogById(m_idCnt) != null) ++m_idCnt;
            return m_idCnt;
        }
    }
}
