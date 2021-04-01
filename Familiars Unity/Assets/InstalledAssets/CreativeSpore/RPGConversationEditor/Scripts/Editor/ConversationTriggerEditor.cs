// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AnimatedValues;

namespace CreativeSpore.RPGConversationEditor
{
    [CustomEditor(typeof(ConversationTrigger))]
    public class ConversationTriggerEditor : Editor 
	{
        [MenuItem("GameObject/RPG Conversation Editor/ConversationTrigger", false, 10)]
        public static void AddConversationTrigger()
        {
            DialogEditorUtils.CreateGameObjectWithComponent<ConversationTrigger>();
        }

        private ConversationTrigger m_target;
        private SerializedProperty m_conversationControllerProp;
        private SerializedProperty m_onCloseEventProp;
        private SerializedProperty m_activeEventsProp;
        private SerializedProperty m_eventsProp;
        private SerializedProperty m_DialogTimeProp;
        private SerializedProperty m_TimeBetweenDialogsProp;
        private SerializedProperty m_TriggerKeyCodeProp;
        private SerializedProperty m_TriggerButtonNameProp;
        private SerializedProperty m_TagFilterProp;
        private GUIContent m_iconToolbarMinus;
        private void OnEnable()
        {
            m_target = target as ConversationTrigger;
            m_conversationControllerProp = serializedObject.FindProperty("conversationController");
            m_onCloseEventProp = serializedObject.FindProperty("onCloseEvent");
            m_activeEventsProp = serializedObject.FindProperty("m_activeEvents");
            m_eventsProp = serializedObject.FindProperty("m_events");
            m_DialogTimeProp = serializedObject.FindProperty("DialogTime");
            m_TimeBetweenDialogsProp = serializedObject.FindProperty("TimeBetweenDialogs");
            m_TriggerKeyCodeProp = serializedObject.FindProperty("TriggerKeyCode");
            m_TriggerButtonNameProp = serializedObject.FindProperty("TriggerButtonName");
            m_TagFilterProp = serializedObject.FindProperty("TagFilter");
            m_iconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus", "Remove trigger event");            
        }

        public override void OnInspectorGUI()
        {            
            serializedObject.Update();  
            if(!m_conversationControllerProp.objectReferenceValue)
            {
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.HelpBox("Set the Conversation Controller component you want to start with an event.", MessageType.Info);
                GUI.backgroundColor = Color.white;
                if(GUILayout.Button("Add Conversation Controller"))
                {
                    m_conversationControllerProp.objectReferenceValue = ConversationControllerEditor.AddConversation();
                }
            }
            EditorGUILayout.PropertyField(m_conversationControllerProp);            
            
            if (m_target.conversationController)
            {
                List<string> conversationList = m_target.conversationController.conversations.Select((o, idx) => idx + ". " + o.name).ToList();
                List<string> conversationPopupList = new List<string>() { "<default>" };
                conversationPopupList.AddRange(conversationList);
                //GUILayout.Label("Conversation Event Triggers", EditorStyles.boldLabel);
                EditorGUIUtility.labelWidth = 140f;
                int index = 0;
                var iter = m_activeEventsProp.GetEnumerator();
                while (iter.MoveNext())
                {
                    ConversationTrigger.eEventType eventType = (ConversationTrigger.eEventType)(iter.Current as SerializedProperty).enumValueIndex;
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label(eventType.ToString(), EditorStyles.boldLabel);
                    Rect rRemoveButton = GUILayoutUtility.GetLastRect();
                    rRemoveButton.xMin = rRemoveButton.xMax - 40f;
                    if (GUI.Button(rRemoveButton, this.m_iconToolbarMinus, DialogEditorStyles.preButton))
                    {
                        RemoveEvent(index);
                    }
                    SerializedProperty eventProp = m_eventsProp.GetArrayElementAtIndex((int)eventType);
                    eventProp.intValue = -1 + EditorGUILayout.Popup("Conversation", eventProp.intValue + 1, conversationPopupList.ToArray());
                    //Display Event Parameters
                    DisplayEventParameters(eventType);
                    EditorGUILayout.EndVertical();
                    ++index;                    
                }                
                EditorGUILayout.Space();

                if (m_activeEventsProp.arraySize < m_eventsProp.arraySize)
                {
                    if (GUILayout.Button("Add Event Trigger..."))
                    {
                        GenericMenu addEventMenu = new GenericMenu();
                        List<ConversationTrigger.eEventType> availableTypes = GetAvailableEvents();
                        for (int i = 0; i < availableTypes.Count; ++i)
                        {
                            ConversationTrigger.eEventType ev = availableTypes[i];
                            string enumDesc = GetEnumDescription(ev);
                            if(m_target.IsUsingEvent(ev))
                                addEventMenu.AddDisabledItem(new GUIContent(enumDesc + ev.ToString()));
                            else
                                addEventMenu.AddItem(new GUIContent(enumDesc + ev.ToString()), false, () => { AddEvent(ev); });
                        }
                        addEventMenu.ShowAsContext();
                    }
                }
                EditorGUILayout.PropertyField(m_onCloseEventProp);
                EditorGUIUtility.labelWidth = 0f;
            }


            //NOTE: eventProp.stringValue is not saved if this is done only when GUI.changed is true
            serializedObject.ApplyModifiedProperties();
        }

        public static string GetEnumDescription(System.Enum value)
        {
            System.Reflection.FieldInfo fi = value.GetType().GetField(value.ToString());

            System.ComponentModel.DescriptionAttribute[] attributes =
                (System.ComponentModel.DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(System.ComponentModel.DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return "";
        }

        private void AddEvent(ConversationTrigger.eEventType eventType)
        {
            ++m_activeEventsProp.arraySize;
            m_activeEventsProp.GetArrayElementAtIndex(m_activeEventsProp.arraySize - 1).enumValueIndex = (int)eventType;
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveEvent(int index)
        {
            m_activeEventsProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }

        private List<ConversationTrigger.eEventType> GetAvailableEvents()
        {
            List<ConversationTrigger.eEventType> enumValues = new List<ConversationTrigger.eEventType>( (ConversationTrigger.eEventType[])System.Enum.GetValues(typeof(ConversationTrigger.eEventType)));
            /* This will remove the events that has been used. Instead, I will disable them in the context menu
            var iter = m_activeEventsProp.GetEnumerator();
            while (iter.MoveNext())
                enumValues.Remove((ConversationTrigger.eEventType)(iter.Current as SerializedProperty).enumValueIndex);
            */
            return enumValues;
        }

        private void DisplayEventParameters(ConversationTrigger.eEventType eventType)
        {
            switch (eventType)
            {
                case ConversationTrigger.eEventType.OnClick:
                    if (!m_target.GetComponent<Collider>() && !m_target.GetComponent<Collider2D>())
                    {
                        GUI.backgroundColor = Color.yellow;
                        EditorGUILayout.HelpBox("You need to add a collider or collider2D to detect the onClick event.", MessageType.Info);
                        GUI.backgroundColor = Color.white;
                        if (GUILayout.Button("Add Collider"))
                        {
                            m_target.gameObject.AddComponent<BoxCollider>().isTrigger = true;
                        }
                        if (GUILayout.Button("Add Collider2D"))
                        {
                            m_target.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
                        }
                    }
                    break;
                case ConversationTrigger.eEventType.OnTimer:
                    EditorGUILayout.PropertyField(m_DialogTimeProp);
                    EditorGUILayout.PropertyField(m_TimeBetweenDialogsProp);
                    break;
                case ConversationTrigger.eEventType.OnKeyDown:
                case ConversationTrigger.eEventType.OnButtonDown:
                    if(eventType == ConversationTrigger.eEventType.OnKeyDown)
                        EditorGUILayout.PropertyField(m_TriggerKeyCodeProp);
                    else
                        EditorGUILayout.PropertyField(m_TriggerButtonNameProp);

                    EditorGUILayout.PropertyField(m_TagFilterProp);
                    if (!m_target.GetComponentInChildren<Collider>() && !m_target.GetComponentInChildren<Collider2D>())
                    {                                                
                        GUI.backgroundColor = Color.yellow;
                        EditorGUILayout.HelpBox("You need to add a trigger collider in order make " + eventType + " to work.", MessageType.Info);
                        GUI.backgroundColor = Color.white;
                        if (GUILayout.Button("Add Collider"))
                        {
                            m_target.gameObject.AddComponent<BoxCollider>().isTrigger = true;
                        }
                        if (GUILayout.Button("Add Collider2D"))
                        {
                            m_target.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
                        }
                    }
                    break;
                case ConversationTrigger.eEventType.OnCollisionEnter:
                case ConversationTrigger.eEventType.OnCollisionExit:
                case ConversationTrigger.eEventType.OnCollisionEnter2D:
                case ConversationTrigger.eEventType.OnCollisionExit2D:
                case ConversationTrigger.eEventType.OnTriggerEnter:
                case ConversationTrigger.eEventType.OnTriggerExit:
                case ConversationTrigger.eEventType.OnTriggerEnter2D:
                case ConversationTrigger.eEventType.OnTriggerExit2D:
                    EditorGUILayout.PropertyField(m_TagFilterProp);
                    break;
            }
        }
    }
}
