using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DialogueComponent
{

    public List<AudioClip> speakerVoice {get; private set;}
    public string dialogueString;

    public DialogueComponent(List<AudioClip> speakerVoice, string dialogeString){
        this.speakerVoice = speakerVoice;
        this.dialogueString = dialogeString;
    }

}
