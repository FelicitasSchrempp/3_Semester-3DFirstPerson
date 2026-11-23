using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Ink;
using Ink.Runtime;
using Ink.UnityIntegration;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DialogueController : MonoBehaviour
{
    public static Action<string, int> OnAddState;
    public static Action<string> OnGetState;
    
    private const string SpeakerSeparator = ":";
    private const string EscapedColon = "::";
    private const string EscapedColonPlaceholder = "§";
    
    public static event Action DialogueClosed;

    ///<summary>Generic Ink event supplying an identifier.</summary>
    public static event Action<string> InkEvent;

    #region Inspector

    [Header("Ink")]

    [SerializeField] private InkFile[] inkAssets;

    [SerializeField] private int languageIndex;
    
    [Header("UI")]

    [SerializeField] private DialogueBox dialogueBox;

    [Header("Dialogue")]
    [SerializeField] private UnityEvent onDialogueStarted;
    [SerializeField] private UnityEvent onDialogueEnd;
    
    [Header("Avatar")] 
    [SerializeField] private DialogueAvatar[] avatars;
    
    #endregion

    private Story inkStory;

    //private GameState gameState;
    
    
    #region Unity Event Functions

    private void Awake()
    {
        SetLanguage(languageIndex);
    }
    
    private void OnEnable()
    {
        DialogueBox.DialogueContinued += OnDialogueContinued;
        DialogueBox.ChoiceSelected += OnChoiceSelected;
    }

    private void Start()
    {
        dialogueBox.gameObject.SetActive(false);
        LoadAvatars();
    }

    private void OnDisable()
    {
        DialogueBox.DialogueContinued -= OnDialogueContinued;
        DialogueBox.ChoiceSelected -= OnChoiceSelected;
    }

    private void OnDestroy()
    {
        inkStory.onError -= OnInkError;
    }

    #endregion

    #region Resources

    private void LoadAvatars()
    {
        avatars = Resources.LoadAll<DialogueAvatar>("Dialogue");
    }
    
    #endregion
    
    #region Dialogue Lifecycle

    public void StartDialogue(string dialoguePath)
    {
        onDialogueStarted?.Invoke();

        OpenDialogue();

        // Like '-> knot' in ink.
        inkStory.ChoosePathString(dialoguePath);
        ContinueDialogue();
    }

    private void OpenDialogue()
    {
        dialogueBox.gameObject.SetActive(true);
    }

    private void CloseDialogue()
    {
        EventSystem.current.SetSelectedGameObject(null);
        dialogueBox.gameObject.SetActive(false);

        StartCoroutine(DelayDialogueEndEvent());
        
        DialogueClosed?.Invoke();
    }

    private IEnumerator DelayDialogueEndEvent()
    {
        yield return new WaitForSeconds(0.2f);
        onDialogueEnd?.Invoke();
    }

    private void ContinueDialogue()
    {
        if (IsAtEnd())
        {
            CloseDialogue();
            return;
        }

        DialogueLine line;
        if (CanContinue())
        {
            // Player: I can not do that.
            string inkLine = inkStory.Continue();
            if (string.IsNullOrWhiteSpace(inkLine))
            {
                ContinueDialogue();
                return;
            }
            line = ParseText(inkLine, inkStory.currentTags);
        }
        else
        {
            line = new DialogueLine();
        }

        line.choices = inkStory.currentChoices;

        dialogueBox.DisplayText(line);
    }

    private void OnDialogueContinued(DialogueBox _)
    {
        print("Dialogue Continued");
        ContinueDialogue();
    }

    private void OnChoiceSelected(DialogueBox _, int choiceIndex)
    {
        inkStory.ChooseChoiceIndex(choiceIndex);
        ContinueDialogue();
    }

    #endregion

    #region Ink

    public void SetLanguage(int languageIndex)
    {
        this.languageIndex = languageIndex;
        
        // Initialize Ink.
        inkStory = new Story(inkAssets[languageIndex].storyJson);
        // Add error handling.
        inkStory.onError += OnInkError;
        // Connect an ink function to a C# function.
        inkStory.BindExternalFunction<string>("Event", Event);
        inkStory.BindExternalFunction<string>("Get_State", Get_State, true);
        inkStory.BindExternalFunction<string, int>("Add_State", Add_State);
    }
    
    private DialogueLine ParseText(string inkLine, List<string> tags)
    {
        DialogueLine line = new DialogueLine();
                                // ::          ->    §
        inkLine = inkLine.Replace(EscapedColon, EscapedColonPlaceholder);
                                        //   : 
        List<string> parts = inkLine.Split(SpeakerSeparator).ToList();

        string speaker;
        string text;

        switch (parts.Count)
        {
            case 1:
                speaker = null;
                text = parts[0];
                break;
            case 2:
                speaker = parts[0];
                text = parts[1];
                break;
            default:
                Debug.LogWarning($"Ink dialogue line was split at more {SpeakerSeparator} than expected." +
                                 $" Please make sure to use {EscapedColon} for {SpeakerSeparator} inside text");
                goto case 2;
        }

        line.speaker = speaker?.Trim();
        line.text = text.Trim().Replace(EscapedColonPlaceholder, SpeakerSeparator);
        //parts Element 1 = Hallo:Player
        if (tags.Contains("thought"))
        {
            line.text = $"<i>{line.text}</i>";
        }

        if (parts.Count > 1 )
        {
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i].Contains("avatar"))
                {
                    List<string> avatar = tags[i].Split(SpeakerSeparator).ToList();
                    line.speakerImage = GetAvatar(avatar[1]);
                }
            }

        }
        return line;
    }

    Sprite GetAvatar(string avatarId)
    {
        for (int i = 0; i < avatars.Length; i++)
        {
            if (avatars[i].avatarId == avatarId)
            {
                return avatars[i].avatarSprite;
            }
        }
        return null;
    }

    
    private bool CanContinue()
    {
        return inkStory.canContinue;
    }

    private bool HasChoices()
    {
        return inkStory.currentChoices.Count > 0;
    }

    private bool IsAtEnd()
    {
        return !CanContinue() && !HasChoices();
    }

    private void OnInkError(string message, ErrorType type)
    {
        switch (type)
        {
            case ErrorType.Author:
                break;
            case ErrorType.Warning:
                Debug.LogWarning(message);
                break;
            case ErrorType.Error:
                Debug.LogError(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private void Event(string eventName)
    {
        InkEvent?.Invoke(eventName);
    }

    private void Get_State(string id)
    {
        OnGetState?.Invoke(id);
    }

    private void Add_State(string id, int amount)
    {
        OnAddState?.Invoke(id, amount);
    }

    #endregion
}

public struct DialogueLine
{
    public string speaker;
    public string text;
    public List<Choice> choices;

    // Here we can also add other information like speaker images or sounds.
    public Sprite speakerImage;
}
