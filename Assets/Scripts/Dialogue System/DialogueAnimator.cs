using System;
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
    private TMP_TextInfo textInfo = null;
    private TextEntryAnimation defaultTextEntryAnimation;
    private float characterAnimationTime;
    private float dialogueAnimationTimeBetweenCharacters;


    private List<Coroutine> allActiveCoroutines = new List<Coroutine>();



    private static readonly Color32 CLEAR = new Color32(0, 0, 0, 0);

    private TMP_MeshInfo[] cachedMeshInfo;
    Color32[][] originalColors;

    #endregion

    private void Start(){
        GetVariablesFromBrain();
    }

    private void GetVariablesFromBrain(){
        dialogueBrain = GetComponent<DialogueBrain>();

        dialogueTextBox = dialogueBrain.dialogueTextBox;

        defaultTextEntryAnimation = dialogueBrain.defaultTextEntryAnimation;

        characterAnimationTime = dialogueBrain.characterAnimationTime;
        dialogueAnimationTimeBetweenCharacters = dialogueBrain.dialogueAnimationTimeBetweenCharacters;
    }

    private void GetTMPObjects(){
        //Set them to null, since the contents are remaining through iterations somehow
        textInfo = null;

        textObject = dialogueTextBox.GetComponent<TMP_Text>();
        textInfo = textObject.textInfo;
    }

    public bool animatingText {get; private set;} = false;

    public void ShowText(string dialogueText){
        animatingText = true;
        allActiveCoroutines.Clear();

        ClearTextBoxMesh();


        dialogueTextBox.text = dialogueText;
        dialogueTextBox.ForceMeshUpdate();

        CacheMeshInfo();

        
        //GetTMPObjects();

        //Prepare the text based on which animation is happening
        PrepareTextMesh();

        //Animate in the text
        allActiveCoroutines.Add(StartCoroutine(AnimateInDialogue()));
        //animatingText = false;//TEMP
    }

    public void SkipText(){
        Debug.Log("SKipping text");
        animatingText = false;

        StopAllCoroutines();

        ShowAllCharacters();

    }

    private void PrepareTextMesh(){
        //Set all the alphas to 0
        //Change this depending on which animation is default
        /*foreach(TMP_WordInfo wordInfo in wordInfos){
            try{
                Debug.Log("new word:" + wordInfo.GetWord());
            }catch{Debug.Log("ERROR getting word");}
            //Loop through each character in the word
            Debug.Log(wordInfo.characterCount);
            //Debug.Log(wordInfo.GetWord());
            for(int i = 0; i < wordInfo.characterCount; i++){
                
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
        }*/
        for(int i = 0; i < textInfo.characterCount; i ++){
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            
            int meshIndex = characterInfo.materialReferenceIndex;
            int vertexIndex = characterInfo.vertexIndex;

            //Set all the vertex alphas to 0
            Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;
            Color32 startColor = vertexColors[vertexIndex];
            startColor.a = 0;
            SetVertexColours(vertexColors, vertexIndex, CLEAR);
        }
        dialogueTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    private void CacheMeshInfo(){
        cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
        originalColors = new Color32[textInfo.meshInfo.Length][];

        for (int i = 0; i < originalColors.Length; i++) {
            Color32[] singleVertexColors = textInfo.meshInfo[i].colors32;
            originalColors[i] = new Color32[singleVertexColors.Length];
            Array.Copy(singleVertexColors, originalColors[i], singleVertexColors.Length);
        }
    }

    private void ClearTextBoxMesh(){
        textInfo = dialogueTextBox.textInfo;
        for (int i = 0; i < textInfo.meshInfo.Length; i++) //Clear the mesh 
        {
            TMP_MeshInfo meshInfer = textInfo.meshInfo[i];
            if (meshInfer.vertices != null) {
                for (int j = 0; j < meshInfer.vertices.Length; j++) {
                    meshInfer.vertices[j] = Vector3.zero;
                }
            }
        }
    }

    private IEnumerator AnimateInDialogue(){
        float timeOfCharacterAnim = Time.time - dialogueAnimationTimeBetweenCharacters;

        //Loop through each character in the dialogue
        for(int i = 0; i < textInfo.characterCount; i++){
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            if(!characterInfo.isVisible)continue;
            
            //Wait untill we can start the animation
            yield return new WaitUntil(() => Time.time >= timeOfCharacterAnim + dialogueAnimationTimeBetweenCharacters);
            Debug.Log("Next Character");
            //Perform the animation
            allActiveCoroutines.Add(StartCoroutine(AnimateInCharacter(characterInfo)));
            timeOfCharacterAnim = Time.time;
        }
        
        
        animatingText = false;
        yield return null;
    }

    private IEnumerator AnimateInCharacter(TMP_CharacterInfo characterInfo){
        float currentTime = 0;
        //float waitPeriod = characterAnimationTime / 100;
        while(currentTime < characterAnimationTime){
            Debug.Log("Step");
            PerformCharacterAnimationStep(characterInfo, currentTime / characterAnimationTime);
            currentTime += Time.deltaTime;

            dialogueTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
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
        Color32 endColor = originalColors[meshIndex][vertexIndex];
        Color32 lerpColor = Color32.Lerp(currentColor, endColor, currentTime);
        SetVertexColours(vertexColors, vertexIndex, lerpColor);
    }

    private void ShowAllCharacters(){
        for(int i = 0; i < textInfo.characterCount; i++){
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            
            int meshIndex = characterInfo.materialReferenceIndex;
            int vertexIndex = characterInfo.vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;
            Color32 originalColor = originalColors[meshIndex][vertexIndex];
            SetVertexColours(vertexColors, vertexIndex, originalColor);
        }
        dialogueTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
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
