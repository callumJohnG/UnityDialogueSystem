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
    private PlayerControls controls;

    private void ConfigureControlListeners(){
        controls = new PlayerControls();

        controls.Dialogue.ProgressDialogue.performed += _ => NextDialogue();
    }

    private void NextDialogue(){
        dialogueBrain.NextDialogue();
    }

}
