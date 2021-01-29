// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CreativeSpore.RPGConversationEditor
{
    public static class DialogEditorStyles
    {
        public static readonly Color c_almostBlack = new Color(0.2f, 0.2f, 0.2f, 1f);
        public static readonly Color c_almostWhite = new Color(0.8f, 0.8f, 0.8f, 1f);

        public static GUIStyle textArea = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            fontStyle = FontStyle.Bold,
            //richText = true, //this is not working very well and now using previews it is not necessary
        };

        public static GUIStyle boldTextField = new GUIStyle("TextField")
        {
            fontStyle = FontStyle.Bold,
        };

        public static GUIStyle boldFoldout = new GUIStyle("Foldout")
        {
            fontStyle = FontStyle.Bold,
        };

        public static GUIStyle headerStyle = new GUIStyle(EditorStyles.helpBox)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            normal = { textColor = Color.blue },
        };

        public static GUIStyle richHelpBox = new GUIStyle("HelpBox")
        {
            richText = true,
        };

        public static GUIStyle preButton = "RL FooterButton";

        public static GUIStyle dialogHeader = new GUIStyle("Label")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,   
            normal = { textColor = Color.white },
        };
    }
}
