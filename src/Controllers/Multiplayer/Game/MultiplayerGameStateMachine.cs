using System.Collections.Generic;
using System.Linq;
using BattleshipWithWords.Services.GameManager;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.Game;

public class MultiplayerGameManagerStateMachine
{
    private IMultiplayerGameManagerState _currentState;
    private MultiplayerGameManager _gameManager;
    
    public IMultiplayerGameManagerState CurrentState => _currentState;

    public MultiplayerGameManagerStateMachine(IMultiplayerGameManagerState currentState, MultiplayerGameManager gameManager)
    {
        _currentState = currentState;
        _gameManager = gameManager;
    }

    public bool TryProcessRemote(MultiplayerMessage msg)
    {
        return _currentState.TryProcessRemote(msg);
    }

    public void TransitionTo(IMultiplayerGameManagerState nextState)
    {
        _currentState?.Exit();
        _currentState = nextState;
        _currentState.Enter();
    }
}

public class SettingUp : IMultiplayerGameManagerState
{
    private MultiplayerGameManager _gameManager;

    public SettingUp(MultiplayerGameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public void Exit()
    {
    }

    public void Enter()
    {
    }

    public bool TryProcessRemote(MultiplayerMessage msg)
    {
        return false;
    }
}

public class SetupComplete : IMultiplayerGameManagerState
{
    private MultiplayerGameManager _gameManager;
    private MultiplayerGameManagerStateMachine _stateMachine;
    public SetupComplete(MultiplayerGameManager multiplayerGameManager, MultiplayerGameManagerStateMachine stateMachine)
    {
        _gameManager = multiplayerGameManager;
        _stateMachine = stateMachine;
    }

    public void Exit()
    {
        _gameManager.StartGame();
    }

    public void Enter()
    {
    }

    public bool TryProcessRemote(MultiplayerMessage msg)
    {
        if (msg.Type != MultiplayerMessageType.Setup) return false;
        
        if (_gameManager.LocalId == 1)
            _stateMachine.TransitionTo(new PlayingState(_stateMachine, _gameManager));
        else
            _stateMachine.TransitionTo(new OpponentPlayingState(_gameManager, _stateMachine));
        return true;
    }
}

public class OpponentPlayingState : IMultiplayerGameManagerState
{
    private MultiplayerGameManager _gameManager;
    private MultiplayerGameManagerStateMachine _stateMachine;

    public OpponentPlayingState(MultiplayerGameManager gameManager, MultiplayerGameManagerStateMachine stateMachine)
    {
        _gameManager = gameManager;
        _stateMachine = stateMachine;
    }

    public void Exit()
    {
    }

    public void Enter()
    {
    }

    public bool TryProcessRemote(MultiplayerMessage msg)
    {
        if (msg.Type == MultiplayerMessageType.Playing)
        {
            var playingData = msg.Data as PlayingData;
            switch (playingData.Type)
            {
                case PlayingDataType.Event:
                    var eventData = playingData.Data as PlayingEventData;
                    _processEvent(eventData);
                    break;
                case PlayingDataType.Response:
                    GD.Print($"OpponentPlayingState::ProcessRemote: Should not have gotten a response while opponent is playing");
                    break;
            }
            return true;
        }

        return false;
    }
    
    private void _processEvent(PlayingEventData eventData)
    {
        switch (eventData.Type)
        {
            case EventType.BackspacePressed:
                _gameManager.OpponentPressBackspace();
                break;
            case EventType.KeyPressed:
                var keyPressedData = eventData.Data as EventKeyPressedData;
                _gameManager.OpponentPressKey(keyPressedData!.Key);
                break;
            case EventType.GuessedWord:
                var guessedWordData = eventData.Data as GuessedWordData;
                _gameManager.ProcessOpponentWordGuess(guessedWordData!.Word);
                break;
            case EventType.UncoveredTile:
                var coords = eventData.Data as UncoveredTileData;
                _gameManager.ProcessOpponentUncoverTile(coords!.Row, coords.Col);
                break;
        }
    }
}

public enum TurnResult
{
    GoAgain,
    TurnOver,
    Win
}

public class PlayingState : IMultiplayerGameManagerState
{
    private MultiplayerGameManager _gameManager;
    private MultiplayerGameManagerStateMachine _stateMachine;
    public PlayingState(MultiplayerGameManagerStateMachine stateMachine, MultiplayerGameManager gameManager)
    {
        _stateMachine = stateMachine;
        _gameManager = gameManager;
    }

    public void Exit()
    {
    }

    public void Enter()
    {
    }

    public bool TryProcessRemote(MultiplayerMessage msg)
    {
        if (msg.Type == MultiplayerMessageType.Playing)
        {
            var playingData = msg.Data as PlayingData;
            switch (playingData.Type)
            {
                case PlayingDataType.Event:
                    var eventData = playingData.Data as PlayingEventData;
                    _processEvent(eventData);
                    break;
                case PlayingDataType.Response:
                    var respData = playingData.Data as PlayingResponseData;
                    _processResponse(respData);
                    break;
            }
            return true;
        }

        GD.Print($"MultiplayerGameManager::PlayingState: did not process msg of type {msg.Type}");
        return false;
    }
    
    private void _processEvent(PlayingEventData eventData)
    {
        switch (eventData.Type)
        {
            case EventType.BackspacePressed:
                _gameManager.OpponentPressBackspace();
                break;
            case EventType.KeyPressed:
                var keyPressedData = eventData.Data as EventKeyPressedData;
                _gameManager.OpponentPressKey(keyPressedData!.Key);
                break;
            case EventType.GuessedWord:
                GD.PrintErr($"MultiplayerGameStateMachine::PlayingState - Event GuessedWord received during PlayingState"); 
                break;
            case EventType.UncoveredTile:
                GD.PrintErr($"MultiplayerGameStateMachine::PlayingState - Event UncoveredTile received during PLayingState");
                break;
            //TODO  UncoveredTile
        }
    }

    private void _processResponse(PlayingResponseData respData)
    {
        switch (respData.Type)
        {
            case ResponseType.WordGuessed:
                var wordGuessData = respData.Data as WordGuessedResponseData;
                _gameManager.UpdateLocalGameFromWordGuess(wordGuessData);
                break;
            case ResponseType.TileUncovered:
                var uncoverTileData = respData.Data as UncoveredTileResponse;
                _gameManager.UpdateLocalFromUncoverTile(uncoverTileData);
                break;
        }
    }

    
}

public class UncoveredTileResponse : IResponseData
{
    public List<string> WordLetterStatus;
    public List<(int row, int col, string letter, GameTileStatus status)> GameboardStatus;
    public System.Collections.Generic.Dictionary<char, KeyboardLetterStatus> KeyboardStatus;
    public TurnResult Result;
    
    public Dictionary Serialize()
    {
        Array<string> wordLetterStatus = [];
        foreach (var word in WordLetterStatus)
        {
            wordLetterStatus.Add(word);
        }

        Godot.Collections.Dictionary<char, int> keyboardStatus = new();
        foreach (var (letter, status) in KeyboardStatus)
        {
            keyboardStatus[letter] = (int)status;
        }

        Array<string> gameboardStatus = [];
        foreach (var (row, col, letter, status) in GameboardStatus)
        {
            gameboardStatus.Add($"{row},{col},{letter},{(int)status}");
        }
        
        return new Dictionary()
        {
            {"GameboardStatus", gameboardStatus},
            {"KeyboardStatus", keyboardStatus},
            {"WordLetterStatus", wordLetterStatus},
            { "TurnResult", (int)Result}
        };
    }

    public static UncoveredTileResponse Deserialize(Dictionary dictionary)
    {
        var wordLetterStatus = ((Array<string>)dictionary["WordLetterStatus"]).ToList();
        var keyboardStatus = new System.Collections.Generic.Dictionary<char, KeyboardLetterStatus>();
        foreach (var (letter, status) in (Godot.Collections.Dictionary<char, int>)dictionary["KeyboardStatus"])
        {
            keyboardStatus[letter] = (KeyboardLetterStatus)status;
        }

        var gameboardStatus = new List<(int row, int col, string letter, GameTileStatus status)>();
        foreach (var status in (Godot.Collections.Array<string>)dictionary["GameboardStatus"])
        {
            var parts = status.Split(',');
            var row = int.Parse(parts[0]);
            var col = int.Parse(parts[1]);
            var letter = parts[2];
            var tileStatus = (GameTileStatus)int.Parse(parts[3]);
            gameboardStatus.Add((row, col, letter, tileStatus));
        }

        return new UncoveredTileResponse()
        {
            WordLetterStatus = wordLetterStatus,
            GameboardStatus = gameboardStatus,
            KeyboardStatus = keyboardStatus,
            Result = (TurnResult)((int)dictionary["TurnResult"]),
        };
    }
}

public class GameOverState : IMultiplayerGameManagerState
{
    private MultiplayerGameManagerStateMachine _stateMachine;
    private MultiplayerGameManager _gameManager;
    private bool _didWin; 
    public GameOverState(MultiplayerGameManagerStateMachine stateMachine, MultiplayerGameManager multiplayerGameManager, bool didWin)
    {
        _stateMachine = stateMachine;
        _gameManager = multiplayerGameManager;
        _didWin = didWin;
    }

    public void Exit()
    {
    }

    public void Enter()
    {
        if (_didWin)
        {
            _gameManager.OnWin();
        }
        else
        {
            _gameManager.OnLose();
        }
    }

    public bool TryProcessRemote(MultiplayerMessage msg)
    {
        return true;
    }
}





public interface IMultiplayerGameManagerState
{
    public void Exit();
    public void Enter();
    bool TryProcessRemote(MultiplayerMessage msg);
}
