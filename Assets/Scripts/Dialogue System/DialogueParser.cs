using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DialogueParser
{

    #region REGEX

    private const string REMAINDER_REGEX = "(.*?((?=>)|(/|$)))";
    private const string ANIM_START_REGEX_STRING = "<anim:(?<anim>" + REMAINDER_REGEX + ")>";
    private static readonly Regex animStartRegex = new Regex(ANIM_START_REGEX_STRING);
    private const string ANIM_END_REGEX_STRING = "</anim>";
    private static readonly Regex animEndRegex = new Regex(ANIM_END_REGEX_STRING);
    private const string SET_SPEAKER_REGEX_STRING = "<setPlayerSpeaker:(?<bool>" + REMAINDER_REGEX + ")>";
    private static readonly Regex setSpeakerRegex = new Regex(SET_SPEAKER_REGEX_STRING);
    private const string SWAP_SPEAKER_REGEX_STRING = "<swapSpeaker>";
    private static readonly Regex swapSpeakerRegex = new Regex(SWAP_SPEAKER_REGEX_STRING);
    private const string PAUSE_REGEX_STRING = "<p:(?<pause>" + REMAINDER_REGEX + ")>";
    private static readonly Regex pauseRegex = new Regex(PAUSE_REGEX_STRING);
    private const string SPEED_REGEX_STRING = "<sp:(?<speed>" + REMAINDER_REGEX + ")>";
    private static readonly Regex speedRegex = new Regex(SPEED_REGEX_STRING);

    #endregion

    public DialogueParser(){}

    #region Pre Parsing

    public bool PreParseDialogueString(string dialogeString, out List<DialogueCommand> resultCommands){
        resultCommands = new List<DialogueCommand>();
        bool boolResult = false;
        List<DialogueCommand> tempCommands;

        //Search for setPlayerSpeaker Command
        if(ParseSetSpeakerRegex(dialogeString, out tempCommands)){
            resultCommands.AddRange(tempCommands);
            boolResult = true;
        }

        //Search for a swapPlayerSpeaker command
        if(ParseSwapSpeakerRegex(dialogeString, out tempCommands)){
            resultCommands.AddRange(tempCommands);
            boolResult = true;
        }

        //,,,,Put other main line commands here when added

        return boolResult;
    }

    #region Specific Regex

    private bool ParseSetSpeakerRegex(string dialogeString, out List<DialogueCommand> resultCommands){
        
        resultCommands = new List<DialogueCommand>();
        bool boolResult = false;


        //Get all the matches and loop through them
        MatchCollection speakerMatches = setSpeakerRegex.Matches(dialogeString);
        foreach(Match match in speakerMatches){

            //Get the value written in the command
            bool commandInput = match.Groups["bool"].Value == "true";

            //For each match, make a new command
            resultCommands.Add( new DialogueCommand(
                commandType:    DialogueCommandType.setPlayerSpeaker,
                boolValue:      commandInput
            ));
            boolResult = true;
        }

        return boolResult;
    }

    private bool ParseSwapSpeakerRegex(string dialogeString, out List<DialogueCommand> resultCommands){
        
        resultCommands = new List<DialogueCommand>();
        bool boolResult = false;


        //Get all the matches and loop through them
        MatchCollection speakerMatches = swapSpeakerRegex.Matches(dialogeString);
        foreach(Match match in speakerMatches){

            //For each match, make a new command
            resultCommands.Add( new DialogueCommand(
                commandType:    DialogueCommandType.swapSpeaker
            ));
            boolResult = true;
        }

        
        return boolResult;
    }

    #endregion

    #endregion

    #region Main Parsing

    public DialogueComponent ParseDialogueString(string dialogeString){
        List<DialogueCommand> dialogueCommands = ParseAllRegex(dialogeString, out string processedDialogueString);

        return new DialogueComponent(processedDialogueString, dialogueCommands);
    }


    private List<DialogueCommand> ParseAllRegex(string dialogeString, out string processedDialogueString){

        List<DialogueCommand> newCommands = new List<DialogueCommand>();
        List<DialogueCommand> tempCommands = new List<DialogueCommand>();
        processedDialogueString = dialogeString;
        
        //Parse for anim start regex
        processedDialogueString = ParseAnimStartRegex(processedDialogueString, out tempCommands);
        newCommands.AddRange(tempCommands);

        //Parse for anim end regex
        processedDialogueString = ParseAnimEndRegex(processedDialogueString, out tempCommands);
        newCommands.AddRange(tempCommands);

        //Parse for pause regex
        processedDialogueString = ParsePauseRegex(processedDialogueString, out tempCommands);
        newCommands.AddRange(tempCommands);

        
        return newCommands;
    }

    #region Specific Regex Parsing

    private string ParseAnimStartRegex(string dialogeString, out List<DialogueCommand> newCommands){        
        newCommands = new List<DialogueCommand>();

        //Get all the matches and loop through them
        MatchCollection animStartMatches = animStartRegex.Matches(dialogeString);
        foreach(Match match in animStartMatches){

            //Get the value written in the command
            string commandInput = match.Groups["anim"].Value;


            //For each match, make a new command
            newCommands.Add(new DialogueCommand(
                commandType:    DialogueCommandType.anim_start,
                charIndex:      GetRealPositionInString(dialogeString, match.Index),
                animationType:  GetTextAnimationType(commandInput)
            ));
        }

        //Remove all the matches from the dialogue string
        dialogeString = Regex.Replace(dialogeString, ANIM_START_REGEX_STRING, "");

        return dialogeString;
    }

    private string ParseAnimEndRegex(string dialogeString, out List<DialogueCommand> newCommands){
        newCommands = new List<DialogueCommand>();

        //Get all the matches and loop through them
        MatchCollection animEndMatches = animEndRegex.Matches(dialogeString);
        foreach(Match match in animEndMatches){

            //For each match, make a new command
            newCommands.Add(new DialogueCommand(
                commandType:    DialogueCommandType.anim_end,
                charIndex:      GetRealPositionInString(dialogeString, match.Index)
            ));
        }

        //Remove all the matches from the dialogue string
        dialogeString = Regex.Replace(dialogeString, ANIM_END_REGEX_STRING, "");

        return dialogeString;
    }

    private string ParsePauseRegex(string dialogeString, out List<DialogueCommand> newCommands){
        newCommands = new List<DialogueCommand>();

        //Get all the matches and loop through them
        MatchCollection pauseMatches = pauseRegex.Matches(dialogeString);
        foreach(Match match in pauseMatches){

            //Get the value written in the command
            float commandInput = float.Parse(match.Groups["pause"].Value);

            //For each match, make a new command
            newCommands.Add(new DialogueCommand(
                commandType:    DialogueCommandType.pause,
                charIndex:      GetRealPositionInString(dialogeString, match.Index),
                floatValue:     commandInput
            ));
        }

        //Remove all the matches from the dialogue string
        dialogeString = Regex.Replace(dialogeString, PAUSE_REGEX_STRING, "");

        return dialogeString;
    }

    #endregion

    #region Helpers

    private int GetRealPositionInString(string dialogueString, int rawIndex){
        bool inBracket = false;
        int counter = 0;

        for(int i = 0; i < rawIndex; i++){

            if(dialogueString[i] == '<'){
                inBracket = true;
            }

            if(dialogueString[i] == '>'){
                inBracket = false;
                counter--;
            }

            if(!inBracket){
                counter++;
            }
        }

        return counter;
    }

    private TextAnimationType GetTextAnimationType(string animationTypeString){
        TextAnimationType animationType;
        try {
            animationType = (TextAnimationType)System.Enum.Parse(typeof(TextAnimationType), animationTypeString, true);
        } catch {
            animationType = TextAnimationType.none;
        }
        return animationType;
    }

    #endregion 

    #endregion
}
