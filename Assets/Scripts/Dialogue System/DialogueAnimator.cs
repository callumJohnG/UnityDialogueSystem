using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueAnimator : MonoBehaviour
{
    private DialogueBrain dialogueBrain;

    #region Variables

    private TextMeshProUGUI dialogueTextBox;
    private TMP_Text textObject;
    private TMP_TextInfo textInfo;
    private TMP_WordInfo[] wordInfos;
    private TextEntryAnimation defaultTextEntryAnimation;
    private float characterAnimationTime;
    private float dialogueAnimationTimeBetweenCharacters;

    #endregion

    private void Start(){
        GetVariablesFromBrain();
    }

    private void GetVariablesFromBrain(){
        dialogueBrain = GetComponent<DialogueBrain>();

        dialogueTextBox = dialogueBrain.dialogueTextBox;

        defaultTextEntryAnimation = dialogueBrain.defaultTextEntryAnimation;
    }

    private void GetTMPObjects(){
        textObject = dialogueTextBox.GetComponent<TMP_Text>();
        textInfo = textObject.textInfo;
        wordInfos = textInfo.wordInfo;
    }

    public bool animatingText {get; private set;} = false;

    public void ShowText(string dialogueText){
        animatingText = true;

        dialogueTextBox.text = dialogueText;
        GetTMPObjects();

        //Prepare the text based on which animation is happening
        PrepareTextMesh();

        //Animate in the text
        StartCoroutine(AnimateInDialogue());
    }

    public void SkipText(){
        Debug.Log("SKipping text");
        animatingText = false;
    }

    private void PrepareTextMesh(){
        //Set all the alphas to 0
        //Change this depending on which animation is default
        foreach(TMP_WordInfo wordInfo in wordInfos){
            Debug.Log("Word");
            //Loop through each character in the word
            Debug.Log(wordInfo.characterCount);
            Debug.Log(wordInfo.GetWord());
            for(int i = 0; i < wordInfo.characterCount; i++){
                Debug.Log("Char");
                
                //Get the relavent indexes
                int charIndex = wordInfo.firstCharacterIndex + i;
                TMP_CharacterInfo characterInfo = textInfo.characterInfo[charIndex];
                int meshIndex = characterInfo.materialReferenceIndex;
                int vertexIndex = characterInfo.vertexIndex;

                //Set all the vertex alphas to 0
                Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;
                Color32 startColor = vertexColors[vertexIndex];
                startColor.a = 0;
                SetVertexColours(vertexColors, vertexIndex, startColor);
            }
        }
    }

    private IEnumerator AnimateInDialogue(){
        //Loop through each word in the text
        foreach(TMP_WordInfo wordInfo in wordInfos){
            //Loop through each character in the word
            for(int i = 0; i < wordInfo.characterCount; i++){
                
                //Get the relavent indexes
                int charIndex = wordInfo.firstCharacterIndex + i;
                TMP_CharacterInfo characterInfo = textInfo.characterInfo[charIndex];
                StartCoroutine(AnimateInCharacter(characterInfo));

                //Perform the animation

            }
        }
        
        
        animatingText = false;
        yield return null;
    }

    private IEnumerator AnimateInCharacter(TMP_CharacterInfo characterInfo){
        float currentTime = 0;
        //float waitPeriod = characterAnimationTime / 100;
        while(currentTime < characterAnimationTime){
            PerformCharacterAnimationStep(characterInfo, currentTime / characterAnimationTime);
            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        //yield return null;
    }

    private void PerformCharacterAnimationStep(TMP_CharacterInfo characterInfo, float currentTime){
        //IMPROVE THIS TO TAKE CARE OF ALL ANIMATION TYPES
        //CURRENTLY JUST FADES IN

        //Lerp the colour of the vertex
        int meshIndex = characterInfo.materialReferenceIndex;
        int vertexIndex = characterInfo.vertexIndex;

        Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;
        Color32 currentColor = vertexColors[vertexIndex];
        Color32 endColor = vertexColors[vertexIndex];
        endColor.a = 1;
        Color32 lerpColor = Color32.Lerp(currentColor, endColor, currentTime);
        SetVertexColours(vertexColors, vertexIndex, lerpColor);
    }

    #region Helpers

    private void SetVertexColours(Color32[] vertexColors, int vertexIndex, Color32 newColor){
        vertexColors[vertexIndex + 0] = newColor;
        vertexColors[vertexIndex + 1] = newColor;
        vertexColors[vertexIndex + 2] = newColor;
        vertexColors[vertexIndex + 3] = newColor;
    }

    #endregion

}
