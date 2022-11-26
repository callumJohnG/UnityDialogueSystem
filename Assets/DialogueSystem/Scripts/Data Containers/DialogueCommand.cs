using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DialogueCommand
{
    public DialogueCommandType commandType  {get; private set;}
    public int charIndex                    {get; private set;}
    public TextAnimationType animationType  {get; private set;}
    public bool boolValue                   {get; private set;}
    public float floatValue                 {get; private set;}
    public int intValue                     {get; private set;}
    public string stringValue               {get; private set;}
    public bool processed                   {get; private set;}



    public DialogueCommand(
                            DialogueCommandType commandType,
                            int                 charIndex = 0,
                            TextAnimationType   animationType = TextAnimationType.none,
                            bool                boolValue = false,
                            float               floatValue = 0,
                            int                 intValue = 0,
                            string              stringValue = "")
    {
        this.commandType = commandType;
        this.charIndex = charIndex;
        this.animationType = animationType;
        this.boolValue = boolValue;
        this.floatValue = floatValue;
        this.intValue = intValue;
        this.stringValue = stringValue;

        processed = false;
    }

    public void SetProcessed(){
        processed = true;
    }
}
