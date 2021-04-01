// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace CreativeSpore.RPGConversationEditor
{
    [CustomEditor(typeof(ConversationAsset))]
	public class ConversationAssetEditor : Editor 
	{
        [MenuItem("Assets/Create/RPG Conversation Editor/ConversationAsset", priority = 50)]
        public static ConversationAsset CreateAsset()
        {
            return DialogEditorUtils.CreateAssetInSelectedDirectory<ConversationAsset>();
        }
    }
}
