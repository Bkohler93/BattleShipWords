using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Utilities;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Services.GameManager;

public class RemoteGameStateManager
{
    private System.Collections.Generic.Dictionary<string, List<(int row, int col, int wordIndex, int letterIndexInWord)>> _letterToLocations;
    private List<HiddenWordState> _hiddenWords;
    private HashSet<(int row, int col)> _foundCoords = new();

    public static RemoteGameStateManager FromSetup(List<string> words, List<List<(int row, int col)>> boardSelections)
    { 
        var letterToLocations = new System.Collections.Generic.Dictionary<string, List<(int row, int col, int wordIndex, int letterIndexInWord)>>();
        var hiddenWords = new List<HiddenWordState>(); 
        var foundCoords = new HashSet<(int row, int col)>();
        
        for (var i = 0; i < 3; i++)
        {
            var word = words[i];
            var hiddenWordState = new HiddenWordState(word);
            hiddenWords.Add(hiddenWordState);
            var wordBoardSelections = boardSelections[i];
            for (var j = 0; j < word.Length; j++)
            {
                hiddenWordState.Positions.Add((wordBoardSelections[j].row, wordBoardSelections[j].col));
                if (letterToLocations.ContainsKey(word[j].ToString()))
                {
                    var letterLocations = letterToLocations[word[j].ToString()];
                    letterLocations.Add((wordBoardSelections[j].row, wordBoardSelections[j].col, i, j));
                }
                else
                {
                    letterToLocations[word[j].ToString()] = [(wordBoardSelections[j].row, wordBoardSelections[j].col, i, j)];
                }
            }
        }
        return new RemoteGameStateManager()
        {
            _letterToLocations = letterToLocations,
            _hiddenWords = hiddenWords,
            _foundCoords = foundCoords,
        };
    }

    public override string ToString()
    {
        var s = "";
        for (var i = 0; i < 3; i++)
        {
            s += $"{_hiddenWords[i].Word}: LettersFound=(";
            foreach (var b in _hiddenWords[i].LettersFound)
            {
                s += $"{b},";
            }

            s += ")\n";

            s += "WordPositions=(";
            foreach (var (row, col) in _hiddenWords[i].Positions)
            {
                s += $"{row}:{col},";
            }
            s += ")\n";
        }

        s += "Character Locations\n";
        foreach (var (c, locations) in _letterToLocations)
        {
            s += $"{c}=[";
            foreach (var (row, col, wordIndex, letterIndexInWord) in locations)
            {
                s += $"({row},{col},{wordIndex},{letterIndexInWord}),";
            }
            s += "]\n";
        }
        
        return s;
    }

    public GuessResponse ProcessWordGuess(string word)
    {
        List<StringBuilder> responseWordBuilders = [new("---"), new("----"), new("-----")];
        System.Collections.Generic.Dictionary<string, LetterResponseStatus> responseLetters = new();
        bool doesPlayerGoAgain = _hiddenWords.Exists(hw => hw.Word == word);
        
        // find letters on game board
        for (var guessWordIndex = 0; guessWordIndex < word.Length; guessWordIndex++)
        {
            if (!responseLetters.ContainsKey(word[guessWordIndex].ToString()))
            {
                responseLetters.Add(word[guessWordIndex].ToString(), new LetterResponseStatus());
            }
            
            var letter = word[guessWordIndex].ToString();
            if (!_letterToLocations.TryGetValue(letter, out var letterLocations))
                continue;

            foreach (var letterLocation in letterLocations.Where(letterLocation => letterLocation.letterIndexInWord == guessWordIndex))
            {
                _foundCoords.Add((letterLocation.row, letterLocation.col));
                _hiddenWords[letterLocation.wordIndex].LettersFound[guessWordIndex] = true;
                responseWordBuilders[letterLocation.wordIndex][letterLocation.letterIndexInWord] = word[guessWordIndex];
            }
        }
        
        // determine keyboard and tile statuses
        for (var i=0;i<word.Length;i++)
        {
            var letter = word[i].ToString();
            var letterResponseStatus = responseLetters[letter];
            
            if (!_letterToLocations.TryGetValue(letter, out var letterLocations))
            {
                letterResponseStatus.KeyboardStatus = KeyboardLetterStatus.OutOfPlay;
                continue;
            }
            
            HashSet<int> hiddenWordIndicesContainingLetter = [];
            foreach (var letterLocation in letterLocations)
            {
                hiddenWordIndicesContainingLetter.Add(letterLocation.wordIndex);
            }
            
            foreach (var letterLocation in letterLocations.Where(letterLocation => letterLocation.letterIndexInWord == i))
            {
                var status = _hiddenWords[letterLocation.wordIndex].IsFound ? GameTileStatus.WordFound : GameTileStatus.Uncovered;
                var letterFoundCoord = new LetterFoundCoordinate
                {
                    Coordinate = new Coordinate
                    {
                        Row = letterLocation.row,
                        Col = letterLocation.col,
                    },
                    GameTileStatus =status 
                };
                
                // letterResponseStatus.UncoveredGameTiles.Add((letterLocation.row, letterLocation.col, status));
                letterResponseStatus.UncoveredGameTiles.Add(letterFoundCoord);
            }
            
            KeyboardLetterStatus keyboardLetterStatus;
            
            // check if all hidden words containing letter have been found
            if (hiddenWordIndicesContainingLetter.All(index => _hiddenWords[index].IsFound))
            {
                keyboardLetterStatus = KeyboardLetterStatus.OutOfPlay;
            }
            // check if all instances of letter have been found
            else if (letterLocations.All(tuple => _foundCoords.Contains((tuple.row, tuple.col))))
            {
                keyboardLetterStatus = KeyboardLetterStatus.AllFound;
            }
            else
            {
                keyboardLetterStatus = KeyboardLetterStatus.InPlay;
            }
            // letterResponseStatus.KeyboardStatus = hiddenWordIndicesContainingLetter.Any(index => !_hiddenWords[index].IsFound) ? KeyboardLetterStatus.InPlay : KeyboardLetterStatus.OutOfPlay;
            letterResponseStatus.KeyboardStatus = keyboardLetterStatus;
        }

        TurnResult result;
        if (_hiddenWords.All(hiddenWord => hiddenWord.IsFound))
        {
            result = TurnResult.Win;
        }
        else if (doesPlayerGoAgain)
        {
            result = TurnResult.GoAgain;
        } 
        else
        {
            result = TurnResult.TurnOver;
        }
        
        GD.Print(result);
        return new GuessResponse()
        {
            WordLetterStatus = responseWordBuilders.ConvertAll(b => b.ToString()),
            ResponseLetters = responseLetters,
            Result = result,
        };
    }

//     public UncoveredTileResponse ProcessTileUncovered(int coordsRow, int coordsCol)
//     {
//         List<StringBuilder> responseWordBuilders = [new("---"), new("----"), new("-----")];
//         var keyboardStatus = new System.Collections.Generic.Dictionary<char, KeyboardLetterStatus>();
//         var gameboardStatus = new List<(int row, int col, string letter, GameTileStatus status)>();
//         (int row, int col, string letter, GameTileStatus status) guessedTileStatus = (coordsRow, coordsCol, "", GameTileStatus.Uncovered);
//
//         _foundCoords.Add((coordsRow, coordsCol));
//         foreach (var (letter, letterToLocation) in _letterToLocations)
//         {
//             if (!letterToLocation.Exists((tuple => tuple.row == coordsRow && tuple.col == coordsCol))) continue;
//             guessedTileStatus.letter = letter.ToString();
//             var tuple = letterToLocation.Find(tuple => tuple.row == coordsRow && tuple.col == coordsCol);
//             _hiddenWords[tuple.wordIndex].LettersFound[tuple.letterIndexInWord] = true;
//             if (!_hiddenWords[tuple.wordIndex].IsFound)
//             {
//                 var letterLocations = _letterToLocations[letter];
//                 // find if all instances of this letter have been found
//                 var allInstancesOfLetterFound = true;
//                 foreach (var letterLocation in letterLocations)
//                 {
//                     if (!_hiddenWords[letterLocation.wordIndex].LettersFound[letterLocation.letterIndexInWord])
//                         allInstancesOfLetterFound = false;
//                 }
//                 if (allInstancesOfLetterFound)
//                     keyboardStatus[letter] = KeyboardLetterStatus.AllFound;
//                 else
//                     keyboardStatus[letter] = KeyboardLetterStatus.InPlay;
//                 break;
//             } 
//             
//             var hiddenWord = _hiddenWords[tuple.wordIndex].Word;
//             
//             for (var i = 0; i < hiddenWord.Length; i++)
//             {
//                 gameboardStatus.Add((_hiddenWords[tuple.wordIndex].Positions[i].row,_hiddenWords[tuple.wordIndex].Positions[i].col, hiddenWord[i].ToString(), GameTileStatus.WordFound));
//             }
//             
//             for (var i = 0; i < hiddenWord.Length; i++)
//             {
//                 var hiddenWordLetter = hiddenWord[i];
//                     
//                 var letterLocations = _letterToLocations[hiddenWordLetter];
//                     
//                 // find if all hidden words that contain have been found
//                 var uniqueWordIndices = new HashSet<int>();
//                 foreach (var letterLocation in letterLocations)
//                     uniqueWordIndices.Add(letterLocation.wordIndex);
//                 var allHiddenWordsWithLetterFound = true;
//                 foreach (var uniqueWordIndex in uniqueWordIndices)
//                 {
//                     if (!_hiddenWords[uniqueWordIndex].IsFound)
//                         allHiddenWordsWithLetterFound = false;
//                 }
//
//                 if (allHiddenWordsWithLetterFound)
//                 {
//                     keyboardStatus[hiddenWordLetter] = KeyboardLetterStatus.OutOfPlay;
//                     continue;
//                 }
//                     
//                 // find if all instances of this letter have been found
//                 var allInstancesOfLetterFound = true;
//                 foreach (var letterLocation in letterLocations)
//                 {
//                     if (!_hiddenWords[letterLocation.wordIndex].LettersFound[letterLocation.letterIndexInWord])
//                         allInstancesOfLetterFound = false;
//                 }
//                 if (allInstancesOfLetterFound)
//                     keyboardStatus[hiddenWordLetter] = KeyboardLetterStatus.AllFound;
//                 else
//                     keyboardStatus[hiddenWordLetter] = KeyboardLetterStatus.InPlay;
//             }
//         }
//         if (gameboardStatus.Count == 0)
//             gameboardStatus.Add(guessedTileStatus);
//
//         var result = _hiddenWords.All(hiddenWord => hiddenWord.IsFound) ? TurnResult.Win : TurnResult.TurnOver;
//         
//         return new UncoveredTileResponse()
//         {
//             WordLetterStatus = responseWordBuilders.ConvertAll(b => b.ToString()),
//             GameboardTileStatus = gameboardStatus,
//             KeyboardStatus = keyboardStatus,
//             Result = result,
//         };
//     }
}



public class GuessResponse
{
    public List<string> WordLetterStatus; // will have '-' for letters not yet found, and actual letters if found
    public System.Collections.Generic.Dictionary<string, LetterResponseStatus> ResponseLetters = new(); 
    public TurnResult Result;

    public override string ToString()
    {
        var s = "Guess Response\n\nLetter Statuses\n";
        foreach (var (letter, status) in ResponseLetters)
        {
            var keyboardStatus = status.KeyboardStatus;
            s += $"{letter}: {keyboardStatus},";
        }

        return s;
    }
}


public class LetterResponseStatus
{
    // public List<(int row, int col, GameTileStatus status)> FoundCoords = [];
    [JsonPropertyName("uncovered_game_tiles")]
    public List<LetterFoundCoordinate> UncoveredGameTiles { get; set; } = [];
    
    [JsonPropertyName("keyboard_status")]
    public KeyboardLetterStatus KeyboardStatus { get; set; }
}

public class LetterFoundCoordinate
{
    [JsonPropertyName("coordinate")]
    public Coordinate Coordinate { get; set; }
    [JsonPropertyName("game_tile_status")]
    public GameTileStatus GameTileStatus { get; set; }
}

public enum GameTileStatus
{
    Uncovered,
    WordFound,
}

public enum KeyboardLetterStatus
{
    InPlay,
    AllFound,
    OutOfPlay,
}

class HiddenWordState
{
    public string Word;
    public List<(int row, int col)> Positions = []; // Same order as in Word
    public bool[] LettersFound;            // Same length as Word

    public HiddenWordState(string word)
    {
        Word = word;
        LettersFound = word.Select(c => false).ToArray();
    }

    public bool IsFound => LettersFound.All(b => b);
}

