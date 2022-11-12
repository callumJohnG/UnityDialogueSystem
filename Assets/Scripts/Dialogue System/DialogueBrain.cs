using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueBrain : MonoBehaviour
{

    public static DialogueBrain Instance {get;private set;}

    private void Awake(){
        Instance = this;

        ConfigureControlListeners();
    }

    private void OnEnable(){
        controls.Enable();
    }

    private void OnDisable(){
        controls.Disable();
    }

    #region Serialized Varibles

    [Header("Player")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Color playerColor;
    [SerializeField] private string playerName;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI dialogueTextObject;
    [SerializeField] private TextMeshProUGUI actorNameTextObject;
    [SerializeField] private GameObject dialogueTextContainer;
    [SerializeField] private Image actorImage;

    [Header("Global Events (Called on each dialogue")]
    [SerializeField] private UnityEvent OnDialogueStart;
    [SerializeField] private UnityEvent OnDialogueEnd;

    #endregion

    #region Player Input

    private PlayerControls controls;

    private void ConfigureControlListeners(){
        controls = new PlayerControls();

        controls.Dialogue.ProgressDialogue.performed += _ => NextDialogue();
    }

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

    private void NextDialogue(){
        if(!dialogueActive)return;

        Debug.Log("NEXT DIALOGUE");

        //If the current index is higher than the length of our dialogue list, we are done
        if(currentDialogueIndex >= currentDialogue.Count){
            EndDialogue();
            return;
        }

        //Get the next line in the text asset
        string nextDialogueString = currentDialogue[currentDialogueIndex];
        currentDialogueIndex++;

        Debug.Log(nextDialogueString);

        //Process the next line of dialogue
        if(ProcessDialogue(nextDialogueString)){            
            //Set the dialogue text UI to have the next string
            dialogueTextObject.text = nextDialogueString;
        } else {
            NextDialogue();
        }
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
            SetSpeakerVariables(playerSprite, playerColor, playerName);
        } else {
            SetSpeakerVariables(currentActor.actorSprite, currentActor.actorColor, currentActor.actorName);
        }
    }

    private void SetSpeakerVariables(Sprite actorSprite, Color textColor, string actorName){
        actorImage.sprite = actorSprite;
        dialogueTextObject.color = textColor;
        actorNameTextObject.color = textColor;
        actorNameTextObject.text = actorName;
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