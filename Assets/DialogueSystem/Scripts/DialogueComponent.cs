using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DialogueComponent
{

    public List<AudioClip> speakerVoice {get; private set;}
    public string dialogueString {get; private set;}
    public List<DialogueCommand> dialogueCommands {get; private set;}

    public DialogueComponent(string dialogeString, List<DialogueCommand> dialogueCommands, List<AudioClip> speakerVoice = null){
        this.dialogueString = dialogeString;
        this.dialogueCommands = dialogueCommands;
        this.speakerVoice = speakerVoice;
    }

    public void SetSpeakerVoice(List<AudioClip> speakerVoice){
        this.speakerVoice = speakerVoice;
    }

}
