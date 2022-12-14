using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(DialogueAnimator))]
public class DialogueBrain : MonoBehaviour
{

    public static DialogueBrain Instance {get;private set;}

    private void Awake(){
        Instance = this;
    } 

    private void Start(){
        GetDialogueComponents();
    }

    #region Dialogue Script References

    private DialogueAnimator dialogueAnimator;
    private DialogueParser parser = new DialogueParser();

    private void GetDialogueComponents(){
        dialogueAnimator = GetComponent<DialogueAnimator>();
    }

    #endregion

    #region Serialized Varibles

    [Header("Player")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Color playerColor;
    [SerializeField] private string playerName;
    [SerializeField] private List<AudioClip> playerVoice;
    [SerializeField] private TMP_FontAsset playerFont;

    [Header("Dialogue Animation / UI")]
    [SerializeField] private GameObject dialogueTextContainer;
    [field:SerializeField] public TextMeshProUGUI dialogueTextBox {get; private set;}
    [field:SerializeField] public TextMeshProUGUI speakerNameTextBox {get; private set;}
    [SerializeField] private Image speakerImage;
    [SerializeField] private GameObject buttonPromptContainer;
    [SerializeField] private TMP_FontAsset defaultFont;

    [Header("Animation")]
    [SerializeField] private bool titleVar1;
    [field:SerializeField] public TextEntryAnimationType defaultTextEntryAnimation {get; private set;}
    [field:SerializeField] public float charAnimationTime {get; private set;}
    [field:SerializeField] public float defaultCharSpeed {get; private set;}
    [field:SerializeField] public bool waitForBlankSpace {get; private set;}

    [Header("Audio")]
    [SerializeField] private List<AudioClip> dialogueSoundEffects;
    [field:SerializeField] public float voiceMinPitch {get; private set;} = 1;
    [field:SerializeField] public float voiceMaxPitch {get; private set;} = 1;
    [field:SerializeField] public AudioSource voiceAudioSource {get; private set;}
    [field:SerializeField] public AudioSource dialogueSFXAudioSource {get; private set;}

    [Header("Events")]
    [SerializeField] private UnityEvent OnDialogueStart;
    [SerializeField] private UnityEvent OnDialogueEnd;
    [field:SerializeField] public UnityEvent OnSpeakerSwap {get;private set;}


    #endregion

    #region Private Variables

    private List<AudioClip> currentSpeakerVoice;
    private List<string> currentDialogue;
    private int currentDialogueIndex = 0;
    private bool dialogueActive = false;

    #endregion

    #region Public Variables

    public bool playerSpeaking {get; private set;} = false;
    public DialogueActor currentActor {get;private set;}

    #endregion

    #region Dialogue Brain

    public void StartDialogue(DialogueActor actor, TextAsset dialogeText){
        dialogueActive = true;
        currentActor = actor;
        OnDialogueStart.Invoke();

        dialogueTextContainer.SetActive(true);
        
        ShowButtonPrompt(false);

        currentDialogue = FilterDialogue(dialogeText.text);

        SetDefaultDialogueSettings();

        //DebugDialogue(currentDialogue);

        NextDialogue();
    }

    public void NextDialogue(){
        if(!dialogueActive)return;

        //At the start of each dialogue, turn off the button prompt
        ShowButtonPrompt(false);

        if(!dialogueAnimator.IsDoneAnimating()){
            dialogueAnimator.SkipText();
            return;
        }

        //If the current index is higher than the length of our dialogue list, we are done
        if(currentDialogueIndex >= currentDialogue.Count){
            EndDialogue();
            return;
        }

        
        //Get the next line in the text asset
        string nextDialogueString = currentDialogue[currentDialogueIndex];
        currentDialogueIndex++;


        //Check for main line commands with the parser
        if(parser.PreParseDialogueString(nextDialogueString, out List<DialogueCommand> resultCommands)){
            //Execute the main line commands and go to next line
            foreach(DialogueCommand command in resultCommands){
                ExecuteMainCommand(command);
            }
            NextDialogue();
            return;
        }



        //If there was no main line commands, then continue with normal parsing

        DialogueComponent nextDialogueComponent = parser.ParseDialogueString(nextDialogueString);
        //Set the speaker voice and pass into the animator
        nextDialogueComponent.SetSpeakerVoice(currentSpeakerVoice);


        //If the new processed dialogue string has no visible characters (ie - is empty)
        //Then we dont want to display them, instead we process all/any commands on that line
        //And then go to next dialogue
        if(CheckDialogueStringIsEmpty(nextDialogueComponent)){
            ProcessAllInLineCommands(nextDialogueComponent);
            NextDialogue();
            return;
        }
        dialogueAnimator.ShowText(nextDialogueComponent);
    }

    public void ShowButtonPrompt(bool active){
        if(buttonPromptContainer == null) return;

        buttonPromptContainer.SetActive(active);
    }

    private void EndDialogue(){
        dialogueActive = false;

        dialogueTextContainer.SetActive(false);

        OnDialogueEnd.Invoke();
        currentActor.EndDialogue();

        ResetDialogueVariables();
    }

    #endregion

    #region Command Processing

    //Only called when the dialogue string is empty
    //Processes all in-line commands that dont rely on text (since there is no text in the string) if any are present
    private void ProcessAllInLineCommands(DialogueComponent component){
        foreach(DialogueCommand command in component.dialogueCommands){
            switch(command.commandType){
                case DialogueCommandType.sound :
                    PlayDialogueSoundEffect(command.intValue);
                    break;
                case DialogueCommandType.dialogueEvent :
                    InvokeDialogueEvent(command.intValue);
                    break;
            }
        }
    }

    private void ExecuteMainCommand(DialogueCommand command){
        switch(command.commandType){
            case DialogueCommandType.setPlayerSpeaker :
                SwapSpeaker(command.boolValue);
                break;
            case DialogueCommandType.swapSpeaker :
                SwapSpeaker();
                break;
        }
    }

    private void SetDefaultDialogueSettings(){
        SwapSpeaker(false);
    }

    private void SwapSpeaker(){
        SwapSpeaker(!playerSpeaking);
    }

    private void SwapSpeaker(bool shouldPlayerSpeak){
        playerSpeaking = shouldPlayerSpeak;

        if(playerSpeaking){
            SetSpeakerVariables(
                playerSprite,
                playerColor,
                playerName,
                playerVoice,
                playerFont);
        } else {
            SetSpeakerVariables(
                currentActor.actorSprite,
                currentActor.actorColor,
                currentActor.actorName,
                currentActor.actorVoice,
                currentActor.actorFont);
        }
        OnSpeakerSwap.Invoke();
    }

    private void SetSpeakerVariables(
        Sprite speakerSprite,
        Color textColor,
        string speakerName,
        List<AudioClip> speakerVoice,
        TMP_FontAsset speakerFont)
    {
        
        try{speakerImage.sprite = speakerSprite;}catch{}
        try{
            speakerNameTextBox.text = speakerName;
            speakerNameTextBox.color = textColor;
        }catch{}
        
        dialogueTextBox.color = textColor;

        //Set the font
        if(speakerFont == null){
            if(defaultFont == null){
                Debug.LogError("No default font assigned in brain, unable to change font");
            } else {
                dialogueTextBox.font = defaultFont;
            }
        } else {
            dialogueTextBox.font = speakerFont;
        }

        currentSpeakerVoice = speakerVoice;
    }

    public void PlayDialogueSoundEffect(int soundID){
        AudioClip sound;
        try{
            sound = dialogueSoundEffects[soundID];
        } catch {
            Debug.Log("soundID (index) not in list");
            return;
        }

        dialogueSFXAudioSource.PlayOneShot(sound);
    }

    public void InvokeDialogueEvent(int eventID){
        foreach(DialogueEvent eventObject in currentActor.dialogueEvents.dialogueEvents){
            if(eventObject.eventID != eventID) continue;

            //This event has the right index, Invoke it!
            eventObject._event.Invoke();
            return;
        }
    }

    #endregion

    #region Helpers

    private void ResetDialogueVariables(){
        currentActor = null;
        currentDialogueIndex = 0;
        currentDialogue = null;
    }

    private void DebugDialogue(List<string> dialogeStrings){
        foreach(string dialogeString in dialogeStrings){
            Debug.Log(dialogeString);
        }
    }

    private List<string> FilterDialogue(string inputDialogue){
        string[] splitDialogue = inputDialogue.Split("\n");
        List<string> newSplitDialogue = new List<string>();

        foreach(string line in splitDialogue){
            if(string.IsNullOrWhiteSpace(line))continue;

            newSplitDialogue.Add(line);
        }

        return newSplitDialogue;
    }

    private bool CheckDialogueStringIsEmpty(DialogueComponent component){
        return string.IsNullOrWhiteSpace(component.dialogueString);
    }

    #endregion

}

#region Enums

public enum TextEntryAnimationType{
    scaleUp,
    fadeIn,
    appear
}

public enum DialogueCommandType{
    anim_start,
    anim_end,
    setPlayerSpeaker,
    swapSpeaker,
    pause,
    speed,
    sound,
    dialogueEvent
}

public enum TextAnimationType{
    none,
    wave,
    shake,
}

#endregion