// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;

namespace CreativeSpore.RPGConversationEditor
{
	public class SelectiveDialogDemo : MonoBehaviour 
	{
        public SpriteRenderer PlayerSpriteRenderer;

        public void StartSelectiveConversation(ConversationController convCtrl)
        {
            string conversationName = PlayerSpriteRenderer.sprite.name;
            if (convCtrl.FindConversationByName(conversationName) == null)
                conversationName = "Default";
            convCtrl.StartConversation(conversationName);
            // if no portrait is set, overwrite the portrait and name with the current character
            if (!convCtrl.uiDialogInstance.portrait.sprite)
            {
                convCtrl.uiDialogInstance.portrait.sprite = PlayerSpriteRenderer.sprite;
                convCtrl.uiDialogInstance.portrait.gameObject.SetActive(true);
                convCtrl.uiDialogInstance.dialogName.text = PlayerSpriteRenderer.sprite.name;
            }
        }
	}
}
