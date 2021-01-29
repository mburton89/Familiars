// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using UnityEditorInternal;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Events;
using UnityEngine.Events;

namespace CreativeSpore.RPGConversationEditor
{
    public class DialogActionReorderableList : ReorderableList
    {
        public enum eGUIMode
        {
            Scene,
            Inspector
        };
        public eGUIMode guiMode = eGUIMode.Inspector;
        public int DraggedActionLinkIndex { get { return m_draggedActionIdx; } }
        public bool IsFocused { get { return this == s_focusedDialogRlist; } }

        private int m_draggedActionIdx = -1;
        private float m_rowWidth = 0f;
        private List<Vector2> m_connectTogglePositions = new List<Vector2>();
        private EventType m_updateRowWidthDuringEvent;
        private Dialog m_dialog;

        private static DialogActionReorderableList s_focusedDialogRlist;

        public void ResetDraggedActionLinkIndex()
        {
            m_draggedActionIdx = -1;
        }

        public Vector2 GetConnectTogglePosition(int index)
        {
            if (index >= 0 && index < m_connectTogglePositions.Count)
                return m_connectTogglePositions[index];
            else // this could happen when adding an action
                return Vector2.zero; 
        }

        public new void DoLayoutList()
        {
            m_connectTogglePositions.Clear();
            m_updateRowWidthDuringEvent = EventType.Repaint;

            base.DoLayoutList();
        }

        public new void DoList(Rect rect)
        {
            //m_rowWidth = rect.width;
            m_connectTogglePositions.Clear();
            m_updateRowWidthDuringEvent = EventType.Layout;
            base.DoList(rect);
        }

        public DialogActionReorderableList(SerializedObject serializedObject, SerializedProperty dialogProp, Dialog dialog)
            : base(serializedObject, dialogProp.FindPropertyRelative("dialogActions"), true, true, true, true)
        {
            m_dialog = dialog;
            drawHeaderCallback += (rect) =>
            {
                GUI.color = DialogColorUtils.GetColorWithHigherContrast(dialog.color, DialogEditorStyles.c_almostBlack, DialogEditorStyles.c_almostWhite);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Dialog Actions", DialogEditorStyles.dialogHeader);
                GUI.color = Color.white;
                //NOTE: This is the only place where the ResetDraggedActionLinkIndex is called properly
                if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)
                {                    
                    ResetDraggedActionLinkIndex();
                }
                if (Event.current.type == EventType.MouseDown)
                {
                    s_focusedDialogRlist = this;
                }
            };
            elementHeightCallback += (index) =>
            {
                return GetElementHeight(index);
            };
            drawFooterCallback += (rect) =>
            {
                if (count == 0)
                {
                    float elementHeight = this.elementHeight + 7f;
                    Rect rList = new Rect(rect.x, rect.y - elementHeight, rect.width, elementHeight);
                    if(Event.current.type == EventType.Repaint)
                        ReorderableList.defaultBehaviours.boxBackground.Draw(rList, false, false, false, false);
                    Rect rLabel = new Rect(rList.x + 4f, rList.y, rList.width, rList.height);                    
                    EditorGUI.LabelField(rLabel, "No Actions / End Conversation");                    
                }

                if (guiMode == eGUIMode.Scene)
                {                                        
                    GUI.backgroundColor = new Color(1f, 1f, 1f, .2f);
                    defaultBehaviours.DrawFooter(rect, this);
                    GUI.backgroundColor = Color.white;


                }
                else
                {
                    defaultBehaviours.DrawFooter(rect, this);
                }
            };
            drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                // NOTE: this should be done in Layout or it won't be updated in the SceveView until forcing a repaint, 
                // but rect.width is negative in the InspectorView during the Layout event because its using DoLayoutList
                if (Event.current.type == m_updateRowWidthDuringEvent)
                    m_rowWidth = rect.width;
                
                Dialog.DialogAction dialogAction = dialog.dialogActions[index];
                Rect rectConnectionToggle = new Rect(rect.xMax - 10f, rect.y, 20f, EditorGUIUtility.singleLineHeight);
                m_connectTogglePositions.Add(rectConnectionToggle.center);
                GUI.color = DialogColorUtils.GetColorWithHigherContrast(dialog.color, DialogEditorStyles.c_almostBlack, DialogEditorStyles.c_almostWhite);
                GUI.Label(new Rect(rect.x, rect.y + 1f, 18, EditorGUIUtility.singleLineHeight), index.ToString(), DialogEditorStyles.dialogHeader);
                GUI.color = Color.white;
                var dialogActionProperty = serializedProperty.GetArrayElementAtIndex(index);
                var nameProperty = dialogActionProperty.FindPropertyRelative("name");                
                float namePropHeight = DialogEditorStyles.textArea.CalcHeight(new GUIContent(nameProperty.stringValue), m_rowWidth - 30f);
                float actionEventsPropertyHeight = 20;//NOTE: it looks that calculating height here is no necessary GetUnityEventPropertyHeight(onSubmitProperty) + GetUnityEventPropertyHeight(onPreProcessProperty);
                Rect rName = new Rect(rect.x + 20f, rect.y + 1f, rect.width - 30f, namePropHeight);
                Rect rActionEvents = new Rect(rect.x, rect.y + namePropHeight + 5f, rect.width, actionEventsPropertyHeight);
                if (nameProperty != null)
                {
                    EditorGUIUtility.labelWidth = 40f;
                    nameProperty.stringValue = EditorGUI.TextArea(rName, nameProperty.stringValue, DialogEditorStyles.textArea);
                    if (guiMode == eGUIMode.Scene)
                    {
                        bool checkToggleDrag = Event.current.type == EventType.MouseDown && rectConnectionToggle.Contains(Event.current.mousePosition);
                        GUI.Toggle(rectConnectionToggle, dialogAction.targetDialogId >= 0, "", EditorStyles.radioButton);
                        if (Event.current.type == EventType.Used && checkToggleDrag)
                        {
                            m_draggedActionIdx = index;
                        }
                    }
                    EditorGUIUtility.labelWidth = 0f;
                    if (this.guiMode == eGUIMode.Inspector)
                    {
                        var onSubmitProperty = dialogActionProperty.FindPropertyRelative("onSubmit");
                        var onSubmit = dialog.dialogActions[index].onSubmit;
                        var onPreProcessProperty = dialogActionProperty.FindPropertyRelative("onPreProcess");
                        var onPreProcess = dialog.dialogActions[index].onPreProcess;
                        bool onSubmitIsDefined = onSubmit != null && onSubmit.GetPersistentEventCount() > 0;
                        bool onPreProcessIsDefined = onPreProcess != null && onPreProcess.GetPersistentEventCount() > 0;
                        if (onSubmitIsDefined)
                        {
                            EditorGUI.PropertyField(rActionEvents, onSubmitProperty);
                            rActionEvents.position += Vector2.up * GetUnityEventPropertyHeight(onSubmitProperty);
                        }
                        if (onPreProcessIsDefined)
                        {                            
                            EditorGUI.PropertyField(rActionEvents, onPreProcessProperty);
                            rActionEvents.position += Vector2.up * GetUnityEventPropertyHeight(onPreProcessProperty);
                        }

                        if (!onSubmitIsDefined || !onPreProcessIsDefined)
                        {
                            rActionEvents = new Rect(rActionEvents.center.x - 40f, rActionEvents.y, 80f, 16f);
                            if (GUI.Button(rActionEvents, "Add Event..."))
                            {
                                GenericMenu addEventMenu = new GenericMenu();
                                if (!onSubmitIsDefined)
                                    addEventMenu.AddItem(new GUIContent("Add onSubmit Event"), false, () => { UnityEventTools.AddPersistentListener(onSubmit); });
                                if (!onPreProcessIsDefined)
                                    addEventMenu.AddItem(new GUIContent("Add onPreProcess Event"), false, () => { UnityEventTools.AddPersistentListener(onPreProcess); });
                                addEventMenu.ShowAsContext();
                            }
                        }
                    }
                }
            };
            onAddCallback += (list) =>
            {
                if (index < 0)
                    index = count - 1;
                if (serializedProperty.arraySize == 0)
                    serializedProperty.arraySize++;                
                else
                    serializedProperty.InsertArrayElementAtIndex(Mathf.Max(index, 0));
                index = Mathf.Max(index + 1, 0);
                serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue = "new action";
                serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("targetDialogId").intValue = -1;
                serializedObject.ApplyModifiedProperties();
            };
            onReorderCallback += (list) =>
            {
                GUI.changed = true;
            };
        }

        private float[] m_elementheightCache;
        private float GetElementHeight(int index)
        {
            // NOTE: this optimization was made because sometimes, like then dragging the sceneve view for the first time,
            // there is a spike of CPU usage because of multiple Layout events (not happening in all Unity versions)
            if (m_elementheightCache == null || m_elementheightCache.Length != count)
            {
                System.Array.Resize(ref m_elementheightCache, count);
            }

            EventType updateEvent = guiMode == eGUIMode.Scene ? EventType.Repaint : EventType.Layout;
            if ( Event.current.type == updateEvent || m_elementheightCache[index] == 0)
            {
                var dialogActionProperty = serializedProperty.GetArrayElementAtIndex(index);
                var nameProperty = dialogActionProperty.FindPropertyRelative("name");
                var onSumbitProperty = dialogActionProperty.FindPropertyRelative("onSubmit");
                var onSubmit = m_dialog.dialogActions[index].onSubmit;
                var onPreProcessProperty = dialogActionProperty.FindPropertyRelative("onPreProcess");
                var onPreProcess = m_dialog.dialogActions[index].onPreProcess;
                float namePropHeight = DialogEditorStyles.textArea.CalcHeight(new GUIContent(nameProperty.stringValue), m_rowWidth - 30f);
                bool onSubmitIsDefined = onSubmit != null && onSubmit.GetPersistentEventCount() > 0;
                bool onPreProcessIsDefined = onPreProcess != null && onPreProcess.GetPersistentEventCount() > 0;
                float actionEventsPropHeight = onSubmit != null && onSubmit.GetPersistentEventCount() > 0 ? GetUnityEventPropertyHeight(onSumbitProperty) : 20f;
                actionEventsPropHeight =
                    (onSubmitIsDefined ? GetUnityEventPropertyHeight(onSumbitProperty) : 0f) +
                    (onPreProcessIsDefined ? GetUnityEventPropertyHeight(onPreProcessProperty) : 0f) +
                    (!onSubmitIsDefined || !onPreProcessIsDefined ? 20f : 0f);
                m_elementheightCache[index] = namePropHeight + (this.guiMode == eGUIMode.Inspector ? actionEventsPropHeight + 10f : 10f);
            }
            return m_elementheightCache[index];
        }

        // The reorderable list of a UnityEventPropertyView gets invalidated sometimes, like when adding/removing a component to the gameObject
        protected float GetUnityEventPropertyHeight(SerializedProperty prop)
        {
            int size = Mathf.Max( 1, prop.FindPropertyRelative("m_PersistentCalls.m_Calls").arraySize);
            return 38f + 43f * size;
        }

    }
}
