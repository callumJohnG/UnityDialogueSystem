using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public struct AnimationRange{
    public int startIndex {get; private set;}
    public int endIndex {get; private set;}
    public TextAnimationType animationType {get; private set;}

    public AnimationRange(int startIndex, int endIndex, TextAnimationType animationType){
        this.startIndex = startIndex;
        this.endIndex = endIndex;
        this.animationType = animationType;
    }
}

public class DialogueAnimator : MonoBehaviour
{
    private DialogueBrain dialogueBrain;

    #region Variables

    #region From Brain
    private TextMeshProUGUI dialogueTextBox;
    private TextEntryAnimationType defaultTextEntryAnimation;
    private float characterAnimationTime;
    private float characterWaitTime;

    #endregion

    #region Private

    private TMP_Text textObject;
    private TMP_TextInfo textInfo = null;
    private float totalCharacters;
    private float visibleCharacters = 0;
    private List<Coroutine> allActiveCoroutines = new List<Coroutine>();
    private static readonly Color32 CLEAR = new Color32(0, 0, 0, 0);
    private TMP_MeshInfo[] cachedMeshInfo;
    private Color32[][] originalColors;
    private DialogueComponent currentDialogueComponent;
    private AudioSource voiceAudioSource;
    private float voiceMaxPitch;
    private float voiceMinPitch;

    #endregion

    #region Public To Brain
    
    public bool animatingText {get; private set;} = false;

    #endregion

    #endregion

    private void Start(){
        GetVariablesFromBrain();
    }

    private void Update(){
        UpdateTextAnimation();
    }

    #region Set-Up

    private void GetVariablesFromBrain(){
        dialogueBrain = GetComponent<DialogueBrain>();
        voiceAudioSource = GetComponent<AudioSource>();

        dialogueTextBox = dialogueBrain.dialogueTextBox;

        defaultTextEntryAnimation = dialogueBrain.defaultTextEntryAnimation;

        this.characterAnimationTime = dialogueBrain.charAnimationTime;
        this.characterWaitTime = dialogueBrain.charWaitTime;

        this.voiceMinPitch = dialogueBrain.voiceMinPitch;
        this.voiceMaxPitch = dialogueBrain.voiceMaxPitch;

    }

    private void GetTMPObjects(){
        //Set them to null, since the contents are remaining through iterations somehow
        textInfo = null;

        textObject = dialogueTextBox.GetComponent<TMP_Text>();
        textInfo = textObject.textInfo;
    }

    #endregion

    #region Main Animation Functions (Show Text / Skip Text)


    public void ShowText(DialogueComponent nextDialogueComponent){
        currentDialogueComponent = nextDialogueComponent;

        animatingText = true;
        allActiveCoroutines.Clear();

        ClearTextBoxMesh();

        dialogueTextBox.text = currentDialogueComponent.dialogueString;
        dialogueTextBox.ForceMeshUpdate();

        CacheMeshInfo();

        //Prepare the text based on which animation is happening
        PrepareTextMesh();

        //Set up the text update animation with the pointers
        InitialiseTextAnimation(currentDialogueComponent.dialogueString.Length, currentDialogueComponent.dialogueCommands);

        //Animate in the text
        allActiveCoroutines.Add(StartCoroutine(AnimateInDialogue()));
        //animatingText = false;//TEMP
    }

    public void SkipText(){
        Debug.Log("SKipping text");
        animatingText = false;
        visibleCharacters = 0;
        totalCharacters = 0;

        StopAllCoroutines();

        ShowAllCharacters();

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
        totalCharacters = textInfo.characterCount;
        visibleCharacters = 0;

        float timeOfCharacterAnim = Time.time - characterWaitTime;

        //Loop through each character in the dialogue
        for(int i = 0; i < textInfo.characterCount; i++){
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            if(!characterInfo.isVisible){
                totalCharacters--; //We dont count invisible characters in our total
                continue;
            }
            
            //Wait untill we can start the animation
            yield return new WaitUntil(() => Time.time >= timeOfCharacterAnim + characterWaitTime);
            
            
            //Perform the animation
            AnimateInCharacter(characterInfo);
            timeOfCharacterAnim = Time.time;
            PlayVoiceAudio();
        }
        
        
        animatingText = false;
        yield return null;
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

    #endregion

    #region Animation Type Dependant Functions

    #region Set-Up

    private void PrepareTextMesh(){
        switch(defaultTextEntryAnimation){
            case TextEntryAnimationType.fadeIn :
            case TextEntryAnimationType.appear :
                PrepareTextMeshAlpha0();
                break;
            case TextEntryAnimationType.scaleUp :
                PrepareTextMeshScale0();
                break;
        }
        dialogueTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    private void PrepareTextMeshAlpha0(){
        Debug.Log("Setting alphas to 0");
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
    }

    private void PrepareTextMeshScale0(){

    }

    #endregion

    #region Animation

    private void AnimateInCharacter(TMP_CharacterInfo characterInfo){
        switch(defaultTextEntryAnimation){
            case TextEntryAnimationType.appear :
                CharacterAnimationAppear(characterInfo);
                break;
            case TextEntryAnimationType.scaleUp :
                allActiveCoroutines.Add(StartCoroutine(CharacterAnimationScaleUp(characterInfo)));
                break;
            case TextEntryAnimationType.fadeIn :
                allActiveCoroutines.Add(StartCoroutine(CharacterAnimationFadeIn(characterInfo)));
                break;
        }
    }

    private void CharacterAnimationAppear(TMP_CharacterInfo characterInfo){
        //Show the character
        int meshIndex = characterInfo.materialReferenceIndex;
        int vertexIndex = characterInfo.vertexIndex;

        Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;
        Color32 newColor = originalColors[meshIndex][vertexIndex];
        SetVertexColours(vertexColors, vertexIndex, newColor);

        dialogueTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

        visibleCharacters ++; //Indicates that we have finished animating;
    }

    private IEnumerator CharacterAnimationFadeIn(TMP_CharacterInfo characterInfo){
        float currentTime = 0;

        while(currentTime < characterAnimationTime){

            //Perform an animation step
            //Lerp the colour of the vertex
            int meshIndex = characterInfo.materialReferenceIndex;
            int vertexIndex = characterInfo.vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;
            Color32 currentColor = vertexColors[vertexIndex];
            Color32 endColor = originalColors[meshIndex][vertexIndex];
            Color32 lerpColor = Color32.Lerp(currentColor, endColor, currentTime / characterAnimationTime);
            SetVertexColours(vertexColors, vertexIndex, lerpColor);



            currentTime += Time.deltaTime;
            dialogueTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            yield return null;
        }

        visibleCharacters ++; //Indicates that we have finished animating;
    }

    private IEnumerator CharacterAnimationScaleUp(TMP_CharacterInfo characterInfo){
        yield return null;
    }

    private void PerformCharacterAnimationStep(TMP_CharacterInfo characterInfo, float currentTime){
        //IMPROVE THIS TO TAKE CARE OF ALL ANIMATION TYPES
        //CURRENTLY JUST FADES IN

        
    }

    #endregion

    #endregion

    #region Update Text Animation

    private List<AnimationRange> animationRanges = new List<AnimationRange>();

    //Make a list of dialogueCommands that indicate what animation should be playing on each character
    private void InitialiseTextAnimation(int length, List<DialogueCommand> allCommands){
        TextAnimationType currentAnimType = TextAnimationType.none;
        animationRanges.Clear();
        int startIndex = 0;

        for(int i = 0; i < length; i++){
            foreach(DialogueCommand command in allCommands){
                if(command.charIndex != i)continue;
                //If its not an animation command, then continue
                if(command.commandType != DialogueCommandType.anim_start && command.commandType != DialogueCommandType.anim_end)continue;
                
                //There is a command on this spot
                if(command.commandType == DialogueCommandType.anim_start){
                    startIndex = i;
                    currentAnimType = command.animationType;
                }

                if(command.commandType == DialogueCommandType.anim_end){
                    animationRanges.Add(new AnimationRange(
                        startIndex,
                        i,
                        currentAnimType
                    ));
                    currentAnimType = TextAnimationType.none;
                    startIndex = 0;
                }
            }
        }
    }

    private void UpdateTextAnimation(){
        //We want to step through each index of the characters in the dialouge
        //at each 
        if(textInfo == null)return;

        //Loop through each character in the dialogue text box
        for(int i = 0; i < textInfo.characterCount; i++){
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            if(!characterInfo.isVisible)continue;

            int vertexIndex = characterInfo.vertexIndex;
            int materialReferenceIndex = characterInfo.materialReferenceIndex;

            Vector3[] sourceVertices = cachedMeshInfo[materialReferenceIndex].vertices;
            Vector3[] destinationVertices = textInfo.meshInfo[materialReferenceIndex].vertices;

            Vector3 animationPosition = GetAnimationPosition(animationRanges, i, dialogueTextBox.fontSize, Time.unscaledTime);
            Vector3 offset = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;
            SetCharacterVerticesPosition(destinationVertices, sourceVertices, animationPosition, offset, vertexIndex);
        }
        dialogueTextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
    private void SetCharacterVerticesPosition(Vector3[] destinationVertices, Vector3[] sourceVertices, Vector3 animationPos, Vector3 offset, int vertexIndex){
        destinationVertices[vertexIndex + 0] = ((sourceVertices[vertexIndex + 0] - offset)) + offset + animationPos;
        destinationVertices[vertexIndex + 1] = ((sourceVertices[vertexIndex + 1] - offset)) + offset + animationPos;
        destinationVertices[vertexIndex + 2] = ((sourceVertices[vertexIndex + 2] - offset)) + offset + animationPos;
        destinationVertices[vertexIndex + 3] = ((sourceVertices[vertexIndex + 3] - offset)) + offset + animationPos;
    }

    #region Text animation position calculations

    private Vector3 GetAnimationPosition(List<AnimationRange> animationRanges, int charIndex, float fontSize, float time){
        float x = 0;
        float y = 0;

        foreach(AnimationRange animationRange in animationRanges){
            if (charIndex >= animationRange.startIndex && charIndex < animationRange.endIndex) {
                switch(animationRange.animationType){
                    case TextAnimationType.wave : GetWavePosition(out y, charIndex, fontSize, time);break;
                    case TextAnimationType.shake: GetShakePosition(out x, out y, charIndex, fontSize, time);break;
                }
            }
        }
        return new Vector3(x, y, 0);
    }

    private const float WAVE_MAGNITUDE_ADJUSTMENT = 0.06f;
    private void GetWavePosition(out float y, int charIndex, float fontSize, float time){
        y = Mathf.Sin((charIndex * 1.5f) + (time * 6)) * fontSize * WAVE_MAGNITUDE_ADJUSTMENT;
    }

    private const float NOISE_MAGNITUDE_ADJUSTMENT = 0.06f;
    private const float NOISE_FREQUENCY_ADJUSTMENT = 15f;
    private void GetShakePosition(out float x, out float y, int charIndex, float fontSize, float time){
        float scaleAdjust = fontSize * NOISE_MAGNITUDE_ADJUSTMENT;
        x = (Mathf.PerlinNoise((charIndex + time) * NOISE_FREQUENCY_ADJUSTMENT, 0) - 0.5f) * scaleAdjust;
        y = (Mathf.PerlinNoise((charIndex + time) * NOISE_FREQUENCY_ADJUSTMENT, 1000) - 0.5f) * scaleAdjust;
    }

    #endregion

    #endregion

    #region Audio

    private void PlayVoiceAudio(){
        //If there's no voice, then do nothing
        if(currentDialogueComponent.speakerVoice.Count == 0)return;

        //Pick a random audio clip to use
        AudioClip randomClip = currentDialogueComponent.speakerVoice[UnityEngine.Random.Range(0, currentDialogueComponent.speakerVoice.Count)];

        //Pick a random pitch for our audio source
        float randomPitch = UnityEngine.Random.Range(voiceMinPitch, voiceMaxPitch);
        voiceAudioSource.pitch = randomPitch;

        voiceAudioSource.PlayOneShot(randomClip);
    }

    #endregion

    #region Helpers

    private void SetVertexColours(Color32[] vertexColors, int vertexIndex, Color32 newColor){
        vertexColors[vertexIndex + 0] = newColor;
        vertexColors[vertexIndex + 1] = newColor;
        vertexColors[vertexIndex + 2] = newColor;
        vertexColors[vertexIndex + 3] = newColor;
    }

    public bool IsDoneAnimating(){
        if(animatingText) return false;

        if(visibleCharacters < totalCharacters) return false;

        return true;
    }

    #endregion

}
