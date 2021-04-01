// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.AnimatedValues;

namespace CreativeSpore.RPGConversationEditor
{
    [CustomEditor(typeof(ConversationController))]
    public class ConversationControllerEditor : Editor
    {
        [MenuItem("GameObject/RPG Conversation Editor/Conversation", false, 10)]
        public static ConversationController AddConversation()
        {
            return DialogEditorUtils.CreateGameObjectWithComponent<ConversationController>();
        }

        public const int k_minDialogWidth = 190;
        public const int k_minDialogHeight = 70; //NOTE: it will be k_minDialogHeight + actionReordList height

        public float ClampedScale { get { return m_handleSize <= 0f ? 1f : Mathf.Clamp(m_zoomFactor / m_handleSize, 0.001f, 5f); } }

        private ConversationController m_target;

        private SerializedProperty m_conversationAssetProp;
        private SerializedProperty m_conversationsProp;
        private SerializedProperty m_conversationDataProp;                
        private SerializedProperty m_dialogListProp;
        private SerializedProperty m_selectedDialogProp;
        private SerializedProperty m_uiDialogProp;
        private SerializedProperty m_uiDialogParentProp;
        private SerializedProperty m_uiDialogOffsetProp;
        private SerializedProperty m_defaultDialogProp;

        private Dialog m_selectedDialog = null;
        private Dialog m_mouseOverDialog = null;
        private Dialog m_draggedDialog = null;
        private Dialog m_resizedDialog = null;
        private Vector2 m_resizeSide = Vector2.zero;
        private float m_zoomFactor = 1f;
        private DialogActionReorderableList m_dialogActionRList = null;
        private ReorderableList m_dialogSentencesRList = null;
        private ReorderableList m_conversationReordList = null;

        private bool m_isDirty = false;
        private bool m_invalidatePreview = false;
        private AnimBool m_overrideNameAnimToggle;
        private AnimBool m_uiDialogAnimToggle;
        private AnimBool m_convPropAnimfoldout;
        private AnimBool m_convActionsAnimfoldout;
        private AnimBool m_sceneConvActionsAnimfoldout;

        private static bool s_foldoutConvProp = true;
        private static bool s_foldoutConvActions = true;
        private static bool s_foldoutDialogEvents = true;

        private void SetSelectedDialog(Dialog dialog)
        {
            // NOTE: for some reason, if the m_dicDialogActionReordList is cleared, an exception will be triggered because of the 
            // UnityEventDrawer of the DialogActions rendered in the SceneGUI:
            // NullReferenceException: SerializedObject of SerializedProperty has been Disposed.
            // It happens only if a new component is added or removed or when going back from Debug Inspector mode 
            // because the serializedObject cached in the UnityEventDrawer is disposed at some point.
            // m_dicDialogActionReordList.Clear(); 
            m_mouseOverDialog = null;
            if (m_selectedDialog != dialog)
            {
                if (m_selectedDialog != null /*&& m_selectedDialog != dialog*/)
                {
                    DialogActionReorderableList rList = GetDialogActionReordList(m_selectedDialog);
                    if(rList != null)
                        rList.index = -1; // deselect previous dialog action
                }
                m_selectedDialog = dialog;
                if (dialog != null)
                {
                    m_selectedDialogProp = m_dialogListProp.GetArrayElementAtIndex(m_target.DialogList.IndexOf(dialog));
                    m_dialogActionRList = CreateDialogActionRList(dialog);
                    m_dialogSentencesRList = CreateDialogSentencesRList();
                    m_overrideNameAnimToggle.value = !dialog.inheritName;
                    m_uiDialogAnimToggle.value = m_uiDialogProp.objectReferenceValue;
                    UpdatePreviewTexture();
                }
                else
                {
                    m_selectedDialogProp = null;
                    m_dialogActionRList = null;
                }
            }
        }

        private void OnEnable()
        {
            m_target = target as ConversationController;
            m_overrideNameAnimToggle = new AnimBool();
            m_uiDialogAnimToggle = new AnimBool();
            m_convPropAnimfoldout = new AnimBool(s_foldoutConvProp);
            m_convActionsAnimfoldout = new AnimBool(s_foldoutConvActions);
            m_sceneConvActionsAnimfoldout = new AnimBool(false);
            m_overrideNameAnimToggle.valueChanged.AddListener(base.Repaint);
            m_uiDialogAnimToggle.valueChanged.AddListener(base.Repaint);
            m_convPropAnimfoldout.valueChanged.AddListener(base.Repaint);
            m_convActionsAnimfoldout.valueChanged.AddListener(base.Repaint);
            m_sceneConvActionsAnimfoldout.valueChanged.AddListener(() => { SceneView.RepaintAll(); });
            m_conversationAssetProp = serializedObject.FindProperty("conversationAsset");
            m_uiDialogProp = serializedObject.FindProperty("uiDialog");
            m_uiDialogParentProp = serializedObject.FindProperty("uiDialogParent");
            m_uiDialogOffsetProp = serializedObject.FindProperty("uiDialogOffset");
            m_defaultDialogProp = serializedObject.FindProperty("defaultDialog");
            m_conversationReordList = CreateConversationsRList();
            m_selectedDialog = null; // gets invalidated when compiling code (avoid GetDialogActionReordList error)
            m_firstSceneGUI = true;
            UpdateConversationProperties();            
        }

        private void OnDisable()
        {
            m_overrideNameAnimToggle.valueChanged.RemoveListener(base.Repaint);
            m_uiDialogAnimToggle.valueChanged.RemoveListener(base.Repaint);
            m_convPropAnimfoldout.valueChanged.RemoveListener(base.Repaint);
            m_convActionsAnimfoldout.valueChanged.RemoveListener(base.Repaint);
            m_sceneConvActionsAnimfoldout.valueChanged.RemoveAllListeners();
        }

        void UpdateConversationProperties()
        {
            if (m_conversationAssetProp.objectReferenceValue)
            {
                SerializedObject temp = new SerializedObject(m_conversationAssetProp.objectReferenceValue);
                m_conversationsProp = temp.FindProperty("conversations");
            }
            else
            {
                m_conversationsProp = serializedObject.FindProperty("m_conversations");
            }
            m_conversationReordList.index = m_target.ActiveConversationIndex;
            if (m_target.ActiveConversationIndex >= 0)
            {
                m_conversationDataProp = m_conversationsProp.GetArrayElementAtIndex(m_target.ActiveConversationIndex);
                m_dialogListProp = m_conversationDataProp.FindPropertyRelative("dialogList");
                SetSelectedDialog(m_target.ActiveConversation.StartDialog);
            }
            else
            {
                m_conversationDataProp = m_dialogListProp = null;
                SetSelectedDialog(null);
            }
            SceneView.RepaintAll();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Dialog Preview");
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            base.OnPreviewGUI(r, background);
            if (s_previewTexture)
            {
                EditorGUI.DrawPreviewTexture(r, s_previewTexture, null, ScaleMode.ScaleToFit);
            }
        }

        public override void OnInspectorGUI()
        {
            Event e = Event.current;
            if(e.type == EventType.ValidateCommand || e.type == EventType.ExecuteCommand)
            {
                //Avoid crash when using Undo/Redo
                if (e.commandName == "UndoRedoPerformed")
                {
                    if (m_selectedDialog != null && !m_target.DialogList.Contains(m_selectedDialog))
                    {
                        m_dicDialogActionReordList.Remove(m_selectedDialog);
                        SetSelectedDialog(m_target.ActiveConversation.StartDialog);
                    }                    
                    //return; //commented to fi undo/redo text changes (it looks like it is not crashing)
                }
            }
            KeepValidDialogValues();
            //DrawDefaultInspector();
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_conversationAssetProp);
            if(EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                UpdateConversationProperties();
            }
            
            if(m_conversationAssetProp.objectReferenceValue)
            {
                if (GUILayout.Button("Embed Conversations"))
                {
                    m_target.EmbedConversationData();

                    serializedObject.Update();                    
                    UpdateConversationProperties();
                    //NOTE: fixes the memory reference between m_conversations and conversationAsset.conversations without a deep copy of the list
                    m_conversationsProp.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                    m_conversationsProp.arraySize--;
                    serializedObject.ApplyModifiedProperties();
                    int savedIndex = m_conversationReordList.index;
                    m_conversationReordList = CreateConversationsRList();
                    m_conversationReordList.index = savedIndex;
                    //---
                }
            }
            else
            {
                if (GUILayout.Button("Export Conversations"))
                {
                    ConversationAsset asset = DialogEditorUtils.CreateAssetWithSaveFilePanel<ConversationAsset>();
                    if (asset)
                    {
                        asset.conversations = m_target.conversations;                       
                        m_target.conversationAsset = asset;
                        serializedObject.Update();
                        UpdateConversationProperties();
                        int savedIndex = m_conversationReordList.index;
                        m_conversationReordList = CreateConversationsRList();
                        m_conversationReordList.index = savedIndex;
                    }
                }
            }
            /*
            if(GUILayout.Button("Export Conversation Json"))
            {
                string jsonStr = JsonUtility.ToJson(m_target.SelectedConversation, true);
                Debug.Log(jsonStr);                
                System.IO.File.WriteAllText(@"e:\tmp.txt", jsonStr);
            }*/

            if(m_target.ActiveConversation == null)
            {
                EditorGUILayout.HelpBox("Select a conversation to edit its properties", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                return;                
            }
            EditorGUILayout.Space();
            s_foldoutConvProp = EditorGUILayout.Foldout(s_foldoutConvProp, "UIDIALOG PROPERTIES", DialogEditorStyles.boldFoldout);
            m_convPropAnimfoldout.target = s_foldoutConvProp;
            if(EditorGUILayout.BeginFadeGroup(m_convPropAnimfoldout.faded))
            {
                GUI.backgroundColor = new Color(0.8f, 0.9f, 0.8f, 1f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.indentLevel++;
                    {
                        if (!m_uiDialogProp.objectReferenceValue)
                        {
                            GUI.backgroundColor = Color.yellow;
                            EditorGUILayout.HelpBox("Set the UIDialog component used to display the dialogs. You can drag the prefab or gameObject containing the UIDialog component.", MessageType.Info);
                            GUI.backgroundColor = Color.white;
                        }
                        GameObject uiDialogObj = m_uiDialogProp.objectReferenceValue ? (m_uiDialogProp.objectReferenceValue as UIDialog).gameObject : null;
                        EditorGUI.BeginChangeCheck();
                        {
                            uiDialogObj = EditorGUILayout.ObjectField(
                                new GUIContent("UIDialog", "Select a gameobject or prefab where the UIDialog component is attached to. This UIDialog will be used to render the dialogs in game."),
                                uiDialogObj, typeof(GameObject), true) as GameObject;
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_uiDialogProp.objectReferenceValue = uiDialogObj? uiDialogObj.GetComponent<UIDialog>() : null;
                            if (uiDialogObj && !m_uiDialogProp.objectReferenceValue)
                            {
                                Debug.LogWarning("The selected UIDialog gameobject has no UIDialog component attached.");
                            }
                        }
                        //EditorGUILayout.PropertyField(m_uiDialogProp);            
                        m_uiDialogAnimToggle.target = m_uiDialogProp.objectReferenceValue;
                        if (EditorGUILayout.BeginFadeGroup(m_uiDialogAnimToggle.faded))
                        {
                            EditorGUILayout.PropertyField(m_uiDialogParentProp, new GUIContent("UIDialog Parent"));
                            GUI.enabled = m_uiDialogParentProp.objectReferenceValue;
                            EditorGUILayout.PropertyField(m_uiDialogOffsetProp, new GUIContent("UIDialog Offset"));
                            GUI.enabled = true;
                        }
                        EditorGUILayout.EndFadeGroup();
                    }
                    EditorGUI.indentLevel--;
                    if (GUI.changed)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.Space();
            s_foldoutConvActions = EditorGUILayout.Foldout(s_foldoutConvActions, "CONVERSATION ACTIONS", DialogEditorStyles.boldFoldout);
            m_convActionsAnimfoldout.target = s_foldoutConvActions;
            if (EditorGUILayout.BeginFadeGroup(m_convActionsAnimfoldout.faded))
            {
                GUI.backgroundColor = new Color(0.9f, 0.9f, 0.8f, 1f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    DisplayConversationActions();
                }
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.Space();

            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.9f, 1f);
            m_conversationReordList.DoLayoutList();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {                
                GUILayout.Label("Conversation Properties", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(m_defaultDialogProp);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_conversationDataProp.FindPropertyRelative("events"), true);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                if (GUI.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.9f, 1f);
            GUILayout.Label("SELECTED DIALOG PROPERTIES", EditorStyles.boldLabel);
            if (m_selectedDialog != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DoDialogInspectorView(m_selectedDialog);
                EditorGUILayout.EndVertical();
            }
            GUI.backgroundColor = Color.white;

            if (GUI.changed)
            {
                m_invalidatePreview = true;
                SceneView.RepaintAll();
                serializedObject.ApplyModifiedProperties();
            }

            if (m_isDirty)
            {
                //Debug.Log("m_isDirty is true");
                m_isDirty = false;
                EditorUtility.SetDirty(target);
                EditorSceneManager.MarkSceneDirty(m_target.gameObject.scene);
            }

            // updates a RenderMode Screen Space UIDialog if the GameView or SceneView is resized
            m_invalidatePreview |= m_target.uiDialog && m_target.uiDialog.GetComponent<RectTransform>().sizeDelta != s_previewDialogSizeDelta;

            if (m_invalidatePreview)
            {
                m_invalidatePreview = false;
                UpdatePreviewTexture();
            }
        }

        private bool m_firstSceneGUI = true;
        //NOTE: GetHandleSize && WorldToGUIPoint is different in Repaint and Layout
        private float m_handleSize;
        private Vector2 m_baseGUIPosition;
        private void OnSceneGUI()
        {
            if (m_target.ActiveConversation == null)
                return;
            Event e = Event.current;

            // FIX lagging drag in Unity 2017.3.1f1
            if (e.isMouse)
                SceneView.currentDrawingSceneView.Repaint();

            if (e.type == EventType.ValidateCommand || e.type == EventType.ExecuteCommand)
            {
                //Avoid crash when using Undo/Redo
                if (e.commandName == "UndoRedoPerformed")
                {
                    if (m_selectedDialog != null && !m_target.DialogList.Contains(m_selectedDialog))
                    {
                        m_dicDialogActionReordList.Remove(m_selectedDialog);
                        SetSelectedDialog(m_target.ActiveConversation.StartDialog);
                    }
                    //return; //commented to fi undo/redo text changes (it looks like it is not crashing)
                }
            }
            if (e.type == EventType.Repaint) // NOTE: using Layout will change the m_handleSize when pressing Ctrl+C
            {
                m_baseGUIPosition = HandleUtility.WorldToGUIPoint(m_target.transform.position);
                float handleSize = HandleUtility.GetHandleSize(m_target.transform.position);
                if (handleSize != m_handleSize)
                {
                    m_handleSize = handleSize;
                    SceneView.RepaintAll(); //Fix initial height of dialogActions
                }
                if (m_firstSceneGUI)
                {
                    m_firstSceneGUI = false;
                    m_zoomFactor = m_handleSize;
                    SceneView.RepaintAll(); //Fix initial height of dialogActions
                }
            }
            if (m_handleSize == 0f)
                m_zoomFactor = m_handleSize = 1f;
            KeepValidDialogValues();
            serializedObject.Update();

            float scale = m_zoomFactor / m_handleSize;
            Vector2 newDialogPos = (e.mousePosition - m_baseGUIPosition) / scale;

            if (e.type == EventType.MouseDown)
            {
                if (e.button == 1)
                {
                    //Fix Hand tool stuck bug
                    if (Tools.current == Tool.View)
                        Tools.current = Tool.None;

                    if (m_mouseOverDialog == null)
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Add Dialog"), false, () => { DoAddNewDialog(newDialogPos); });
                        menu.AddItem(new GUIContent("Reset Zoom"), false, () => { m_zoomFactor = m_handleSize; });
                        menu.ShowAsContext();
                    }
                    else
                    {
                        SetSelectedDialog(m_mouseOverDialog);
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Remove Dialog"), false, () => { DoRemoveSelectedDialog(); });
                        menu.AddItem(new GUIContent("Duplicate Dialog"), false, () => { DoDuplicateSelectedDialog(); });
                        menu.AddSeparator("");
                        if (m_target.ActiveConversation.StartDialog != m_selectedDialog)
                            menu.AddItem(new GUIContent("Set Start Dialog"), false, () => { DoSetSelectedDialogAsStartDialog(); });
                        else
                            menu.AddDisabledItem(new GUIContent("Set Start Dialog"));
                        menu.ShowAsContext();
                    }
                }
            }
            if (e.type == EventType.MouseUp || e.type == EventType.MouseDown)
            {
                m_resizedDialog = null;
                m_draggedDialog = null;
            }

            DoSceneViewConversationActionWindow();

            foreach (Dialog dialog in m_target.DialogList)
            {
                if (e.type != EventType.Layout || m_firstSceneGUI //NOTE: mouse and key events need to be processed or GUI texts actions like selecting text, tabs, etc will fail
                    || m_selectedDialog == dialog || m_mouseOverDialog == dialog || m_draggedDialog == dialog || m_resizedDialog == dialog
                    || e.type == EventType.MouseDrag && e.control)
                    DrawDialog(m_baseGUIPosition, dialog);
            }

            // fix dragging scene camera when hand tool is active
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                if (m_draggedDialog != null)
                {
                    e.Use();
                }
            }
        }

        private void DoSceneViewConversationActionWindow()
        {
            Vector2 viewSize = SceneView.currentDrawingSceneView.position.size;
            const string titleUp = "▲▲▲ Conversation Actions ▲▲▲";
            const string titleDown = "▼▼▼ Conversation Actions ▼▼▼";
            float convActionPosY = Mathf.Lerp(18f, 110f, m_sceneConvActionsAnimfoldout.faded);
            Rect rect = new Rect(viewSize.x - 214f, viewSize.y - convActionPosY, 212f, convActionPosY);
            GUILayout.Window(0, rect,
                (id) =>
                {
                    Event e = Event.current;
                    Rect localRect = new Rect(Vector2.zero, rect.size);
                    if (e.type == EventType.Repaint)
                    {
                        GUI.color = new Color(.3f, .3f, .3f, .9f);
                        GUI.skin.window.Draw(localRect, GUIContent.none, 0);
                        GUI.color = Color.white;
                    }
                    Rect headerRect = GUILayoutUtility.GetRect(localRect.width, 18f);
                    GUI.color = new Color(.6f, .6f, .6f, 1f);
                    EditorGUI.DrawRect(new Rect(localRect.x + 1f, localRect.y + 16f, localRect.width, localRect.height - 16f), Color.black * 0.5f);
                    GUI.Label(headerRect, new GUIContent(m_sceneConvActionsAnimfoldout.faded < .5f ? titleUp : titleDown), DialogEditorStyles.dialogHeader);
                    if (e.type == EventType.MouseDown && e.button == 0 && e.mousePosition.y < 20f)
                        m_sceneConvActionsAnimfoldout.target = !m_sceneConvActionsAnimfoldout.target;                    
                    DisplayConversationActions();
                    if (e.type == EventType.Layout && localRect.Contains(e.mousePosition))
                    {
                        HandleUtility.AddDefaultControl(0);
                    }
                    GUI.color = GUI.contentColor = GUI.backgroundColor = Color.white;
                },
                GUIContent.none, GUIStyle.none);
        }

        private void KeepValidDialogValues()
        {
            if (m_target.ActiveConversation != null)
            {
                if (m_target.DialogList.Count == 0)
                    m_target.AddDialog();
                if (m_target.ActiveConversation.StartDialog == null)
                {
                    m_target.ActiveConversation.StartDialog = (m_target.DialogList[0]);
                }
            }
        }

        void DisplayConversationActions()
        {
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Start Conversation"))
                {
                    m_target.StartConversation();
                }
            }
            else
            {
                if (GUILayout.Button("Add Dialog"))
                {
                    DoAddNewDialog((m_selectedDialog ?? m_target.ActiveConversation.StartDialog).position + new Vector2(100f, -250f));
                }
                if (m_selectedDialog != null)
                {
                    if (m_target.DialogList.Count > 1 && GUILayout.Button("Remove Dialog"))
                    {
                        DoRemoveSelectedDialog();
                    }
                    GUI.enabled = m_target.ActiveConversation.StartDialog != m_selectedDialog;
                    if (GUILayout.Button("Set Start Dialog"))
                    {
                        DoSetSelectedDialogAsStartDialog();
                    }
                    GUI.enabled = true;
                }
            }
            if (GUILayout.Button("Reset Zoom"))
            {
                m_zoomFactor = m_handleSize;
            }
        }

        private void DoAddNewDialog(Vector2 position)
        {
            Undo.RecordObject(target, "Add New Dialog");
            Dialog newDialog = m_target.AddDialog();
            if (m_selectedDialog != null)
            {
                newDialog.portrait = m_selectedDialog.portrait;
                newDialog.name = m_selectedDialog.name;
                newDialog.inheritName = m_selectedDialog.inheritName;
                newDialog.color = m_selectedDialog.color;
            }
            newDialog.position = position;

            serializedObject.Update();
            SetSelectedDialog(newDialog);
            SceneView.RepaintAll();
            m_isDirty = true;
            m_invalidatePreview = true;
        }        

        private void DoRemoveSelectedDialog()
        {
            Undo.RecordObject(target, "Remove Dialog");
            m_target.DialogList.Remove(m_selectedDialog);
            m_dicDialogActionReordList.Remove(m_selectedDialog);
            foreach (Dialog dialog in m_target.DialogList)
            {
                foreach (Dialog.DialogAction dialogAction in dialog.dialogActions)
                {
                    if (dialogAction.targetDialogId == m_selectedDialog.id)
                        dialogAction.targetDialogId = -1;
                }
            }
            serializedObject.Update();
            m_dicDialogActionReordList.Clear(); //the property index has been invalidated after removing an element
            SetSelectedDialog(null);
            m_isDirty = true;
            m_invalidatePreview = true;
        }

        private void DoDuplicateSelectedDialog()
        {
            DoAddNewDialog(m_selectedDialog.position + new Vector2(100f, -100f));
        }

        private void DoSetSelectedDialogAsStartDialog()
        {
            Undo.RecordObject(target, "Set Start Dialog");
            m_target.ActiveConversation.StartDialog = m_selectedDialog;
            serializedObject.Update();            
            SceneView.RepaintAll();
            m_isDirty = true;
            m_invalidatePreview = true;
        }

        static Texture2D s_previewTexture;
        static Vector2 s_previewDialogSizeDelta;
        static float s_previewZoomScale = 1f;
        private void UpdatePreviewTexture()
        {
            s_previewTexture = m_target.uiDialog && m_selectedDialog != null ? CreateDialogPreviewTexture(m_target.uiDialog, m_selectedDialog) : null;
        }
        private void DoDialogInspectorView(Dialog dialog)
        {
            EditorGUI.BeginChangeCheck();
            if (s_previewTexture)
            {
                Vector2 maxPreviewSize = new Vector2(EditorGUIUtility.currentViewWidth - 50f, 128 * s_previewZoomScale);
                Vector2 previewSize = new Vector2(s_previewTexture.width, s_previewTexture.height);
                if (previewSize.x > maxPreviewSize.x)
                {
                    previewSize.y = maxPreviewSize.x * previewSize.y / previewSize.x;
                    previewSize.x = maxPreviewSize.x;
                }
                if (previewSize.y > maxPreviewSize.y)
                {
                    previewSize.x = maxPreviewSize.y * previewSize.x / previewSize.y;
                    previewSize.y = maxPreviewSize.y;
                }
                GUILayout.Label("UIDialog Preview ", EditorStyles.boldLabel);

                s_previewZoomScale = EditorGUILayout.Slider("Preview Zoom", s_previewZoomScale, 1f, 4f);
                GUILayout.Box(s_previewTexture, GUILayout.Width(previewSize.x), GUILayout.Height(previewSize.y));
            }
            Vector2 dialogPos = EditorGUILayout.Vector2Field("Position", dialog.position);
            if(dialog.position != dialogPos)
            {
                Undo.RecordObject(target, "Dialog Position");
                dialog.position = dialogPos;
            }

            Vector2 dialogSize = EditorGUILayout.Vector2Field("Size", dialog.rect.size);
            if(dialogSize != dialog.rect.size)
            {
                Undo.RecordObject(target, "Dialog Size");
                dialog.rect.size = dialogSize;
            }
            Color color = EditorGUILayout.ColorField("Color", dialog.color);
            if(color != dialog.color)
            {
                Undo.RecordObject(target, "Dialog Color");
                dialog.color = color;
            }
            EditorGUILayout.PrefixLabel("Portrait");
            float portraitAspect = dialog.portrait ? dialog.portrait.textureRect.height / dialog.portrait.textureRect.width : 1f;
            Sprite dialogPortrait = (Sprite)EditorGUILayout.ObjectField(dialog.portrait, typeof(Sprite), false, GUILayout.Width(64f), GUILayout.Height(64f * portraitAspect));
            if(dialog.portrait != dialogPortrait)
            {
                Undo.RecordObject(target, "Dialog Portrait");
                dialog.portrait = dialogPortrait;
            }
            EditorGUILayout.Space();
            EditorGUIUtility.labelWidth = 160f;
            bool inheritName = EditorGUILayout.Toggle("Inherit Conversation Name", dialog.inheritName);
            EditorGUIUtility.labelWidth = 0f;
            if (dialog.inheritName != inheritName)
            {
                Undo.RecordObject(target, "Dialog InheritName");
                dialog.inheritName = inheritName;
                if (dialog.inheritName)
                    dialog.name = m_target.ActiveConversation.name;
            }
            m_overrideNameAnimToggle.target = !dialog.inheritName;
            if (EditorGUILayout.BeginFadeGroup(m_overrideNameAnimToggle.faded))
            {
                EditorGUILayout.PrefixLabel("Name");
                string dialogName = EditorGUILayout.TextField(dialog.name, DialogEditorStyles.textArea);
                if (dialogName != dialog.name)
                {
                    Undo.RecordObject(target, "Dialog Name");
                    dialog.name = dialogName;
                }
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.Space();
            dialog.rect.size = new Vector2(Mathf.Max(k_minDialogWidth, dialog.rect.size.x), Mathf.Max(k_minDialogHeight, dialog.rect.size.y));
            serializedObject.Update();
            m_dialogSentencesRList.displayRemove = m_dialogSentencesRList.count > 1;
            m_dialogSentencesRList.DoLayoutList();
            m_dialogActionRList.guiMode = DialogActionReorderableList.eGUIMode.Inspector;
            m_dialogActionRList.DoLayoutList();
            EditorGUI.indentLevel++;
            s_foldoutDialogEvents = EditorGUILayout.Foldout(s_foldoutDialogEvents, "Dialog Events", DialogEditorStyles.boldFoldout);
            if (s_foldoutDialogEvents)
            {
                EditorGUILayout.PropertyField(m_selectedDialogProp.FindPropertyRelative("onEnter"), true);
                EditorGUILayout.PropertyField(m_selectedDialogProp.FindPropertyRelative("onExit"), true);
            }
            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                m_isDirty = true;
                m_invalidatePreview = true;
            }
        }

        private Texture2D CreateDialogPreviewTexture(UIDialog uiDialog, Dialog dialog)
        {
            if (!uiDialog)
                return null;
            Camera previewCamera = Camera.main ? Camera.main : Object.FindObjectOfType<Camera>();
            previewCamera = GameObject.Instantiate(previewCamera);
            GameObject dialogObj = GameObject.Instantiate(uiDialog.gameObject);
            Texture2D outputTexture = null;
            try
            {
                dialogObj.gameObject.SetActive(true); //in case uiDialog was not active
                                                      //NOTE: if there is no EventSystem in the scene, this will crash when previewCamera.Render(); is called after stopping the game play
                EventSystem eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
                if (eventSystem == null)
                {
                    GameObject gameObject = new GameObject("EventSystem");
                    GameObjectUtility.SetParentAndAlign(gameObject, null);
                    eventSystem = gameObject.AddComponent<EventSystem>();
                    gameObject.AddComponent<StandaloneInputModule>();
                    Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
                }

                Canvas canvas = dialogObj.GetComponent<Canvas>();
                CanvasScaler canvasScaler = dialogObj.GetComponent<CanvasScaler>();
                RectTransform rectTramsform = dialogObj.GetComponent<RectTransform>();
                RectTransform originalRectTransform = uiDialog.GetComponent<RectTransform>();
                rectTramsform.pivot = new Vector2(.5f, .5f); //fix offset displacement in preview
                s_previewDialogSizeDelta = originalRectTransform.sizeDelta;

                UIDialog uiDialogPreview = dialogObj.GetComponent<UIDialog>();
                uiDialogPreview.SetupDialog(dialog);
                uiDialogPreview.SkipToTheLastSentence();
                // TODO: try to create a preview scene see: internal class PreviewScene and PreviewRenderUtility
                dialogObj.layer = 31;
                previewCamera.cullingMask = 1 << 31;// dialogObj.layer;
                previewCamera.transform.position = new Vector3(dialogObj.transform.position.x, dialogObj.transform.position.y, -10);
                Vector2 size;
                if (canvas.renderMode != RenderMode.WorldSpace)
                {
                    Canvas originalCanvas = uiDialog.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = previewCamera;

                    // NOTE: CanvasScaler modifies the scaleFactor of the Canvas during the Update or OnEnable message 
                    // by reading the Screen.width and Screen.height that here is the size of the Inspector View, so 
                    // the original values needs to be taken. (This could make prefabs UIDialogs to keep the last updated scale)
                    if (canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                        canvas.scaleFactor = DialogEditorUtils.HandleScaleWithScreenSize(canvasScaler, previewCamera.pixelRect.size);
                    else
                        canvas.scaleFactor = originalCanvas.scaleFactor;

                    //NOTE: this is not working in Unity2017 and I think is is not really needed to be set
                    //canvas.referencePixelsPerUnit = originalCanvas.referencePixelsPerUnit;
                    //rectTramsform.sizeDelta = originalRectTransform.sizeDelta;

                    //previewCamera.Render(); //Not sure if this is better than above lines (it respect better the real size when using prefabs and scene objects)                

                    size = previewCamera.pixelRect.size;
                }
                else
                {
                    previewCamera.Render(); // force an update of all RectTransforms. This is needed for example when using a Content Size Fitter
                    /* This was used before, to get the rect enclosing all child elements as well, but it doesn't work well for example if you have big panel to prevent outside the dialog. 
                        * The preview will make this dialog to be very small.
                    Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(dialogObj.GetComponentInChildren<CanvasRenderer>().transform);
                    Vector2 rectSize = bounds.size;
                    */
                    Vector2 rectSize = rectTramsform.sizeDelta; // Only the elements inside the parent RectTransform will be visible
                    size = new Vector2(512f, 512f * rectSize.y / rectSize.x);
                    previewCamera.orthographicSize = (rectSize.y * dialogObj.transform.localScale.y) / 2f;
                }

                RenderTexture rendTextr = new RenderTexture((int)size.x, (int)size.y, 32, RenderTextureFormat.ARGB32);
                rendTextr.Create();

                RenderTexture savedActiveRT = RenderTexture.active;
                RenderTexture savedCamTargetTexture = previewCamera.targetTexture;
                bool savedIsOrthographic = previewCamera.orthographic;
                RenderTexture.active = rendTextr;
                previewCamera.targetTexture = rendTextr;
                previewCamera.orthographic = true;
                previewCamera.backgroundColor = Color.black;
                previewCamera.clearFlags = CameraClearFlags.SolidColor;
                previewCamera.Render();
                outputTexture = new Texture2D((int)size.x, (int)size.y, TextureFormat.ARGB32, false);
                //outputTexture.filterMode = FilterMode.Point;
                outputTexture.ReadPixels(new Rect(0, 0, (int)size.x, (int)size.y), 0, 0);
                outputTexture.Apply();
                previewCamera.targetTexture = savedCamTargetTexture;
                previewCamera.orthographic = savedIsOrthographic;
                RenderTexture.active = savedActiveRT;

                Object.DestroyImmediate(rendTextr);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                Object.DestroyImmediate(previewCamera.gameObject);
                Object.DestroyImmediate(dialogObj);
            }

            return outputTexture;

        }        

        private ReorderableList CreateConversationsRList()
        {
            ReorderableList reordList = new ReorderableList(m_target.conversations, typeof(ConversationData));
            reordList.drawHeaderCallback += (rect) => EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Conversations", EditorStyles.boldLabel);
            reordList.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                GUI.Label(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight), index.ToString());
                ConversationData conversationData = reordList.list[index] as ConversationData;                
                {
                    EditorGUIUtility.labelWidth = 40f;
                    GUI.Label(new Rect(rect.x, rect.y, rect.x + 30f, EditorGUIUtility.singleLineHeight), index.ToString() + ": ");
                    string name = EditorGUI.TextField(new Rect(rect.x + 30f, rect.y, rect.width - 30f, EditorGUIUtility.singleLineHeight), conversationData.name);
                    if(!name.Equals(conversationData.name))
                    {
                        Undo.RecordObject(target, "Changed Conversation Name");
                        conversationData.name = name;
                    }
                    EditorGUIUtility.labelWidth = 0f;
                }
            };
            reordList.onAddCallback += (list) =>
            {
                Undo.RecordObject(target, "Add Conversation");
                ReorderableList.defaultBehaviours.DoAddButton(list);
                serializedObject.Update();                
            };
            reordList.onRemoveCallback += (list) =>
            {
                Undo.RecordObject(target, "Remove Conversation");
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                serializedObject.Update();
                m_dicDialogActionReordList.Clear(); // NOTE: dialogAction.serializedProperty is invalidated because the path index is no valid anymore                
            };
            reordList.onChangedCallback += (list) =>
            {
                reordList.onSelectCallback(list);
            };
            reordList.onReorderCallback += (list) =>
            {
                m_dicDialogActionReordList.Clear(); // NOTE: dialogAction.serializedProperty is invalidated because the path index is no valid anymore                
            };
            reordList.onSelectCallback += (ReorderableList list) =>
            {
                m_target.ActiveConversationIndex = list.index;
                UpdateConversationProperties();              
            };
            return reordList;
        }

        private ReorderableList CreateDialogSentencesRList()
        {
            ReorderableList reordList = new ReorderableList(serializedObject, m_selectedDialogProp.FindPropertyRelative("m_sentences"));
            reordList.drawHeaderCallback += (rect) => EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Dialog Sentences", EditorStyles.boldLabel);
            reordList.elementHeightCallback += (index) => 
            {
                return EditorGUI.GetPropertyHeight(reordList.serializedProperty.GetArrayElementAtIndex(index));
                //return 100f;
            };
            reordList.drawElementCallback += (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.height = EditorGUI.GetPropertyHeight(reordList.serializedProperty.GetArrayElementAtIndex(index));
                EditorStyles.textArea.fontStyle = FontStyle.Bold;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, rect.height), reordList.serializedProperty.GetArrayElementAtIndex(index));
                EditorStyles.textArea.fontStyle = FontStyle.Normal;
            };
            return reordList;
        }

        private DialogActionReorderableList CreateDialogActionRList(Dialog dialog)
        {
            if (dialog != null)
            {
                int dialogIdx = m_target.DialogList.IndexOf(dialog);
                if (dialogIdx < 0)
                    return null;
                SerializedProperty dialogProp = m_dialogListProp.GetArrayElementAtIndex(dialogIdx);
                DialogActionReorderableList reordList = new DialogActionReorderableList(serializedObject, dialogProp, dialog);
                return reordList;
            }
            return null;
        }

        private Dictionary<Dialog, DialogActionReorderableList> m_dicDialogActionReordList = new Dictionary<Dialog, DialogActionReorderableList>();
        private DialogActionReorderableList GetDialogActionReordList(Dialog dialog)
        {
            DialogActionReorderableList reordList = null;
            if (dialog != null && !m_dicDialogActionReordList.TryGetValue(dialog, out reordList))
            {
                reordList = CreateDialogActionRList(dialog);
                m_dicDialogActionReordList.Add(dialog, reordList);
            }
            return reordList;
        }

        private Rect CalculateScreenDialogRect(Vector2 basePos, Dialog dialog, bool scaled = true)
        {
            float scale = scaled? ClampedScale : 1f;
            Rect dlgRect = new Rect(basePos + dialog.position * scale, dialog.rect.size * scale);
            dlgRect.position -= dlgRect.size / 2f; //center dialog
            ReorderableList actionReordList = GetDialogActionReordList(dialog);
            dlgRect.width = Mathf.RoundToInt(dlgRect.width);
            dlgRect.height = Mathf.RoundToInt(dlgRect.height + actionReordList.GetHeight() * scale);
            dlgRect.x = Mathf.RoundToInt(dlgRect.x);
            dlgRect.y = Mathf.RoundToInt(dlgRect.y);
            return dlgRect;
        }

        private struct bezierLine_s
        {
            public Vector3 startPosition, endPosition, startTangent, endTangent;
            public Color lineColor;
            public bezierLine_s(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, Color lineColor)
            {
                this.startPosition = startPosition; this.endPosition = endPosition; this.startTangent = startTangent; this.endTangent = endTangent;
                this.lineColor = lineColor;
            }
        }
        
        private void DrawDialog(Vector2 basePos, Dialog dialog)
        {
            Rect scrDlgRect = CalculateScreenDialogRect(basePos, dialog, true);

            if (dialog == m_target.ActiveConversation.StartDialog)
            {
                Vector3 worldBasePos = HandleUtility.GUIPointToWorldRay(basePos).GetPoint(1);
                Vector3 worldDialogPos = HandleUtility.GUIPointToWorldRay(scrDlgRect.center).GetPoint(1);
                Handles.DrawBezier(worldBasePos, worldDialogPos, worldBasePos, worldBasePos, Color.white * 0.9f, null, 8f);
            }

            Event e = Event.current;

            //Set mouseOver Dialog
            if (scrDlgRect.Contains(e.mousePosition)) m_mouseOverDialog = dialog;
            else if (m_mouseOverDialog == dialog) m_mouseOverDialog = null;


            if (dialog.inheritName && !dialog.name.Equals(m_target.ActiveConversation.name))
            {
                dialog.name = m_target.ActiveConversation.name;
                serializedObject.Update();
                m_isDirty = true;
                m_invalidatePreview = true;
            }

            float scale = ClampedScale;
            Vector2 savedMousePos = e.mousePosition;
            Handles.BeginGUI();
            GUIScaleUtils.BeginNoClip();
            //GUI.BeginGroup(new Rect(0f, 0f, scrDlgRect.width / scale, scrDlgRect.height / scale)); //NOTE: AddCursorRect is not working when using a Group, neither actionList '+' button
            Matrix4x4 savedMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(scrDlgRect.position, Quaternion.identity, Vector3.one);
            /*
            Matrix4x4 matrix = GUI.matrix;
            Vector2 vector = scrDlgRect.position;
            Matrix4x4 lhs = Matrix4x4.TRS(vector, Quaternion.identity, new Vector3(scale, scale, 1f)) * Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
            GUI.matrix = lhs * matrix;
            */
            // NOTE: oreviously to use BeginNoClip, this was having issues with rendering GUI element displaced respect the input area
            EditorGUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);

            //Fix Mouse position that is displaced in Y for using this method to scale the GUI
            e.mousePosition = (savedMousePos - scrDlgRect.position) / scale;

            //Draw Dialog
            DrawDialogWindowEditable(dialog, basePos, scrDlgRect);

            //restore mouse position
            e.mousePosition = savedMousePos;

            GUI.matrix = savedMatrix;
            //GUI.EndGroup();
            GUIScaleUtils.RestoreClips();
            Handles.EndGUI();
        }

        private static Vector2[] s_resizeSide = new Vector2[]
        {
            Vector2.left,Vector2.right,Vector2.up,Vector2.down,Vector2.up + Vector2.left,Vector2.down + Vector2.right,Vector2.up + Vector2.right,Vector2.down + Vector2.left,
        };

        private static MouseCursor[] s_resizeCursor = new MouseCursor[]
        {
            MouseCursor.ResizeHorizontal, MouseCursor.ResizeVertical, MouseCursor.ResizeUpLeft, MouseCursor.ResizeUpRight,
        };
        private void DrawDialogWindowEditable(Dialog dialog, Vector2 basePos, Rect scrDlgRect)
        {
            // Window Content
            DrawDialogContent(dialog, basePos, scrDlgRect);            
        }

        // The dialog contect is positions based on position (0, 0) not scrDlgRect. 
        // GUI.matrix will transform all the gui elements according to position and scale
        Rect DrawDialogContent(Dialog dialog, Vector2 basePos, Rect scrDlgRect)
        {
            DialogActionReorderableList actionReordList = GetDialogActionReordList(dialog);
            float scale = ClampedScale;
            Rect dialogRect = dialog.rect;
            const float titleBarHeight = 16f;
            float portraitWidth = Mathf.Min(64, dialog.width / 4f);
            float portraitAspect = dialog.portrait ? dialog.portrait.textureRect.height / dialog.portrait.textureRect.width : 1f;
            float portraitHeight = portraitWidth * portraitAspect;
            Rect headerRect = new Rect(0f, 0f, dialog.width, titleBarHeight);
            Rect portraitRect = new Rect(10, titleBarHeight + 2f, portraitWidth, portraitHeight);
            if (portraitRect.yMax > dialog.height)
            {
                portraitRect.yMax = dialog.height;
                portraitRect.width = portraitRect.height / portraitAspect;
            }
            Rect textRect = new Rect(portraitRect.xMax + 10, portraitRect.y, dialog.width - portraitRect.width - 30, dialog.height - 25f);
            Rect actionsRect = new Rect(10, dialog.height, dialog.width - 20, actionReordList.GetHeight());

            Rect localRect = new Rect(Vector2.zero, new Vector2(dialog.width, dialog.height + actionsRect.height));
            Event e = Event.current;
            bool isMouseInside = localRect.Contains(e.mousePosition);
            if (e.type == EventType.Layout && isMouseInside)
            {
                HandleUtility.AddDefaultControl(0);
            }

            if(e.type == EventType.MouseDown && isMouseInside)
            {
                if (e.mousePosition.y < headerRect.yMax)
                    m_draggedDialog = dialog;
                else
                    m_draggedDialog = null;
                SetSelectedDialog(dialog);
                HandleUtility.Repaint();
                Repaint();
            }
            if(e.type == EventType.MouseDrag && e.button == 0)
            {
                if (m_draggedDialog == dialog || e.control)
                {
                    dialogRect.position += e.delta;                    
                    Repaint();
                }
            }
            

            EditorGUI.BeginChangeCheck();
            {                
                if (m_selectedDialog == dialog)
                {
                    EditorGUI.DrawRect(localRect, dialog.color);
                }
                else
                {
                    EditorGUI.DrawRect(localRect, dialog.color * 0.9f);
                }
                GUI.color = DialogColorUtils.GetColorWithHigherContrast(dialog.color, DialogEditorStyles.c_almostBlack, DialogEditorStyles.c_almostWhite);
                EditorGUI.LabelField(headerRect, dialog.id + ":" + dialog.name, DialogEditorStyles.dialogHeader);
                GUI.color = Color.white;

                //NOTE: the '+' and '-' buttons were failing sometimes when this was not on top
                actionReordList.guiMode = DialogActionReorderableList.eGUIMode.Scene;
                actionReordList.showDefaultBackground = false;
                actionReordList.DoList(actionsRect);

                Sprite dialogPortrait = (Sprite)EditorGUI.ObjectField(portraitRect, dialog.portrait, typeof(Sprite), false);
                if (dialogPortrait != dialog.portrait)
                {
                    Undo.RecordObject(target, "Dialog Portrait");
                    dialog.portrait = dialogPortrait;
                }

                //scale the text font according to the scale
                //DialogEditorStyles.textArea.fontSize = Mathf.RoundToInt(Mathf.Clamp(Mathf.Lerp(12, 8, (1f / scale - 1f) / .25f), 8, 12));

                string dialogText = DialogEditorUtils.ScrollableTextAreaInternal(textRect, dialog.GetSentence(0), ref dialog.textScrollPos, DialogEditorStyles.textArea);
                if (dialogText != dialog.GetSentence(0))
                {
                    Undo.RecordObject(target, "Dialog Text");
                    dialog.SetSentence(0, dialogText);
                }
                DialogEditorStyles.textArea.fontSize = 0;

                if (GUI.changed)
                    serializedObject.ApplyModifiedProperties();

                //Draw action/dialog connections
                for (int actionIdx = 0; actionIdx < dialog.dialogActions.Count; ++actionIdx)
                {
                    Dialog.DialogAction dlgAction = dialog.dialogActions[actionIdx];
                    Dialog targetDialog = m_target.FindDialogById(dlgAction.targetDialogId);
                    Vector2 lineStart = actionReordList.GetConnectTogglePosition(actionIdx);
                    Vector2 lineEnd = lineStart;
                    Color lineColor = Color.black;
                    bool isLineOnTheLeft = false;
                    bool drawLine = false;
                    if (targetDialog != null)
                    {
                        if (targetDialog.position.x < dialog.position.x)
                        {
                            isLineOnTheLeft = true;
                            lineStart.x = 10f;
                        }

                        Rect targetDialogRect = CalculateScreenDialogRect(Vector2.zero, targetDialog, false);
                        targetDialogRect.position -= dialog.rect.position;
                        
                        bool insideTargetRect = !DialogEditorUtils.LineRectIntersection(lineStart, targetDialogRect.center, targetDialogRect, out lineEnd);
                        if (insideTargetRect)
                            lineEnd = targetDialogRect.center;
                        lineColor = new Color(1f, 1f, 1f, 0.8f);
                        drawLine = true;
                    }
                    if (actionReordList.DraggedActionLinkIndex == actionIdx)
                    {
                        int prevTargetId = dlgAction.targetDialogId;
                        if (m_mouseOverDialog != null && m_mouseOverDialog != dialog)
                            dlgAction.targetDialogId = m_mouseOverDialog.id;
                        else
                            dlgAction.targetDialogId = -1;
                        GUI.changed |= dlgAction.targetDialogId != prevTargetId;
                        if (dlgAction.targetDialogId < 0)
                        {
                            lineEnd = Event.current.mousePosition;
                            if (lineEnd.x < lineStart.x - dialog.rect.width)
                            {
                                isLineOnTheLeft = true;
                                lineStart.x -= dialog.rect.width - 25f;
                            }
                            lineColor = new Color(1f, 1f, 0f, 0.8f);
                            drawLine = true;
                        }
                    }
                    if (drawLine)
                    {
                        float tan0Size = isLineOnTheLeft ? -100f : +100f;
                        Vector2 vTan0 = lineStart + new Vector2(tan0Size, 0);
                        tan0Size = Mathf.Min(100f, Vector2.Distance(lineStart, lineEnd) - 15f);
                        tan0Size = isLineOnTheLeft ? -tan0Size : tan0Size;
                        vTan0 = lineStart + new Vector2(tan0Size, 0);

                        Handles.DrawBezier(lineStart, lineEnd, vTan0, lineEnd, lineColor, null, 6f);
                        Vector2 endTan = DialogMathUtils.CalculateBezierTangent(0.70f, lineStart, lineEnd, vTan0, lineEnd);
                        DialogEditorUtils.HandlesDrawArrowEnd(lineEnd, 18f, 12f, endTan, lineColor);

                    }
                }
            }

            Rect[] dialogBorders = new Rect[]
            {
                new Rect(0, 20, 5, scrDlgRect.height - 30), //left
                new Rect(scrDlgRect.width - 7, 20, 5, scrDlgRect.height - 30), //right
                new Rect(10, 0, scrDlgRect.width - 20, 5), //top
                new Rect(10, scrDlgRect.height - 7, scrDlgRect.width - 20 - 60 * scale, 5), //bottom
                new Rect(0, 0, 5, 5), //top left
                new Rect(scrDlgRect.width - 7, scrDlgRect.height - 7, 5, 5), //bottom right
                new Rect(scrDlgRect.width - 7, 0, 5, 5), //top right
                new Rect(0, scrDlgRect.height - 7, 5, 5), //bottom left
            };

            for (int i = 0; i < dialogBorders.Length; ++i)
            {
                //NOTE: GUI.DragWindow should be placed under this in order to receive the EventType.MouseDown event on top
                if (s_resizeSide[i].y > 0f) continue;  //skip top bar (it's better to leave it only for dragging the window)
                
                Rect cursorRect = new Rect(dialogBorders[i].position, dialogBorders[i].size);
#if UNITY_5_3
                cursorRect.position += scrDlgRect.position + Vector2.up * 37f; //NOTE: after using BeginNoClip rects need to be displaced 37 units (the size of the window header including the tab)
#else
                cursorRect.position += Vector2.up * 37f; //NOTE: after using BeginNoClip rects need to be displaced 37 units (the size of the window header including the tab)
                cursorRect.position /= scale;
                cursorRect.size /= scale;
#endif
                EditorGUIUtility.AddCursorRect(cursorRect, s_resizeCursor[i / 2]);
                //fix some bugs where the cursor changes but mouse position is not inside the border rect
                Rect borderRect = dialogBorders[i];
                borderRect.position -= 2f * Vector2.one;
                borderRect.size += 4f * Vector2.one;
                borderRect.position /= scale;
                borderRect.size /= scale;
                //EditorGUI.DrawRect(borderRect, new Color(0.9f, 0f, 0f, 0.5f)); //debug rects                
                if (e.type == EventType.MouseDown && borderRect.Contains(e.mousePosition))
                {
                    m_resizedDialog = dialog;
                    m_resizeSide = s_resizeSide[i];
                    break;
                }
            }

            if (e.type == EventType.MouseDrag && e.button == 0 && m_resizedDialog == dialog)
            {
                Undo.RecordObject(target, "Dialog Size");
                if (m_resizeSide.x > 0f)
                    dialogRect.xMax += e.delta.x;
                if (m_resizeSide.x < 0f)
                    dialogRect.xMin += e.delta.x;
                if (m_resizeSide.y > 0f)
                    dialogRect.yMin += e.delta.y;
                if (m_resizeSide.y < 0f)
                    dialogRect.yMax += e.delta.y;
                e.Use(); // fix dragging scene camera when hand tool is active
                HandleUtility.Repaint();
                Repaint();
            }

            bool dialogRectChanged = dialog.rect != dialogRect;
            if (EditorGUI.EndChangeCheck() || dialogRectChanged)
            {

                serializedObject.ApplyModifiedProperties();
                if (dialogRectChanged)
                {
                    Undo.RecordObject(target, "Dialog Rect");
                    dialog.rect = dialogRect;
                }
                m_isDirty = true;
                m_invalidatePreview = !dialogRectChanged; // no need for dialog rect changes
            }

            return dialogRect;
        }   
    }
}
