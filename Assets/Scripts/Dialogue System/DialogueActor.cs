using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueActor : MonoBehaviour
{

    [SerializeField] private List<TextAsset> dialogueList;

    [field:SerializeField] public Sprite actorSprite {get; private set;}
    [field:SerializeField] public Color actorColor {get; private set;}
    [field:SerializeField] public string actorName {get; private set;}
    [field:SerializeField] public List<AudioClip> actorVoice {get; private set;}

    private int dialogueIndex = 0;

    [SerializeField] private UnityEvent OnDialogueStart;
    [SerializeField] private UnityEvent OnDialogueEnd;


    public void StartDialogue(){
        OnDialogueStart.Invoke();

        DialogueBrain.Instance.StartDialogue(this, dialogueList[dialogueIndex]);


        if(dialogueIndex < dialogueList.Count - 1){
            dialogueIndex++;
        }
    }

    public void EndDialogue(){
        OnDialogueEnd.Invoke();
    }

}
