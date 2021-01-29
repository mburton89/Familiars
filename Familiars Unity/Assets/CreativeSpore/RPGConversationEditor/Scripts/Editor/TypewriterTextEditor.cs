// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.UI;

namespace CreativeSpore.RPGConversationEditor
{
    [CustomEditor(typeof(TypewriterText))]
    public class TypewriterTextEditor : UnityEditor.UI.TextEditor
    {

        [MenuItem("GameObject/RPG Conversation Editor/UI/TypewriterText", false, 10)]
        public static void AddTypewriterText(MenuCommand menuCommand)
        {
            // Replicates the creation of a Unity Text component and copy all the properties but m_Script
            // into the TypewriterText component
            GameObject go = DialogEditorUtils.MenuOptions_AddText(menuCommand);
            Text comp = go.GetComponent<Text>();
            SerializedObject compSerialized = new SerializedObject(comp);
            DestroyImmediate(comp);
            comp = go.AddComponent<TypewriterText>();
            SerializedObject newCompSerialized = new SerializedObject(comp);
            SerializedProperty prop = compSerialized.GetIterator();

            while (prop.NextVisible(true))
            {
                if (!prop.name.Equals("m_Script"))
                {
                    newCompSerialized.CopyFromSerializedProperty(prop);
                    Debug.Log(prop.propertyPath + " " + prop.name);
                }
            }
            newCompSerialized.ApplyModifiedProperties();
        }

        SerializedProperty m_typingSpeed;
        SerializedProperty m_fillAmount;
        SerializedProperty m_typingSound;
        TypewriterText m_target;

        protected override void OnEnable()
        {
            if (target) //fix some log errors when compiling code while this component in in the scene
            {
                base.OnEnable();
                m_typingSpeed = serializedObject.FindProperty("m_typingSpeed");
                m_fillAmount = serializedObject.FindProperty("m_fillAmount");
                m_typingSound = serializedObject.FindProperty("m_typingSound");
                m_target = target as TypewriterText;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            DoTypewriterInspectorGUI();

            if(GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                if (m_target.TypingSound)
                {
                    AudioSource audioSource = m_target.GetComponent<AudioSource>();
                    if (!audioSource)
                        audioSource = m_target.gameObject.AddComponent<AudioSource>();
                    audioSource.clip = m_target.TypingSound;
                }
                EditorUtility.SetDirty(target);
            }
        }

        public void DoTypewriterInspectorGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Typewriter Text Properties", EditorStyles.boldLabel);
                EditorGUI.indentLevel += 1;

                EditorGUILayout.PropertyField(m_typingSpeed);
                EditorGUILayout.PropertyField(m_fillAmount);
                EditorGUILayout.PropertyField(m_typingSound);

                EditorGUI.indentLevel -= 1;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
