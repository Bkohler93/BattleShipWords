using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleshipWithWords.Services.GameManager;

public class RemoteGameState
{
    private Dictionary<char, List<(int row, int col, int wordIndex, int letterIndexInWord)>> _letterToLocations;
    private List<HiddenWordState> _hiddenWords;
    private HashSet<(int row, int col)> _foundCoords;

    public ProcessGuessResponse ProcessWordGuess(string word)
    {
        List<StringBuilder> responseWordBuilders = [new("---"), new("----"), new("-----")];
        Dictionary<char, LetterResponseStatus> responseLetters = new();
        bool doesPlayerGoAgain = _hiddenWords.Exists(hw => hw.Word == word);
        
        for (var i = 0; i < word.Length; i++)
        {
            if (!responseLetters.ContainsKey(word[i]))
            {
                responseLetters.Add(word[i], new LetterResponseStatus());
            }
            var letterResponseStatus = responseLetters[word[i]];
            
            var letter = word[i];
            bool isLetterFound = true;
            bool isAllLetterWordsFound = true;
            var letterLocations = _letterToLocations[letter];
            if (letterLocations.Count == 0)
            {
                letterResponseStatus.KeyboardStatus = KeyboardLetterStatus.OutOfPlay;
                continue;
            }
            
            HashSet<int> hiddenWordIndicesContainingLetter = [];
            foreach (var letterLocation in letterLocations)
            {
                hiddenWordIndicesContainingLetter.Add(letterLocation.wordIndex);
                
                if (isLetterFound && letterLocation.letterIndexInWord != i)
                {
                    letterResponseStatus.KeyboardStatus = KeyboardLetterStatus.InPlay;
                    isLetterFound = false;
                    isAllLetterWordsFound = false; 
                }

                if (letterLocation.letterIndexInWord == i)
                {
                    responseWordBuilders[letterLocation.wordIndex][letterLocation.letterIndexInWord] = word[i];
                    _hiddenWords[letterLocation.wordIndex].LettersFound[i] = true;
                    letterResponseStatus.FoundCoords.Add((letterLocation.row, letterLocation.col));
                }
            }

            if (!isAllLetterWordsFound) continue;
            foreach (var index in hiddenWordIndicesContainingLetter)
            {
                if (!_hiddenWords[index].IsFound)
                {
                    isAllLetterWordsFound = false;
                    break;
                }
            }
                
            if (isAllLetterWordsFound)
                letterResponseStatus.KeyboardStatus = KeyboardLetterStatus.OutOfPlay;
            else
                letterResponseStatus.KeyboardStatus = KeyboardLetterStatus.InPlay;
        }
    }
}

class LetterResponseStatus
{
    public List<(int row, int col)> FoundCoords = [];
    public KeyboardLetterStatus KeyboardStatus;
}

enum KeyboardLetterStatus
{
    InPlay,
    AllFound,
    OutOfPlay,
}

class HiddenWordState
{
    public string Word;
    public List<(int x, int y)> Positions; // Same order as in Word
    public bool[] LettersFound;            // Same length as Word
    public bool IsFound => LettersFound.All(b => b);
}