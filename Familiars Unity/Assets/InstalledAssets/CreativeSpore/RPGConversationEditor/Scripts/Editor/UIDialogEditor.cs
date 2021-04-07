// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEditor.Events;
using UnityEngine.Events;

namespace CreativeSpore.RPGConversationEditor
{
    [CustomEditor(typeof(UIDialog))]
    public class UIDialogEditor : Editor
    {
        UIDialog m_target;
        TypewriterTextEditor m_typewriterTextEditor = null;

        private void OnEnable()
        {
            m_target = target as UIDialog;
            TypewriterText typewriterText = m_target.text as TypewriterText;
            if(typewriterText)
                m_typewriterTextEditor = Editor.CreateEditor(m_target.text) as TypewriterTextEditor;
        }

        private void OnDisable()
        {
            if(m_typewriterTextEditor)
                DestroyImmediate(m_typewriterTextEditor);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //Add a Persistent event. Code keep for later, when I know in what cases these events should be automatically attached                                        
            const string actionEventsHelp = "This will allow to communicate the PointerClick and Submit event to the UIDialog";
            if (m_target.actionList && GUILayout.Button(new GUIContent("Add Default EventTriggers to Dialog Actions", actionEventsHelp)))
            {
                for (int i = 0, s = m_target.actionList.transform.childCount; i < s; ++i)
                {
                    Transform actionObj = m_target.actionList.transform.GetChild(i);
                    if (actionObj)
                    {
                        EventTrigger trigger = actionObj ? actionObj.GetComponent<EventTrigger>() : null;
                        if (!trigger) trigger = actionObj.gameObject.AddComponent<EventTrigger>();
                        AddVoidPersistentListenerOnce(trigger, m_target.DoActionTriggerEvent, EventTriggerType.PointerClick);
                        AddVoidPersistentListenerOnce(trigger, m_target.DoActionTriggerEvent, EventTriggerType.Submit);
                        AddVoidPersistentListenerOnce(trigger, m_target.DoActionSelectedEvent, EventTriggerType.Select);
                    }
                }
            }

            if (m_target.text && GUILayout.Button(new GUIContent("Add Default EventTriggers to Text component", actionEventsHelp)))
            {
                EventTrigger trigger = m_target.text.GetComponent<EventTrigger>();
                if (!trigger) trigger = m_target.text.gameObject.AddComponent<EventTrigger>();
                AddVoidPersistentListenerOnce(trigger, m_target.DoContinueTextEvent, EventTriggerType.PointerClick);
                AddVoidPersistentListenerOnce(trigger, m_target.DoContinueTextEvent, EventTriggerType.Submit);
            }

            if (m_typewriterTextEditor)
            {
                m_typewriterTextEditor.DoTypewriterInspectorGUI();
                if (GUI.changed)
                {
                    m_typewriterTextEditor.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        bool AddVoidPersistentListenerOnce(EventTrigger evTrigger, UnityAction call, EventTriggerType eventId)
        {
            if(!ContainsTriggerCallback(evTrigger, call, eventId))
            {
                EventTrigger.Entry entry = new EventTrigger.Entry() { eventID = eventId };
                UnityEventTools.AddVoidPersistentListener(entry.callback, call);
                evTrigger.triggers.Add(entry);
                Debug.Log("Added UnityEvent to " + evTrigger.name + " calling " + call.Method.Name + " when " + eventId + " is triggered!", evTrigger);
                return true;
            }
            return false;
        }

        bool ContainsTriggerCallback(EventTrigger evTrigger, UnityAction call, EventTriggerType eventId)
        {
            foreach (var trigger in evTrigger.triggers)
            {
                if(trigger.eventID == eventId)
                {
                    for(int i = 0, s = trigger.callback.GetPersistentEventCount(); i < s; ++i)
                    {
                        if (trigger.callback.GetPersistentMethodName(i) == call.Method.Name)
                            return true;
                    }
                }
            }
            return false;
        }

        bool ContainsTriggerCallback(EventTrigger evTrigger, UnityAction call)
        {
            foreach (var trigger in evTrigger.triggers)
            {
                //if (trigger.eventID == eventId)
                {
                    for (int i = 0, s = trigger.callback.GetPersistentEventCount(); i < s; ++i)
                    {
                        if (trigger.callback.GetPersistentMethodName(i) == call.Method.Name)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
