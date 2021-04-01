// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using System.ComponentModel;
using UnityEngine.Events;

namespace CreativeSpore.RPGConversationEditor
{
    /// <summary>
    /// Starts a conversation when a Unity message is received.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("RPG Conversation Editor/Conversation Trigger", 10)]
	public class ConversationTrigger : MonoBehaviour 
	{
        [Tooltip("The conversation controller that will be activated.")]
        public ConversationController conversationController;
        [Tooltip("Event invoked when the started conversation is closed.")]
        public UnityEvent onCloseEvent;

        [Tooltip("The time the dialog will be displayed.")]
        public float DialogTime = 1f;
        [Tooltip("The time to wait between dialogs.")]
        public float TimeBetweenDialogs = 2f;
        [Tooltip("The key that will start the conversation if Player is inside the collider and key is down.")]
        public KeyCode TriggerKeyCode = KeyCode.None;
        [Tooltip("The button that will start the conversation if Player is inside the collider and button is down.")]
        public string TriggerButtonName = "Fire1";
        [Tooltip("The tag of the gameObject that will be allowed to activate this trigger when it's colliding it. Leave it empty to allow any tag.")]
        public string TagFilter = "Player";

        private bool m_updateTimer = false;
        private bool m_updateMouseRaycast3D = false;
        private bool m_updateMouseRaycast2D = false;
        private float m_timer = 0f;
        private bool m_updateKeyDown = false;
        private bool m_updateButtonDown = false;
        private Transform m_playerInRange = null;

        [SerializeField]
        protected int[] m_events;

        [SerializeField]
        List<eEventType> m_activeEvents = new List<eEventType>(); 

        private void ValidateData()
        {
            int eventCounter = System.Enum.GetNames(typeof(eEventType)).Length;
            if (m_events == null || m_events.Length != eventCounter)
                System.Array.Resize(ref m_events, eventCounter);
            m_activeEvents = m_activeEvents.Distinct().ToList();
        }

        private void OnValidate()
        {
            ValidateData();
        }

        private void Reset()
        {
            conversationController = GetComponentInChildren<ConversationController>();
            ValidateData();
        }

        virtual protected void Start()
        {
            ExecuteEvent(eEventType.Start);
            m_updateTimer = m_activeEvents.Contains(eEventType.OnTimer);
            if(m_activeEvents.Contains(eEventType.OnClick))
            {
                m_updateMouseRaycast3D = GetComponent<Collider>();
                m_updateMouseRaycast2D = GetComponent<Collider2D>();
            }
            m_updateKeyDown = m_activeEvents.Contains( eEventType.OnKeyDown);
            m_updateButtonDown = m_activeEvents.Contains(eEventType.OnButtonDown);
        }

        virtual protected void Update()
        {
            if (m_updateTimer)
            {
                m_timer -= Time.deltaTime;
                if (m_timer <= 0)
                {
                    m_timer += DialogTime + TimeBetweenDialogs;
                    ExecuteEvent(eEventType.OnTimer);
                }
                if (conversationController 
                    && conversationController.IsPlayingDialog 
                    && m_timer <= TimeBetweenDialogs)
                {
                    conversationController.StopConversation();
                }
            }
            if((m_updateMouseRaycast3D || m_updateMouseRaycast2D) && Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo3D;
                RaycastHit2D hitInfo2D;
                if (m_updateMouseRaycast3D && Physics.Raycast(ray, out hitInfo3D) && hitInfo3D.transform == transform)
                    OnClick();
                if(m_updateMouseRaycast2D)
                {
                    hitInfo2D = Physics2D.Raycast(ray.origin, ray.direction);
                    if (hitInfo2D.transform == transform)
                        OnClick();
                }
            }
            
            if(m_updateKeyDown)
            {
                if(m_playerInRange && Input.GetKeyDown(TriggerKeyCode))
                {
                    ExecuteEvent( eEventType.OnKeyDown);
                }
            }
            if(m_updateButtonDown)
            {
                if(m_playerInRange && Input.GetButtonDown(TriggerButtonName))
                {
                    ExecuteEvent(eEventType.OnButtonDown);
                }
            }
        }     
        
        private bool CheckTagFilter(GameObject obj, string tag)
        {
            return string.IsNullOrEmpty(TagFilter) || obj.CompareTag(TagFilter);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CheckTagFilter(other.gameObject, TagFilter))
            {
                m_playerInRange = other.transform;
                DoOnTriggerEnter(other);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (CheckTagFilter(other.gameObject, TagFilter))
            {
                m_playerInRange = other.transform;
                DoOnTriggerEnter2D(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (CheckTagFilter(other.gameObject, TagFilter))
            {
                m_playerInRange = null;
                DoOnTriggerExit(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (CheckTagFilter(other.gameObject, TagFilter))
            {
                m_playerInRange = null;
                DoOnTriggerExit2D(other);
            }
        }

        public bool IsUsingEvent(eEventType eventType)
        {
            return m_activeEvents.Contains(eventType);
        }

        private void ExecuteEvent(eEventType eventType)
        {
            if (enabled)
            {
                int conversationIndex = m_events[(int)eventType];
                if (conversationController && !conversationController.IsPlayingDialog && m_activeEvents.Contains(eventType))
                {
                    UnityAction onCloseAction = null;
                    if (onCloseEvent != null) onCloseAction = () => onCloseEvent.Invoke();
                    if (conversationIndex < 0)
                        conversationController.StartConversation(onCloseAction);
                    else
                        conversationController.StartConversation(conversationIndex, onCloseAction);
                }
            }
        }

        public enum eEventType
        {
            Start,
            OnEnable,
            [Description("Physics 3D/")]
            OnTriggerEnter,
            [Description("Physics 3D/")]
            OnTriggerExit,
            [Description("Physics 2D/")]
            OnTriggerEnter2D,
            [Description("Physics 2D/")]
            OnTriggerExit2D,
            [Description("Physics 3D/")]            
            OnCollisionEnter,
            [Description("Physics 3D/")]
            OnCollisionExit,
            [Description("Physics 2D/")]
            OnCollisionEnter2D,
            [Description("Physics 2D/")]
            OnCollisionExit2D,
            [Description("Input/")]
            OnClick,
            OnTimer,
            [Description("Input/")]
            OnKeyDown,
            [Description("Input/")]
            OnButtonDown,
            //--- Add new enums at the end. Do not change the enum order ---//
        }
        //virtual protected void Start () { ExecuteEvent(eEventType.Start); } // defined above
        virtual protected void OnEnable() { ExecuteEvent(eEventType.OnEnable); }
        virtual protected void DoOnTriggerEnter(Collider other) { ExecuteEvent(eEventType.OnTriggerEnter); }
        virtual protected void DoOnTriggerEnter2D(Collider2D collision) { ExecuteEvent(eEventType.OnTriggerEnter2D); }
        virtual protected void DoOnTriggerExit(Collider other) { ExecuteEvent(eEventType.OnTriggerExit); }
        virtual protected void DoOnTriggerExit2D(Collider2D collision) { ExecuteEvent(eEventType.OnTriggerExit2D); }
        virtual protected void OnCollisionEnter(Collision collision) { if(CheckTagFilter(collision.gameObject, TagFilter)) ExecuteEvent(eEventType.OnCollisionEnter); }
        virtual protected void OnCollisionEnter2D(Collision2D collision) { if (CheckTagFilter(collision.gameObject, TagFilter)) ExecuteEvent(eEventType.OnCollisionEnter2D); }
        virtual protected void OnCollisionExit(Collision collision) { if (CheckTagFilter(collision.gameObject, TagFilter)) ExecuteEvent(eEventType.OnCollisionExit); }
        virtual protected void OnCollisionExit2D(Collision2D collision) { if (CheckTagFilter(collision.gameObject, TagFilter)) ExecuteEvent(eEventType.OnCollisionExit2D); }
        virtual protected void OnClick() // optimized to avoid using OnMouse messages
        {
            bool isBlockedByUIelement = EventSystem.current && EventSystem.current.IsPointerOverGameObject();
            if(!isBlockedByUIelement)
                ExecuteEvent(eEventType.OnClick);
        }
    }
}
