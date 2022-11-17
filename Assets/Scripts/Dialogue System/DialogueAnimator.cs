using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueAnimator : MonoBehaviour
{
    private DialogueBrain dialogueBrain;

    #region Variables

    private TextMeshProUGUI dialogueTextBox;
    private TMP_Text textObject;
    private TMP_TextInfo textInfo;
    private TextEntryAnimation defaultTextEntryAnimation;

    #endregion

    private void Start(){
        GetVariablesFromBrain();
    }

    private void GetVariablesFromBrain(){
        dialogueBrain = GetComponent<DialogueBrain>();

        dialogueTextBox = dialogueBrain.dialogueTextBox;
        textObject = dialogueTextBox.GetComponent<TMP_Text>();
        textInfo = textObject.textInfo;

        defaultTextEntryAnimation = dialogueBrain.defaultTextEntryAnimation;
    }

    public void ShowText(string dialogueText){
        dialogueTextBox.text = dialogueText;

        //Put text into the text box
        //Set all the rects to be invisible
        //Depending on the animation mode (default anim mode) scale them in
        //Have some variables like fade speed, text speed (taken from the brain)
    }

}
