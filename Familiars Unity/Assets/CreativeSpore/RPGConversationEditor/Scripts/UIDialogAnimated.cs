// Copyright (C) 2018 Creative Spore - All Rights Reserved
using UnityEngine;
using System.Collections;

namespace CreativeSpore.RPGConversationEditor
{
    /// <summary>
    /// This class inherites from UIDialog to add support for a dialog open/close animation
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]   
    [AddComponentMenu("RPG Conversation Editor/UIDialogAnimated", 10)]
    public class UIDialogAnimated : UIDialog
    {
        [Header("UIDialogAnimated Properties")]
        /// <summary>
        /// The name of the state with the open animation
        /// </summary>
        public string openDialogState;
        /// <summary>
        /// The name of the state with the close animation
        /// </summary>
        public string closeDialogState;

        private bool m_disableIfAnimationIsOver = false;
        private Animator m_animator;
        protected override void Start()
        {
            base.Start();
            m_animator = GetComponent<Animator>();            
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StopAllCoroutines();
            m_disableIfAnimationIsOver = false;
            m_animator = GetComponent<Animator>();
            if (!string.IsNullOrEmpty(openDialogState))
                m_animator.Play(openDialogState, 0, 0f);
        }

        protected override void Update()
        {
            base.Update();
            if (m_disableIfAnimationIsOver)
            {
                AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName(closeDialogState) && stateInfo.normalizedTime >= 1f && !m_animator.IsInTransition(0))
                {
                    base.DoOnCloseConversation();
                }
            }
        }

        protected override void DoOnCloseConversation()
        {
            if (!string.IsNullOrEmpty(closeDialogState))
            {
                m_isClosing = true;
                m_animator.Play(closeDialogState);
                m_disableIfAnimationIsOver = true;
            }
            else
            {
                base.DoOnCloseConversation();
            }
        }
    }
}
