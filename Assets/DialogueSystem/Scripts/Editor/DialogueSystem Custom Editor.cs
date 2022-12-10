using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//Custom editor used to display the list of unity events in the inspector

[CustomEditor(typeof(DialogueEvents))]
public class DialogueSystemCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DialogueEvents _target = (DialogueEvents)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueEvents"), true);
        serializedObject.ApplyModifiedProperties();
    }
}