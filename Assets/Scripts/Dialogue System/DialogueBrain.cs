using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

#region Enums

public enum TextEntryAnimation{
    scaleUp,
    fadeIn,
    appear
}

public enum TextMidAnimation{
    wave,
    shake
}

#endregion

[RequireComponent(typeof(DialogueParser), typeof(DialogueAnimator), typeof(AudioSource))]
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
    private DialogueParser dialogueParser;

    private void GetDialogueComponents(){
        dialogueAnimator = GetComponent<DialogueAnimator>();
        dialogueParser = GetComponent<DialogueParser>();
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
    [field:SerializeField] public TextEntryAnimation defaultTextEntryAnimation {get; private set;}
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

    private List<AudioClip> currentSpeakerVoice;

    #endregion

    #region Dialogue Brain

    private DialogueActor currentActor;
    private List<string> currentDialogue;
    private int currentDialogueIndex = 0;
    private bool dialogueActive = false;

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
        #region TO GO IN PARSER
        
        //Get the next line in the text asset
        string nextDialogueString = currentDialogue[currentDialogueIndex];
        currentDialogueIndex++;

        //Process the next line of dialogue
        if(ProcessDialogue(nextDialogueString)){            
            //Set the dialogue text UI to have the next string
            //dialogueTextObject.text = nextDialogueString;
            DialogueComponent nextDialogueComponent = new DialogueComponent(currentSpeakerVoice, nextDialogueString);
            dialogueAnimator.ShowText(nextDialogueComponent);
        } else {
            NextDialogue();
        }
        
        #endregion

        //PLAN
        //Parse the dialogue (dialogue parser)
        //Show the dialogue (dialogue Animator)
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

    private bool ProcessDialogue(string dialogueString){
        
        //Pattern matching to get the correct function to run
        if(dialogueString.Contains("[SetSpeakerPlayer]")){
            bool value = dialogueString.Contains("true");
            SwapSpeaker(value);
            return false;
        }

        else if(dialogueString.Contains("[SwapSpeaker]")){
            SwapSpeaker();
            return false;
        }


        //it is just normal text
        return true;
        
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