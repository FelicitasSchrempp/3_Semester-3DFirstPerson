
using UnityEngine;

public class InkDialogue : MonoBehaviour
{
    #region Inspector

    [Tooltip("Path to a specified knot.stitch in the ink file.")]
    [SerializeField] private string dialoguePath;
    
    #endregion
    
    private DialogueController _dialogueController;

    private void Awake()
    {
        _dialogueController = FindAnyObjectByType<DialogueController>();
    }

    public void StartDialogue(string _dialoguePath)
    {
        if (string.IsNullOrWhiteSpace(_dialoguePath))
        {
            Debug.LogWarning("No dialogue path defined", this);
            return;
        }
        
        dialoguePath = _dialoguePath;
        _dialogueController.StartDialogue(dialoguePath);
    }
    
    public void StartDialogue()
    {
        if (string.IsNullOrWhiteSpace(dialoguePath))
        {
            Debug.LogWarning("No dialogue path defined", this);
            return;
        }
        
        _dialogueController.StartDialogue(dialoguePath);
    }
    
    public void SetDialoguePath(string newDialoguePath)
    {
        dialoguePath = newDialoguePath;
    }
}