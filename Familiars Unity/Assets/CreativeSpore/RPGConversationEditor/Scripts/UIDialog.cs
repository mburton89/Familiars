// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace CreativeSpore.RPGConversationEditor
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RPG Conversation Editor/UIDialog", 10)]
    public class UIDialog : MonoBehaviour
    {
        [Tooltip("The Text component used to display the dialog name.")]
        public Text dialogName;
        [Tooltip("The Text component used to display the dialog text.")]
        public Text text;
        [Tooltip("The Text component used to display the dialog portrait.")]
        public Image portrait;
        [Tooltip("The parent gameObject used to organize the dialog actions.")]
        public GameObject actionList;
        [Tooltip("When set to true, the dialog will be updated with unscaled realtime.")]
        public bool unscaledTime = true;
        [Tooltip("If true, Time.timeScale will be set to 0 while the dialog is enabled.")]
        public bool pauseGame = false;
        [Tooltip("If true, the first action in the dialog will be selected. Useful when using keyboard controller.")]
        public bool autoSelectFirstAction = false;
        [Tooltip("If it should waits until the text is filled, when using a Typewriter text component, before displaying the dialog actions.")]
        public bool displayActionsAfterTextIsFilled = false;
        [Tooltip("If this property is defined, this gameObject will be displayed in multi-sentence dialog text to indicate that dialog text is not finished.")]
        public GameObject continueMark;
        [Tooltip("If the value is greather than 0, the dialog will continue to the next text automatically or close the dialog when the time is over.")]
        public float timeToContinueText = 0f;
        [Tooltip("If this property is defined, the gameObject will be placed over the selected dialog action.")]
        public RectTransform selectionMark;
        [Header("Events")]
        [Tooltip("This unity event will be invoked when a dialog action is selected.")]
        public UnityEvent onSelectActionEvent;
        [Tooltip("This unity event will be invoked when the UIDialog is enabled.")]
        public UnityEvent onEnableEvent;
        [Tooltip("This unity event will be invoked when the UIDialog is disabled.")]
        public UnityEvent onDisableEvent;

        public static Dialog.DialogAction processedDialogAction = new Dialog.DialogAction();

        public int CurrentDialogId { get { return m_currentDialog != null ? m_currentDialog.id : -1; } }
        public ConversationData CurrentConversation { get { return m_conversation; } }
        public Dialog CurrentDialog { get { return m_currentDialog; } }
        public bool CurrentDialogHasActions { get { return m_currentDialog != null ? m_currentDialog.dialogActions.Count > 0 : false; } }

        /// <summary>
        /// Returns true if the dialog is active or it was active during the current frame
        /// </summary>
        public bool IsPlaying { get { return isActiveAndEnabled || Time.frameCount == m_stopPlayingFrame; } }

        protected ConversationData m_conversation;
        protected Dialog m_currentDialog;
        protected bool m_isClosing = false;
        protected Queue m_sentenceQueue = new Queue();
        protected UnityAction m_onCloseCallback;
        protected GameObject m_selectActionOnNextUpdate = null; //fix SetupDialog called from OnEnable where EventSystem.current could be null
        protected float m_timerToContinue = -1f;
        protected TypewriterText m_typeWriterText;
        protected int m_stopPlayingFrame;
        private float m_savedTimeScale;

        #region UNITY MESSAGES
        private void OnValidate()
        {
            ReflectionUtils.AutoFillComponentFields(this);
        }

        private void Reset()
        {
            ReflectionUtils.AutoFillComponentFields(this);
        }        

        virtual protected void Start()
        {
        }

        virtual protected void OnEnable()
        {
            m_typeWriterText = text as TypewriterText;
            m_timerToContinue = timeToContinueText;
            if (m_typeWriterText)
            {
                m_typeWriterText.OnFilledTextEvent += OnTypeWriterFilledText;
                m_typeWriterText.unscaledTime = unscaledTime;
            }
            if(onEnableEvent != null)
                onEnableEvent.Invoke();
        }

        virtual protected void OnDisable()
        {
            if (m_typeWriterText)
                m_typeWriterText.OnFilledTextEvent -= OnTypeWriterFilledText;
            if (onDisableEvent != null)
                onDisableEvent.Invoke();
        }

        void OnTypeWriterFilledText()
        {
            if(m_sentenceQueue.Count == 0) //Visible only in the last sentence
            {
                actionList.SetActive(CurrentDialogHasActions);
                if (displayActionsAfterTextIsFilled && CurrentDialogHasActions)
                    m_selectActionOnNextUpdate = actionList.transform.GetChild(0).gameObject;
            }
        }

        virtual protected void Update()
        {
            float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if(!EventSystem.current)
            {
                UIUtils.InitializeEventSystem();
            }
            if (EventSystem.current)
            {
                if(m_selectActionOnNextUpdate)
                {
                    if(actionList)
                        LayoutRebuilder.ForceRebuildLayoutImmediate(actionList.GetComponent<RectTransform>()); // fix position of selectionMark
                    EventSystem.current.SetSelectedGameObject(m_selectActionOnNextUpdate);                    
                    m_selectActionOnNextUpdate = null;
                }
                GameObject selectedGo = EventSystem.current.currentSelectedGameObject;
                if (!selectedGo && Input.anyKeyDown)
                {
                    SelectFirstAvailableAction();
                }
            }            

            if (continueMark)
            {
                bool isFullTextDisplayed = !m_typeWriterText || m_typeWriterText.fillAmount == 1f;
                float time = Time.unscaledTime * 2f;
                continueMark.SetActive(isFullTextDisplayed && !actionList.activeSelf && (time - (int)time) < 0.7f);
            }

            if(m_timerToContinue > 0f)
            {
                if(!m_typeWriterText || m_typeWriterText.fillAmount == 1f)
                    m_timerToContinue -= deltaTime;
                if(m_timerToContinue <= 0f)
                {
                    DoContinueTextEvent();
                }
            }
            else
            {
                bool isFullTextDisplayed = !m_typeWriterText || m_typeWriterText.fillAmount == 1f;
                if (isFullTextDisplayed)
                    m_timerToContinue = timeToContinueText;
            }
        }
        #endregion

        /// <summary>
        /// Play a conversation. If dialogId is -1, it will start with the start dialog.
        /// </summary>
        /// <param name="conversation">The played conversation.</param>
        /// <param name="dialogId">The dialog to start with.</param>
        public void PlayConversation(ConversationData conversation, int dialogId = -1)
        {
            PlayConversation(conversation, null, dialogId);
        }
                
        /// Play a conversation. If dialogId is -1, it will start with the start dialog.
        /// </summary>
        /// <param name="conversation">The played conversation.</param>
        /// <param name="onCloseCallback">The callback called when the conversation is ended or the dialog closed.</param>
        /// <param name="dialogId">The dialog to start with.</param>
        public void PlayConversation(ConversationData conversation, UnityAction onCloseCallback, int dialogId = -1)
        {
            Dialog dialog = dialogId < 0? conversation.StartDialog : conversation.FindDialogById(dialogId);
            if (dialog != null)
            {
                DoOnStartConversation(conversation);
                m_onCloseCallback = onCloseCallback;
                PlayDialog(dialog);
            }
            else
            {
                Debug.LogWarning("Dialog id " + dialogId + " not found in conversation " + conversation.name);
            }
        }

        /// <summary>
        /// Stops the current played conversation.
        /// </summary>
        public void StopConversation()
        {
            DoOnCloseConversation();
        }

        /// <summary>
        /// Selects the first available dialog action if any.
        /// </summary>
        public void SelectFirstAvailableAction()
        {
            if (actionList)
            {
                for (int i = 0; i < actionList.transform.childCount; ++i)
                {
                    GameObject obj = actionList.transform.GetChild(i).gameObject;
                    if (obj.activeSelf && EventSystem.current)
                    {
                        EventSystem.current.SetSelectedGameObject(obj);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Advances the conversation to the next sentence or dialog. Usually called by UI events.
        /// </summary>
        public void DoContinueTextEvent()
        {
            if (m_typeWriterText && m_typeWriterText.fillAmount < 1f)
                m_typeWriterText.fillAmount = 1f;
            else
            {
                if (m_sentenceQueue.Count > 0)
                {
                    text.text = m_sentenceQueue.Dequeue() as string;
                    if (m_sentenceQueue.Count == 0)
                    {
                        actionList.SetActive(CurrentDialogHasActions && !displayActionsAfterTextIsFilled); //Visible only in the last sentence
                        if (autoSelectFirstAction)
                        {
                            SelectFirstAvailableAction();
                        }
                    }
                    if(m_typeWriterText)
                        m_typeWriterText.fillAmount = 0f;
                }
                else if(m_currentDialog != null)
                {
                    if (m_currentDialog.dialogActions.Count == 0)
                        DoOnCloseConversation();
                    else if (string.IsNullOrEmpty(m_currentDialog.dialogActions[0].name))
                    {
                        DoAction(0);
                    }
                }
            }
        }

        /// <summary>
        /// Executes the dialog action. This method should be called by a trigger event placed in the action gameObjects children of actionList.
        /// </summary>
        public void DoActionTriggerEvent()
        {
            if (m_typeWriterText && m_typeWriterText.fillAmount < 1f)
                m_typeWriterText.fillAmount = 1f;
            else if(EventSystem.current.currentSelectedGameObject)
            {
                int actionIdx = EventSystem.current.currentSelectedGameObject.transform.GetSiblingIndex();
                //Debug.Log("DoActionTriggerEvent called by " + EventSystem.current.currentSelectedGameObject.name + " idx " + actionIdx);
                DoAction(actionIdx);
            }
            else
            {
                DoOnCloseConversation();
            }
        }

        /// <summary>
        /// This method should be called when a dialog action is selected.
        /// </summary>
        public void DoActionSelectedEvent()
        {
            if (selectionMark)
            {
                GameObject selectedGo = EventSystem.current.currentSelectedGameObject;
                RectTransform selectedRect = selectedGo ? selectedGo.GetComponent<RectTransform>() : null;
                selectionMark.gameObject.SetActive(
                    selectedRect && selectedRect.gameObject.activeSelf
                    && selectedRect.transform.IsChildOf(actionList.transform));
                if (selectionMark.gameObject.activeSelf)
                {
                    Vector3 pos = selectionMark.position;
                    pos.x = selectedRect.offsetMin.x;
                    pos.y = (selectedRect.offsetMin.y + selectedRect.offsetMax.y) / 2f;
                    selectionMark.position = selectedRect.position;
                }
            }
            if(onSelectActionEvent!=null)
            {
                onSelectActionEvent.Invoke();
            }
        }

        /// <summary>
        /// Performs a dialog action.
        /// </summary>
        /// <param name="actionIdx"></param>
        private void DoAction(int actionIdx)
        {
            EventSystem.current.SetSelectedGameObject(null);
            if (m_conversation != null && m_currentDialog != null && m_currentDialog.dialogActions.Count > actionIdx)
            {
                Dialog.DialogAction dialogAction = m_currentDialog.dialogActions[actionIdx];
                Dialog nextDialog = m_conversation.FindDialogById(dialogAction.targetDialogId);
                if (dialogAction.onSubmit != null)
                    dialogAction.onSubmit.Invoke();
                PlayDialog(nextDialog);
            }
            else
            {
                DoOnCloseConversation();
            }
        }

        /// <summary>
        /// Skip the dialog to the last sentence
        /// </summary>
        public void SkipToTheLastSentence()
        {
            while (m_sentenceQueue.Count > 0)
                DoContinueTextEvent();
            DoContinueTextEvent(); //this extra call is to fill the typewriter text
        }
        
        /// <summary>
        /// Setups the dialog ui elements filling them with the dialog data.
        /// </summary>
        /// <param name="dialog"></param>
        public void SetupDialog(Dialog dialog)
        {
            m_sentenceQueue.Clear();
            for (int i = 0; i < dialog.sentences.Length; ++i)
                m_sentenceQueue.Enqueue(dialog.sentences[i]);
            string sentenceText = m_sentenceQueue.Count > 0? m_sentenceQueue.Dequeue() as string : string.Empty;
            if (text)
            {
                text.text = sentenceText;
                // needed for Typewriter Text
                text.enabled = false;
                text.enabled = true;
            }
            if (portrait)
            {
                portrait.sprite = dialog.portrait;
                portrait.gameObject.SetActive(portrait.sprite);
            }
            if (dialogName)
                dialogName.text = dialog.name;
            if (selectionMark)
                selectionMark.gameObject.SetActive(false);
            GameObject uiSelectedObj = null;
            if (actionList)
            {
                actionList.SetActive(true);
                UIUtils.ResizeGridItems(actionList, dialog.dialogActions.Count);
                for (int i = 0; i < dialog.dialogActions.Count; ++i)
                {
                    Text textComp = actionList.transform.GetChild(i).GetComponent<Text>();
                    if (!textComp)
                    {
                        Debug.LogWarning("No text component found in action item for action name " + dialog.dialogActions[i].name);
                        continue;
                    }
                    dialog.dialogActions[i].CloneNonAlloc(processedDialogAction);
                    if (processedDialogAction.onPreProcess != null)
                        processedDialogAction.onPreProcess.Invoke();
                    // If action name is null, it is hidden by default. It is used to set the next dialog when clicking the text
                    if (string.IsNullOrEmpty(processedDialogAction.name))
                    {
                        textComp.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (!uiSelectedObj)
                            uiSelectedObj = textComp.gameObject;
                        textComp.name = processedDialogAction.name;
                        textComp.text = processedDialogAction.name;

                        if (processedDialogAction.hidden)
                            textComp.gameObject.SetActive(false);
                    }
                }
                actionList.SetActive(CurrentDialogHasActions && m_sentenceQueue.Count == 0 && !displayActionsAfterTextIsFilled); //Visible only in the last sentence
            }

            if (!uiSelectedObj || !actionList || !actionList.activeSelf)
            {
                uiSelectedObj = text ? text.gameObject : null;
            }

            if (autoSelectFirstAction)
            {
                m_selectActionOnNextUpdate = uiSelectedObj;
            }
        }

        /// <summary>
        /// Does all the stuff related with the start of a conversation.
        /// </summary>
        /// <param name="conversation"></param>
        virtual protected void DoOnStartConversation(ConversationData conversation)
        {
            if( m_conversation != null && gameObject.activeSelf)
            {
                InvokeConversationEndEvents();
            }
            m_conversation = conversation;
            m_isClosing = false;
            m_currentDialog = null;
            gameObject.SetActive(true);
            if(pauseGame)
            {
                m_savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            if (m_conversation.events.onConversationStart != null)
                m_conversation.events.onConversationStart.Invoke();

        }

        /// <summary>
        /// Does all the stuff related with the end of a conversation.
        /// </summary>
        virtual protected void DoOnCloseConversation()
        {
            m_isClosing = true;
            gameObject.SetActive(false);
            m_stopPlayingFrame = Time.frameCount;
            InvokeConversationEndEvents();
            if (pauseGame)
            {
                Time.timeScale = m_savedTimeScale;
            }
        }

        private void InvokeConversationEndEvents()
        {
            if (m_currentDialog != null)
                m_currentDialog.onExit.Invoke();
            if (m_conversation.events.onConversationEnd != null)
                m_conversation.events.onConversationEnd.Invoke();
            if (m_onCloseCallback != null)
            {
                m_onCloseCallback.Invoke();
                m_onCloseCallback = null;
            }
        }

        private void PlayDialog(Dialog dialog)
        {
            Dialog prevDialog = m_currentDialog;                     
            m_currentDialog = dialog;
            if (prevDialog != null && prevDialog != dialog)
                prevDialog.onExit.Invoke();
            if (!m_isClosing) // in case onExit call CloseConversation
            {
                if (dialog != null)
                {
                    dialog.onEnter.Invoke();
                    SetupDialog(dialog);
                }
                else
                {
                    DoOnCloseConversation();
                }
            }
        }
    }
}
