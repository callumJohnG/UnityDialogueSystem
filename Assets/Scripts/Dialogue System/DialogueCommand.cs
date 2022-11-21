using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DialogueCommand
{
    public DialogueCommandType commandType {get; private set;}
    public int charIndex {get; private set;}
    public TextAnimationType animationType {get; private set;}

    public DialogueCommand(DialogueCommandType commandType, int charIndex, TextAnimationType animationType = TextAnimationType.none){
        this.commandType = commandType;
        this.charIndex = charIndex;
        this.animationType = animationType;
    }
}
