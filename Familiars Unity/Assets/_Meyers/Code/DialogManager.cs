using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public static DialogManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    Dialog dialog;
    Action onDialogFinished;

    int currentLine = 0;
    bool isTyping;

    GameObject player;

    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialog(Dialog dialog, GameObject player, Action onFinished=null)
    {
        yield return new WaitForEndOfFrame();
        this.player = player;

        OnShowDialog?.Invoke();

        IsShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;

        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    void Update()
    {
        HandleUpdate();
    }

    public void HandleUpdate()
    {
        if (IsShowing)
        {
            if (Input.GetKeyDown(KeyCode.Z) && !isTyping)
            {
                ++currentLine;
                if (currentLine < dialog.Lines.Count)
                {
                    StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
                }
                else
                {
                    currentLine = 0;
                    IsShowing = false;
                    Debug.Log("[DialogManager.cs] Dialog Finished");
                    dialogBox.SetActive(false);
                    onDialogFinished?.Invoke();
                    OnCloseDialog?.Invoke();
                    player.GetComponent<CharacterController>().state = PlayerState.Normal;
                }
            }
        }
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }

    public void StartBattle(FamiliarParty fParty)
    {
        Debug.Log("[DialogManager.cs] Starting Battle After Dialog");
        onDialogFinished = () =>
        {
            Debug.Log("[DialogManager.cs] Dialog Finished, in onDialogFinished Check");
            GameController.Instance.StartTrainerBattle(fParty.familiars);
        };
    }
}
