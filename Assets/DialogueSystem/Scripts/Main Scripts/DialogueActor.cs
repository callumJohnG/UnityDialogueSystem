using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(DialogueEvents))]
public class DialogueActor : MonoBehaviour
{

    [SerializeField] private List<TextAsset> dialogueList;
    [SerializeField] private bool repeatLastDialouge = true;

    [field:SerializeField] public Sprite actorSprite {get; private set;}
    [field:SerializeField] public Color actorColor {get; private set;}
    [field:SerializeField] public string actorName {get; private set;}
    [field:SerializeField] public List<AudioClip> actorVoice {get; private set;}
    [field:SerializeField] public TMP_FontAsset actorFont {get; private set;}
    public DialogueEvents dialogueEvents {get; private set;}

    private int dialogueIndex = 0;

    [SerializeField] private UnityEvent OnDialogueStart;
    [SerializeField] private UnityEvent OnDialogueEnd;

    private void Awake(){
        dialogueEvents = GetComponent<DialogueEvents>();
    }

    public void StartDialogue(){
        //If we have run out of dialouge, then keep repeating the last dialogue if repeatLastDialogue is true
        if(!CheckHasDialogueRemaining())return;
        if(dialogueIndex >= dialogueList.Count){
            dialogueIndex = dialogueList.Count -1;
        }

        
        OnDialogueStart.Invoke();

        DialogueBrain.Instance.StartDialogue(this, dialogueList[dialogueIndex]);

        dialogueIndex++;
    }

    public void EndDialogue(){
        OnDialogueEnd.Invoke();
    }

    //Returns whether or not this actor has dialogue remaining in their list
    //Accounts for repeatLastDialogue
    public bool CheckHasDialogueRemaining(){
        if(dialogueIndex >= dialogueList.Count){
            if(repeatLastDialouge){
                return true;
            } else {
                return false;
            }
        }
        return true;
    }
}
