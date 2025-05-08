using System;
using System.Collections.Generic;
using BattleshipWithWords.Services.GameManager;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.Game;

public class WordGuessedResponseData : IResponseData
{
    public TurnResult Result { get; set; }
    public System.Collections.Generic.Dictionary<char, LetterResponseStatus> ResponseLetters { get; set; }
    public List<string> WordLetterStatus { get; set; }
    public Dictionary Serialize()
    {
        var responseLetters = new Godot.Collections.Dictionary<char, Godot.Collections.Dictionary<string, Variant>>();
        foreach (var (letter, letterStatus) in ResponseLetters)
        {
            var keyboardStatus = (int)letterStatus.KeyboardStatus;
            Array<string> foundCoords = [];
            foreach (var (row, col, status) in letterStatus.FoundCoords)
            {
                foundCoords.Add($"{row},{col},{(int)status}");    
            }

            responseLetters[letter] = new Godot.Collections.Dictionary<string, Variant>
            {
                {"keyboardStatus", keyboardStatus},
                {"foundCoords", foundCoords}, 
            };
        }
        
        var wordLetterStatuses = new Array<string>();
        foreach (var word in WordLetterStatus)
        {
            wordLetterStatuses.Add(word);
        }
        var d = new Dictionary();
        d["TurnResult"] = (int)Result;
        d["ResponseLetterStatus"] = responseLetters;
        d["WordLetterStatus"] = wordLetterStatuses;

        return d;
    }
    
    public static WordGuessedResponseData Deserialize(Dictionary data)
    {
        var result = new WordGuessedResponseData
        {
            Result = (TurnResult)(int)data["TurnResult"],
            ResponseLetters = new System.Collections.Generic.Dictionary<char, LetterResponseStatus>()
        };

        var responseLetters = (Dictionary)data["ResponseLetterStatus"];
        foreach (char letter in responseLetters.Keys)
        {
            var letterData = (Dictionary)responseLetters[letter];
            var keyboardStatus = (KeyboardLetterStatus)(int)letterData["keyboardStatus"];

            var foundCoords = new List<(int, int, GameTileStatus)>();
            var coordsArray = (Godot.Collections.Array)letterData["foundCoords"];
            foreach (string coordStr in coordsArray)
            {
                var parts = coordStr.Split(',');
                int row = int.Parse(parts[0]);
                int col = int.Parse(parts[1]);
                int status = int.Parse(parts[2]);
                foundCoords.Add((row, col, (GameTileStatus)status));
            }

            result.ResponseLetters[letter] = new LetterResponseStatus
            {
                KeyboardStatus = keyboardStatus,
                FoundCoords = foundCoords
            };
        }

        result.WordLetterStatus = [];
        var wordStatusArray = (Godot.Collections.Array)data["WordLetterStatus"];
        foreach (string word in wordStatusArray)
        {
            result.WordLetterStatus.Add(word);
        }

        return result;
    }
}

public class UncoveredTileData : IEventData, IResponseData
{
    public int Row;
    public int Col;
    public Dictionary Serialize()
    {
        return new Dictionary()
        {
            { "row", Row },
            { "col", Col }
        };
    }

    public static UncoveredTileData Deserialize(Dictionary data)
    {
        return new UncoveredTileData()
        {
            Row = (int)data["row"],
            Col = (int)data["col"],
        };
    }
}

public enum ResponseType
{
    WordGuessed,
    TileUncovered,
}

public interface IResponseData
{
    public Dictionary Serialize();
}


public class PlayingResponseData : IPlayingData
{
    public ResponseType Type;
    public IResponseData Data;
    public Dictionary Serialize()
    {
        return new Dictionary
        {
            ["Type"] = (int)Type,
            ["Data"] = Data.Serialize()
        };
    }

    public static PlayingResponseData Deserialize(Dictionary responseData)
    {
       var type = (ResponseType)(int)responseData["Type"];
       IResponseData data = type switch
       {
           ResponseType.WordGuessed => WordGuessedResponseData.Deserialize((Dictionary)responseData["Data"]),
           ResponseType.TileUncovered => UncoveredTileResponse.Deserialize((Dictionary)responseData["Data"]),
           _ => throw new ArgumentOutOfRangeException()
       };
       return new PlayingResponseData()
       {
           Type = type,
           Data = data
       };
    }
}

public class GuessedWordData : IResponseData, IEventData
{
    public string Word;
    public Dictionary Serialize()
    {
        return new Dictionary
        {
            {"Word", Word},
        };
    }

    public static GuessedWordData Deserialize(Dictionary data)
    {
        return new GuessedWordData {Word = (string)data["Word"]};
    }
}






public class MultiplayerMessage
{
    public MultiplayerMessageType Type;
    public IMultiplayerMessageData Data;

    public MultiplayerMessage(MultiplayerMessageType type, IMultiplayerMessageData data)
    {
        Type = type;
        Data = data;
    }

    public static MultiplayerMessage From(Dictionary serialized)
    {
        var type = (MultiplayerMessageType)(int)serialized["Type"];
        var data = type switch
        {
            MultiplayerMessageType.Setup => null,
            MultiplayerMessageType.Playing => PlayingData.Deserialize((Dictionary)serialized["Data"]),
            _ => throw new ArgumentOutOfRangeException()
        };
        return  new MultiplayerMessage(type, data);
    }

    public Dictionary Serialize()
    {
        return new Dictionary()
        {
            {"Type", (int)Type},
            {"Data", Data?.Serialize()}
        };
    }
}

public interface IMultiplayerMessageData
{
    public Dictionary Serialize();
}

public class PlayingData : IMultiplayerMessageData
{
    public PlayingDataType Type;
    public IPlayingData Data;
    public Dictionary Serialize()
    {
        return new Dictionary()
        {
            {"Type", (int)Type},
            {"Data", Data?.Serialize()}
        };
    }

    public static PlayingData Deserialize(Dictionary serialized)
    {
        var type = (PlayingDataType)(int)serialized["Type"];
        IPlayingData data = type switch
        {
            PlayingDataType.Event => PlayingEventData.Deserialize((Dictionary)serialized["Data"]),
            PlayingDataType.Response => PlayingResponseData.Deserialize((Dictionary)serialized["Data"]),
            _ => throw new ArgumentOutOfRangeException()
        };
        return new PlayingData()
        {
            Type = type,
            Data = data 
        };
    }
}

/// <summary>
/// Can be GuessedWord, UncoveredTile, BackspacePressed, KeyPressed 
/// </summary>
public class PlayingEventData: IPlayingData
{
    public EventType Type;
    public IEventData Data;
    public Dictionary Serialize()
    {
        return new Dictionary
        {
            {"Type", (int)Type},
            {"Data", Data?.Serialize()}
        };
    }

    public static PlayingEventData Deserialize(Dictionary serialized)
    {
        var type = (EventType)(int)serialized["Type"];
        IEventData data = type switch
        {
            EventType.GuessedWord => GuessedWordData.Deserialize((Dictionary)serialized["Data"]),
            EventType.UncoveredTile => UncoveredTileData.Deserialize((Dictionary)serialized["Data"]),
            EventType.BackspacePressed => null,
            EventType.KeyPressed => EventKeyPressedData.Deserialize((Dictionary)serialized["Data"]),
            _ => throw new ArgumentOutOfRangeException()
        };
        return new PlayingEventData()
        {
            Type = type,
            Data = data
        };
    }
}

public class EventKeyPressedData : IEventData
{
    public string Key;
    public Dictionary Serialize()
    {
        return new Dictionary()
        {
            {"Key", Key}
        };
    }

    public static EventKeyPressedData Deserialize(Dictionary serialized)
    {
        return new EventKeyPressedData()
        {
            Key = (string)serialized["Key"],
        };
    }
}

public interface IEventData
{
    public Dictionary Serialize();
}

public enum EventType
{
    GuessedWord,
    UncoveredTile,
    BackspacePressed,
    KeyPressed
}

public interface IPlayingData
{
    public Dictionary Serialize();
}

public enum PlayingDataType
{
    Event,
    Response
}

public enum MultiplayerMessageType
{
    Setup,
    Playing
}
