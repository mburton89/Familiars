using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace CreativeSpore.RPGConversationEditor
{
    /// <summary>
    /// Conversation controller to play and edit conversations in the scene
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("RPG Conversation Editor/Conversation Controller", 10)]
    public class ConversationController : MonoBehaviour
    {
        /// <summary>
        /// When the conversation is saved as an asset, this will reference to that asset.
        /// </summary>
        public ConversationAsset conversationAsset;        

        [Tooltip("The UI Dialog component used to draw this conversation dialogs. It can be a component in the scene or inside a prefab.")]
        public UIDialog uiDialog;

        [Tooltip("The parent gameobject that will be assigned to the UIDialog instance when the conversation starts. Usually used for World Space Canvases.")]
        public Transform uiDialogParent;
        [Tooltip("The local position set to the UIDialog instance. It will be relative to the UIDialogParent object.")]
        public Vector3 uiDialogOffset;

        public enum eDefaultDialog
        {
            /// <summary>
            /// The dialog defined as start dialog.
            /// </summary>
            StartDialog,
            /// <summary>
            /// The dialogs will be selected randomly.
            /// </summary>
            Random,
            /// <summary>
            /// The dialogs will be selected randomly but trying not to repeat the previous dialog.
            /// </summary>
            RandomNonRepeating,
        }
        [Tooltip("The default dialog when no dialog is specified (when dialogId is -1).")]
        public eDefaultDialog defaultDialog = eDefaultDialog.StartDialog;

        /// <summary>
        /// Returns a list with all the conversations.
        /// </summary>
        public List<ConversationData> conversations { get { return conversationAsset ? conversationAsset.conversations : m_conversations; } }
        /// <summary>
        /// Returns a list with all the dialogs in the active conversation.
        /// </summary>
        public List<Dialog> DialogList { get { return ActiveConversation != null ? ActiveConversation.dialogList : null; } }
        /// <summary>
        /// Returns true while a dialog is being played.
        /// </summary>
        public bool IsPlayingDialog { get { return m_uiDialogInstance && m_uiDialogInstance.IsPlaying; } }
        /// <summary>
        /// The UIDialog component used to display the dialog.
        /// </summary>
        public UIDialog uiDialogInstance { get { return m_uiDialogInstance; } }

        /// <summary>
        /// The current active conversation
        /// </summary>
        public ConversationData ActiveConversation
        {
            get
            {
                return GetConversation(m_activeConversationIndex);
            }
            set
            {
                ActiveConversationIndex = conversations.IndexOf(value);
            }
        }

        /// <summary>
        /// The current active conversation index
        /// </summary>
        public int ActiveConversationIndex
        {
            get
            {
                m_activeConversationIndex = Mathf.Clamp(m_activeConversationIndex, -1, conversations.Count - 1);
                return m_activeConversationIndex;
            }
            set
            {
                if (value != m_activeConversationIndex)
                {
                    m_activeConversationIndex = Mathf.Clamp(value, -1, conversations.Count - 1);
                }
            }
        }

        [SerializeField]
        private UIDialog m_uiDialogInstance;
        [SerializeField]
        private int m_activeConversationIndex = 0;
        [SerializeField]
        private List<ConversationData> m_conversations = new List<ConversationData>();

        private void Reset()
        {
            m_activeConversationIndex = 0;
            m_conversations.Add(new ConversationData("New Conversation"));
        }

        private void OnValidate()
        {
            if (conversations.Count == 0)
            {
                m_activeConversationIndex = 0;
                conversations.Add(new ConversationData());
            }
        }

        private void Start()
        {
            ActiveConversationIndex = 0;
        }

        /// <summary>
        /// When using a conversation asset, this will remove the reference and save the conversation data in the ConversationController component.
        /// </summary>
        public void EmbedConversationData()
        {
            if (conversationAsset)
            {
                m_conversations = new List<ConversationData>( conversationAsset.conversations );
                conversationAsset = null;
            }
        }

        /// <summary>
        /// Starts the current active conversation.
        /// </summary>
        public void StartConversation()
        {
            StartConversation(ActiveConversationIndex, null, -1);
        }

        /// <summary>
        /// Starts the first conversation.
        /// </summary>
        /// <param name="onCloseCallback">Callback to be called when the conversation is closed.</param>
        public void StartConversation(UnityAction onCloseCallback)
        {
            StartConversation(ActiveConversationIndex, onCloseCallback, - 1);
        }

        /// <summary>
        /// Starts a conversation by name.
        /// </summary>
        /// <param name="name">Name of the conversation.</param>
        public void StartConversation(string name)
        {
            StartConversation(name, -1);
        }

        /// <summary>
        /// Starts a conversation by index in the conversation list.
        /// </summary>
        /// <param name="index">The index of the conversation.</param>
        public void StartConversation(int index)
        {
            StartConversation(index, -1);
        }

        /// <summary>
        /// Move to the next conversation in the list.
        /// </summary>
        public void MoveToNextConversation()
        {
            ActiveConversationIndex++;
        }

        /// <summary>
        /// Move to the previous conversation in the list.
        /// </summary>
        public void MoveToPreviousConversation()
        {
            ActiveConversationIndex--;
        }

        /// <summary>
        /// Starts a conversation by index.
        /// </summary>
        /// <param name="index">Index of the conversation.</param>
        /// <param name="dialogId">Dialog id where the conversation starts. -1 to get the default dialog.</param>
        public void StartConversation(int index, int dialogId)
        {
            StartConversation(index, null, dialogId);
        }

        /// <summary>
        /// Starts a conversation by name.
        /// </summary>
        /// <param name="name">The name of the conversation.</param>
        /// <param name="dialogId">Dialog id where the conversation starts. -1 to get the default dialog.</param>
        public void StartConversation(string name, int dialogId)
        {
            StartConversation(name, null, dialogId);
        }

        /// <summary>
        /// Starts a conversation by index.
        /// </summary>
        /// <param name="index">Index of the conversation.</param>
        /// <param name="onCloseCallback">Callback to be called when the conversation is closed.</param>
        /// <param name="dialogId">Dialog id where the conversation starts. -1 to get the default dialog.</param>
        public void StartConversation(int index, UnityAction onCloseCallback, int dialogId = -1)
        {
            ConversationData conversation = this.GetConversation(index);
            if (conversation != null)
                StartConversation(conversation, onCloseCallback, dialogId);
            else
                Debug.LogWarning("Conversation " + index + " not found!", this);
        }

        /// <summary>
        /// Starts a conversation by index.
        /// </summary>
        /// <param name="name">The name of the conversation.</param>
        /// <param name="onCloseCallback">Callback to be called when the conversation is closed.</param>
        /// <param name="dialogId">Dialog id where the conversation starts. -1 to get the default dialog.</param>
        public void StartConversation(string name, UnityAction onCloseCallback, int dialogId = -1)
        {
            ConversationData conversation = this.FindConversationByName(name);
            if (conversation != null)
                StartConversation(conversation, onCloseCallback, dialogId);
            else
                Debug.LogWarning("Conversation " + name + " not found!", this);
        }        

        private void StartConversation(ConversationData conversation, UnityAction onCloseCallback = null, int dialogId = -1)
        {
            CacheUIDialogInstance();
            Debug.Assert(m_uiDialogInstance, name + ": UIDialog needs to be defined!", this);
            Debug.Assert(conversation!=null, "conversation is null!", this);
            if (conversation != null && m_uiDialogInstance)
            {
                if (dialogId < 0)
                {
                    if(defaultDialog == eDefaultDialog.Random)
                        dialogId = conversation.GetRandomDialog(false).id;
                    else if (defaultDialog == eDefaultDialog.RandomNonRepeating)
                        dialogId = conversation.GetRandomDialog(true).id;
                }
                m_uiDialogInstance.PlayConversation(conversation, onCloseCallback, dialogId);
                if (uiDialogParent)
                { //NOTE: do this after calling PlayConversation in case the OnExit event of current conversation is changing the transform
                    m_uiDialogInstance.transform.SetParent(uiDialogParent);
                    m_uiDialogInstance.transform.localPosition = uiDialogOffset;
                }
            }
        }

        /// <summary>
        /// Stops the current played conversation
        /// </summary>
        public void StopConversation()
        {
            if (m_uiDialogInstance)
                m_uiDialogInstance.StopConversation();
        }

        /// <summary>
        /// Overrides the start dialog for the current conversation.
        /// </summary>
        public void OverrideStartDialog()
        {
            if(m_uiDialogInstance && m_uiDialogInstance.CurrentConversation != null)
                 m_uiDialogInstance.CurrentConversation.OverrideStartDialog( m_uiDialogInstance.CurrentDialogId);
        }

        /// <summary>
        /// Overrides the parent of the UIDialog instance.
        /// </summary>
        /// <param name="transform"></param>
        public void OverrideUIDialogParent(Transform transform)
        {
            if (m_uiDialogInstance && transform)
                m_uiDialogInstance.transform.position = transform.TransformPoint(uiDialogOffset);
        }

        /// <summary>
        /// Restores the UIDialog parent to the uiDialogParent property.
        /// </summary>
        public void RestoreUIDialogParent()
        {
            OverrideUIDialogParent(uiDialogParent);
        }

        /// <summary>
        /// Finds a conversation by name. Returns null if no conversation was found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ConversationData FindConversationByName(string name)
        {
            return conversations.FirstOrDefault(o => o.name.Equals(name));
        }

        /// <summary>
        /// Returns a conversation by index or null if no conversation was found.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ConversationData GetConversation(int index)
        {
            return index >= 0 && index < conversations.Count ? conversations[index] : null;
        }

        /// <summary>
        /// Adds a dialog to the current active conversation.
        /// </summary>
        /// <returns></returns>
        public Dialog AddDialog()
        {
            return ActiveConversation.AddDialog();
        }

        /// <summary>
        /// Removes a dialog from the current active conversation.
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        public bool RemoveDialog(Dialog dialog)
        {
            return ActiveConversation.RemoveDialog(dialog);
        }

        /// <summary>
        /// Removes a dialog from the current active conversation using the dialog id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool RemoveDialog(int id)
        {
            return ActiveConversation.RemoveDialog(id);
        }

        /// <summary>
        /// Finds a dialog by id in the active conversation.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Dialog FindDialogById(int id)
        {
            return ActiveConversation.FindDialogById(id);
        }

        private static Dictionary<UIDialog, UIDialog> s_dicPrefabToInstance = new Dictionary<UIDialog, UIDialog>();
        private void CacheUIDialogInstance()
        {
            if (uiDialog && !m_uiDialogInstance)
            {
                bool isPrefab = !uiDialog.gameObject.scene.IsValid();
                if (isPrefab)
                {
                    UIDialog dialogInstance;
                    if (!s_dicPrefabToInstance.TryGetValue(uiDialog, out dialogInstance) || !dialogInstance)
                    {
                        dialogInstance = Instantiate(uiDialog);
                        s_dicPrefabToInstance[uiDialog] = dialogInstance;
                    }
                    m_uiDialogInstance = dialogInstance;
                }
                else
                {
                    m_uiDialogInstance = uiDialog;
                }
            }
        }        
    }
}
