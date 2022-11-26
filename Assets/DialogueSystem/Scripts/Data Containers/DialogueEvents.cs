using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//The list of unity events unique to this character

[System.Serializable]
public class DialogueEvents : MonoBehaviour
{
    public List<DialogueEvent> dialogueEvents;
}

[System.Serializable]
public struct DialogueEvent
{
    public int eventID;
    public UnityEvent _event;
}
