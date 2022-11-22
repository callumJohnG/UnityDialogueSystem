using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DialogueCommand
{
    public DialogueCommandType commandType {get; private set;}
    public int charIndex {get; private set;}
    public TextAnimationType animationType {get; private set;}
    public bool boolValue {get; private set;}

    public DialogueCommand(
        DialogueCommandType commandType,
        int charIndex = 0,
        TextAnimationType animationType = TextAnimationType.none,
        bool boolValue = false){

            this.commandType = commandType;
            this.charIndex = charIndex;
            this.animationType = animationType;
            this.boolValue = boolValue;
    }
}
