using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

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
    swapSpeaker
}

public enum TextAnimationType{
    none,
    wave,
    shake,
}

#endregion

[RequireComponent(typeof(DialogueAnimator), typeof(AudioSource))]
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

    [Header("Dialogue Animation / UI")]
    [SerializeField] private GameObject dialogueTextContainer;
    [field:SerializeField] public TextMeshProUGUI dialogueTextBox {get; private set;}
    [field:SerializeField] public TextMeshProUGUI actorNameTextBox {get; private set;}
    [SerializeField] private Image actorImage;

    [Header("Animation")]
    [SerializeField] private bool titleVar1;
    [field:SerializeField] public TextEntryAnimationType defaultTextEntryAnimation {get; private set;}
    [field:SerializeField] public float charAnimationTime {get; private set;}
    [field:SerializeField] public float charWaitTime {get; private set;}

    [Header("Audio")]
    [SerializeField] private bool titleVar2;
    [field:SerializeField] public float voiceMinPitch {get; private set;} = 1;
    [field:SerializeField] public float voiceMaxPitch {get; private set;} = 1;

    [Header("Global Events (Called on each dialogue")]
    [SerializeField] private UnityEvent OnDialogueStart;
    [SerializeField] private UnityEvent OnDialogueEnd;

    #endregion

    #region Private Variables

    private List<AudioClip> currentSpeakerVoice;private DialogueActor currentActor;
    private List<string> currentDialogue;
    private int currentDialogueIndex = 0;
    private bool dialogueActive = false;

    #endregion

    #region Dialogue Brain

    public void StartDialogue(DialogueActor actor, TextAsset dialogeText){
        Debug.Log("Dialoge Starting");
        dialogueActive = true;
        currentActor = actor;
        OnDialogueStart.Invoke();

        dialogueTextContainer.SetActive(true);

        currentDialogue = FilterDialogue(dialogeText.text);

        SetDefaultDialogueSettings();

        //DebugDialogue(currentDialogue);

        //Dont need to call "NextDialogue()" here because it gets called by the input system (controls) when the player clicks to start the dialogue
    }

    public void NextDialogue(){
        if(!dialogueActive)return;

        if(!dialogueAnimator.IsDoneAnimating()){
            dialogueAnimator.SkipText();
            return;
        }

        Debug.Log("NEXT DIALOGUE");

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
        dialogueAnimator.ShowText(nextDialogueComponent);
        
    }

    private void EndDialogue(){
        Debug.Log("Ending Dialogue");
        dialogueActive = false;

        dialogueTextContainer.SetActive(false);

        OnDialogueEnd.Invoke();
        currentActor.EndDialogue();

        ResetDialogueVariables();
    }

    #endregion

    #region Dialogue Processing

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


    private bool playerSpeaking = false;

    private void SwapSpeaker(){
        Debug.Log("SWAPPING SPEAKER");
        SwapSpeaker(!playerSpeaking);
    }

    private void SwapSpeaker(bool shouldPlayerSpeak){
        playerSpeaking = shouldPlayerSpeak;

        if(playerSpeaking){
            SetSpeakerVariables(
                playerSprite,
                playerColor,
                playerName,
                playerVoice);
        } else {
            SetSpeakerVariables(
                currentActor.actorSprite,
                currentActor.actorColor,
                currentActor.actorName,
                currentActor.actorVoice);
        }
    }

    private void SetSpeakerVariables(Sprite speakerSprite, Color textColor, string speakerName, List<AudioClip> speakerVoice){
        actorImage.sprite = speakerSprite;
        actorNameTextBox.text = speakerName;

        dialogueTextBox.color = textColor;
        actorNameTextBox.color = textColor;

        currentSpeakerVoice = speakerVoice;
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

    #endregion

}