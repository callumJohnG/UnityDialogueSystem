using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DialogueBrain))]
public class DialogueInputController : MonoBehaviour
{


    private void Awake(){
        dialogueBrain = GetComponent<DialogueBrain>();
        ConfigureControlListeners();
    }

    private void OnEnable(){
        controls.Enable();
    }

    private void OnDisable(){
        controls.Disable();
    }

    private DialogueBrain dialogueBrain;
    private DialogueSamplePlayerControls controls;

    private void ConfigureControlListeners(){
        controls = new DialogueSamplePlayerControls();

        controls.Dialogue.ProgressDialogue.performed += _ => NextDialogue();
    }

    private void NextDialogue(){
        dialogueBrain.NextDialogue();
    }

}
