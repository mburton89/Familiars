// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;

namespace CreativeSpore.RPGConversationEditor
{
	public class ConditionalDialogDemo : MonoBehaviour 
	{
        [Header("Conditional Dialog Actions")]
        public bool JohannaQuestAchieved = false;
        public bool AskJohannaToJoinTheParty = false;

        /// <summary>
        /// Called from Johanna's conversation dialog action event onPreProcessed will hide or unhide the action according to the state of the mission.
        /// </summary>
        public void DialogActionCheck_JohannaQuestAchieved()
        {
            UIDialog.processedDialogAction.visible = JohannaQuestAchieved;
        }

        public void DialogActionCheck_JohannaWasAskedToJoinTheParty()
        {
            UIDialog.processedDialogAction.visible = AskJohannaToJoinTheParty;
        }

        public void CompleteJohannaQuest()
        {
            JohannaQuestAchieved = true;
        }

        public void CompleteAskJohannaToJoinTheParty()
        {
            AskJohannaToJoinTheParty = true;
        } 

    }
}
