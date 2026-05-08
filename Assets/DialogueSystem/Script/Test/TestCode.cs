using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TestCode : MonoBehaviour
{
    
    public List<CharacterInformation> currentDialogue;

    public void AddDialogueTest()
    {
        DialogueManager.Instance.AddDialogueAndStart(currentDialogue);
        Debug.Log("AddDialogueTest() executed!");
    }
    public void StartDialogueTest()
    {
        DialogueManager.Instance.StartDialogue();
        Debug.Log("StartDialogue() executed!");
    }
}

[CustomEditor(typeof(TestCode))]
public class TestCodeEditor : Editor
{
    public bool startDialogue = false;
    public override void OnInspectorGUI()
    {

        GUILayout.Space(5);

   
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Dialogue Test Info", titleStyle);

        GUILayout.Space(5);

      
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.wordWrap = true; 

        EditorGUILayout.LabelField(
            "This test script starts a dialogue through the DialogueManager using the currentDialogue list provided in the Inspector. " +
            "When the “Dialogue Test” button is pressed, the list is sent to the manager and the dialogue begins. " +
            "During playback, the Space key can be used to speed up the typing or move to the next line. " +
            "This allows quick verification that the dialogue system is working correctly.",
            textStyle
        );

        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


        DrawDefaultInspector();

        TestCode testCode = (TestCode)target;

        if (startDialogue == false)
        {
            if (GUILayout.Button(new GUIContent("Start Dialogue", "Starts dialogue using current data")))
            {
                testCode.StartDialogueTest();
            }
        }

        if (GUILayout.Button(new GUIContent("Add The Dialogue And Start", "Adds dialogue list to the manager and starts it")))
        {
            startDialogue = true;
            testCode.AddDialogueTest();
        }

        GUILayout.Space(10);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);
        GUIStyle titleStyle2 = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 14;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Created by Batu Özçamlık", titleStyle);

        GUILayout.Space(5);

        GUIStyle textStyle2 = new GUIStyle(GUI.skin.label);
        textStyle.wordWrap = true;
        textStyle.richText = true;

        GUIStyle signatureStyle = new GUIStyle(GUI.skin.label);
        signatureStyle.alignment = TextAnchor.MiddleRight;
        signatureStyle.fontStyle = FontStyle.Italic;
        EditorGUILayout.LabelField("www.batuozcamlik.com", signatureStyle);

    }
}
