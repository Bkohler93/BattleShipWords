using System;
using System.Collections.Generic;
using System.Linq;
using androidplugintest.ConnectionManager;
using BattleshipWithWords.Controllers.Multiplayer.Setup;
using BattleshipWithWords.Services.GameManager;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.Game;

public class MultiplayerGameManagerStateMachine
{
    private MultiplayerGameManagerState _currentState;
    private MultiplayerGameManager _gameManager;
    
    public MultiplayerGameManagerState CurrentState => _currentState;

    public MultiplayerGameManagerStateMachine(MultiplayerGameManager gameManager)
    {
        _gameManager = gameManager;
        TransitionTo(new SettingUp(_gameManager, this));
    }

    public void ProcessLocalUpdate(UIEvent @event)
    {
        _currentState.ProcessLocalUpdate(@event);
    }

    // public bool TryProcessRemote(MultiplayerMessage msg)
    // {
    //     return _currentState.TryProcessRemote(msg);
    // }

    public void TransitionTo(MultiplayerGameManagerState nextState)
    {
        _currentState?.Exit();
        _currentState = nextState;
        _currentState.Enter();
    }
}

public class SettingUp : MultiplayerGameManagerState
{
    private MultiplayerGameManager _gameManager;
    private MultiplayerGameManagerStateMachine _stateMachine;
    private bool _finishedSettingUp;
    private bool _peerFinishedSettingUp;

    public SettingUp(MultiplayerGameManager gameManager, MultiplayerGameManagerStateMachine stateMachine)
    {
        _gameManager = gameManager;
        _gameManager.SetConnectionListener(this);
        _stateMachine = stateMachine;
    }

    public override void Exit()
    {
        _gameManager.StartGame();
    }

    public override void Enter()
    {
    }

    public override void Connected()
    {
        throw new System.NotImplementedException();
    }

    public override void UnableToConnect()
    {
        throw new System.NotImplementedException();
    }

    public override void Reconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void ReconnectFailed()
    {
        throw new System.NotImplementedException();
    }

    public override void Disconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void Receive(Dictionary message)
    {
        var msg = MultiplayerMessage.FromDictionary(message);
        if (msg.Type != MultiplayerMessageType.Setup)
        {
            GD.Print($"MultiplayerGameManager::SettingUp: did not process msg of type {msg.Type}");
            return;
        }

        if (_finishedSettingUp)
        {
            _completeSetup();
            _gameManager.SetupUpdated?.Invoke(SetupSceneUpdate.SetupComplete);   
        } else
            _peerFinishedSettingUp = true;
    }

    public override void ProcessLocalUpdate(UIEvent @event)
    {
        if (@event.Type == EventType.SetupCompleted)
        {
            if (@event.Data is not SetupCompletedEventData data)
                throw new Exception("MultiplayerGameManager::SettingUp::ProcessLocalUpdate: data is not SetupCompleteData");
            _gameManager.PlayerTwoGameStateManager = RemoteGameStateManager.FromSetup(data.SelectedWords, data.SelectedGridCoordinates);
            _gameManager.SendSetupComplete();

            if (_peerFinishedSettingUp)
            {
                _completeSetup();
                _gameManager.SetupUpdated?.Invoke(SetupSceneUpdate.SetupComplete);   
            }
            else
            {
                _finishedSettingUp = true;
                _gameManager.SetupUpdated?.Invoke(SetupSceneUpdate.WaitingForOtherPlayer);   
            }
        }
        else
        {
            throw new Exception("MultiplayerGameManager::SettingUp::ProcessLocalUpdate: received UIEvent of incorrect type");        
        }
    }

    private void _completeSetup()
    {
        if (_gameManager.IsHost())
        {
            GD.Print("Completed setup, transitioning into PlayingState");
            _stateMachine.TransitionTo(new PlayingState(_stateMachine, _gameManager));
        }
        else
        {
            GD.Print("Completed setup, transitioning into OpponentPlayingState");
            _stateMachine.TransitionTo(new OpponentPlayingState(_gameManager, _stateMachine));
        }
    }
}

public class OpponentPlayingState : MultiplayerGameManagerState
{
    private MultiplayerGameManager _gameManager;
    private MultiplayerGameManagerStateMachine _stateMachine;

    public OpponentPlayingState(MultiplayerGameManager gameManager, MultiplayerGameManagerStateMachine stateMachine)
    {
        _gameManager = gameManager;
        _gameManager.SetConnectionListener(this);
        _stateMachine = stateMachine;
    }

    public override void Exit()
    {
    }

    public override void Enter()
    {
    }

    public override void Reconnected()
    {
        throw new NotImplementedException();
    }

    public override void ReconnectFailed()
    {
        throw new NotImplementedException();
    }

    public override void Disconnected()
    {
        throw new NotImplementedException();
    }

    public override void Receive(Dictionary message)
    {
        var msg = MultiplayerMessage.FromDictionary(message);
        if (msg.Type == MultiplayerMessageType.Playing)
        {
            var playingData = msg.Data as PlayingData;
            switch (playingData!.Type)
            {
                case PlayingDataType.Event:
                    var eventData = playingData.Data as PlayingEventData;
                    _processEvent(eventData);
                    break;
                case PlayingDataType.Response:
                    GD.Print("OpponentPlayingState::ProcessRemote: Should not have gotten a response while opponent is playing");
                    break;
            }
        }
        else
        {
            GD.Print($"MultiplayerGameManager::OpponentPlayingState: did not process msg of type {msg.Type}");
        }
    }

    public override void ProcessLocalUpdate(UIEvent @event)
    {
        switch (@event.Type)
        {
            case EventType.SetupCompleted:
                GD.PrintErr($"MultiplayerGameStateMachine::PlayingState - ProcessLocalUpdate received SetupCompleted event.");
                break;
            case EventType.GuessedWord:
                break;
            case EventType.UncoveredTile:
                break;
            case EventType.KeyPressed:
                var data = (@event.Data as EventKeyPressedData);
                GD.Print($"ProcessLocalUpdate: KeyPressed={data.Key}");
                _gameManager.UpdateLocalUI(new UIUpdate
                {
                    Type = UIUpdateType.KeyPressed,
                    Data = new KeyData(data.Key), 
                });
                _gameManager.SendPlayingEvent(@event.Type, @event.Data as EventKeyPressedData);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void _processEvent(PlayingEventData eventData)
    {
        TurnResult? result = null;
        switch (eventData.Type)
        {
            case EventType.BackspacePressed:
            {
                _gameManager.UpdateOpponentUI(new UIUpdate
                {
                    Type = UIUpdateType.KeyPressed,
                    Data = new KeyData("backspace") 
                });
                break;
            }
            case EventType.KeyPressed:
            {
                if (eventData.Data is not EventKeyPressedData keyPressedData)
                    throw new Exception("OpponentPlayingState: invalid KeyPressed Data received in _processEvent");
                _gameManager.UpdateOpponentUI(new UIUpdate
                {
                    Type = UIUpdateType.KeyPressed,
                    Data = new KeyData(keyPressedData.Key) 
                });
                break;
            }
            case EventType.GuessedWord:
            {
                if (eventData.Data is not GuessedWordData guessedWordData)
                    throw new Exception("OpponentPlayingState: invalid WordGuess Data received in _processEvent");
                var res = _processWordGuess(guessedWordData.Word);
                _gameManager.SendPlayingResponse(ResponseType.WordGuessed, res);
                _gameManager.UpdateOpponentUI(new UIUpdate
                {
                    Type = UIUpdateType.GuessedWord,
                    Data = res, 
                });
                result = res.Result;
                if (res.Result == TurnResult.TurnOver)
                    _stateMachine.TransitionTo(new PlayingState(_stateMachine, _gameManager));
                else if (res.Result == TurnResult.Win)
                    _stateMachine.TransitionTo(new GameOverState(_stateMachine, _gameManager, false));
                break; 
            }
            case EventType.UncoveredTile:
            {
                if (eventData.Data is not UncoveredTileData uncoveredTileData)
                    throw new Exception("OpponentPlayingState: invalid UncoverTile Data received in _processEvent");
                var res = _processUncoverTile(uncoveredTileData.Row, uncoveredTileData.Col);
                _gameManager.SendPlayingResponse(ResponseType.TileUncovered, res);
                _gameManager.UpdateOpponentUI(new UIUpdate
                {
                    Type = UIUpdateType.UncoveredTile,
                    Data = res 
                });
                result = res.Result;
                break;
            }
        }

        if (result != null)
        {
            switch (result)
            {
                case TurnResult.GoAgain:
                    break;
                case TurnResult.TurnOver:
                    _stateMachine.TransitionTo(new PlayingState(_stateMachine, _gameManager));
                    break;
                case TurnResult.Win:
                    _stateMachine.TransitionTo(new GameOverState(_stateMachine, _gameManager, false));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private UncoveredTileResponse _processUncoverTile(int row, int col)
    {
        var res = _gameManager.PlayerTwoGameStateManager.ProcessTileUncovered(row, col);
        return res;
    }

    private WordGuessedResponseData _processWordGuess(string word)
    {
        var res = _gameManager.PlayerTwoGameStateManager.ProcessWordGuess(word);

        var data = new WordGuessedResponseData()
        {
            Result = res.Result,
            ResponseLetters = res.ResponseLetters,
            WordLetterStatus = res.WordLetterStatus,
        };
        return data;
    }
}

public enum TurnResult
{
    GoAgain,
    TurnOver,
    Win
}

public class PlayingState : MultiplayerGameManagerState
{
    private MultiplayerGameManager _gameManager;
    private MultiplayerGameManagerStateMachine _stateMachine;
    public PlayingState(MultiplayerGameManagerStateMachine stateMachine, MultiplayerGameManager gameManager)
    {
        _stateMachine = stateMachine;
        _gameManager = gameManager;
        _gameManager.SetConnectionListener(this);
    }

    public override void Exit()
    {
    }

    public override void Enter()
    {
    }

    

    public override void Reconnected()
    {
        throw new NotImplementedException();
    }

    public override void Receive(Dictionary message)
    {
        var msg = MultiplayerMessage.FromDictionary(message); 
        if (msg.Type == MultiplayerMessageType.Playing)
        {
            var playingData = msg.Data as PlayingData;
            switch (playingData!.Type)
            {
                case PlayingDataType.Event:
                    var eventData = playingData.Data as PlayingEventData;
                    _processEvent(eventData);
                    break;
                case PlayingDataType.Response:
                    var respData = playingData.Data as PlayingResponseData;
                    var result = _processResponse(respData);
                    if (result == null) break;
                    switch (result)
                    {
                        case TurnResult.GoAgain:
                            break;
                        case TurnResult.TurnOver:
                            _stateMachine.TransitionTo(new OpponentPlayingState(_gameManager, _stateMachine));
                            break;
                        case TurnResult.Win:
                            _stateMachine.TransitionTo(new GameOverState(_stateMachine, _gameManager, true));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
            }
        }
        else
        {
            GD.Print($"MultiplayerGameManager::PlayingState: did not process msg of type {msg.Type}");
        } 

    }

    public override void ProcessLocalUpdate(UIEvent @event)
    {
        switch (@event.Type)
        {
            case EventType.SetupCompleted:
                GD.PrintErr($"MultiplayerGameStateMachine::PlayingState - ProcessLocalUpdate received SetupCompleted event.");
                break;
            case EventType.GuessedWord:
                _gameManager.SendPlayingEvent(@event.Type, @event.Data as GuessedWordData);
                break;
            case EventType.UncoveredTile:
                _gameManager.SendPlayingEvent(@event.Type, @event.Data as UncoveredTileData);
                break;
            case EventType.KeyPressed:
                var data = (@event.Data as EventKeyPressedData);
                GD.Print($"ProcessLocalUpdate: KeyPressed={data.Key}");
                _gameManager.UpdateLocalUI(new UIUpdate
                {
                    Type = UIUpdateType.KeyPressed,
                    Data = new KeyData(data.Key), 
                });
                _gameManager.SendPlayingEvent(@event.Type, @event.Data as EventKeyPressedData);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override void ReconnectFailed()
    {
        throw new NotImplementedException();
    }

    public override void Disconnected()
    {
        throw new NotImplementedException();
    }

    private void _processEvent(PlayingEventData eventData)
    {
        switch (eventData.Type)
        {
            case EventType.BackspacePressed:
            {
                _gameManager.UpdateOpponentUI(new UIUpdate
                {
                    Type = UIUpdateType.KeyPressed,
                    Data = new KeyData("backspace") 
                });
                break;
            }
            case EventType.KeyPressed:
            {
                if (eventData.Data is not EventKeyPressedData keyPressedData)
                    throw new Exception("OpponentPlayingState: invalid KeyPressed Data received in _processEvent");
                _gameManager.UpdateOpponentUI(new UIUpdate
                {
                    Type = UIUpdateType.KeyPressed,
                    Data = new KeyData(keyPressedData.Key) 
                });
                break;
            }
            case EventType.GuessedWord:
                GD.PrintErr($"MultiplayerGameStateMachine::PlayingState - Event GuessedWord received during PlayingState"); 
                break;
            case EventType.UncoveredTile:
                GD.PrintErr($"MultiplayerGameStateMachine::PlayingState - Event UncoveredTile received during PLayingState");
                break;
        }
    }

    private TurnResult? _processResponse(PlayingResponseData respData)
    {
        TurnResult result;
        if (respData.Type == ResponseType.WordGuessed)
        {
            var wordGuessData = respData.Data as WordGuessedResponseData;
            result = wordGuessData.Result;
            _gameManager.UpdateLocalUI(new UIUpdate
            {
                Type = UIUpdateType.GuessedWord,
                Data = wordGuessData
            });
            // _gameManager.UpdateLocalGameFromWordGuess(wordGuessData);
        }
        else if (respData.Type == ResponseType.TileUncovered)
        {
            var uncoverTileData = respData.Data as UncoveredTileResponse;
            result = uncoverTileData.Result;
            _gameManager.UpdateLocalUI(new UIUpdate
            {
                Type = UIUpdateType.UncoveredTile,
                Data = uncoverTileData,
            });
            // _gameManager.UpdateLocalFromUncoverTile(uncoverTileData);
        }
        else
        {
            GD.PrintErr("MultiplayerGameStateMachine::PlayingState:_processResponse processed invalid TurnResult");
            result = TurnResult.TurnOver;
        }

        return result;
    }
}

public class UncoveredTileResponse : UIUpdateData, IResponseData 
{
    public List<string> WordLetterStatus;
    public List<(int row, int col, string letter, GameTileStatus status)> GameboardStatus;
    public System.Collections.Generic.Dictionary<char, KeyboardLetterStatus> KeyboardStatus;
    public TurnResult Result;
    
    public Dictionary ToDictionary()
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

    public static UncoveredTileResponse FromDictionary(Dictionary dictionary)
    {
        var wordLetterStatus = ((Array<string>)dictionary["WordLetterStatus"]).ToList();
        var keyboardStatus = new System.Collections.Generic.Dictionary<char, KeyboardLetterStatus>();
        foreach (var (letter, status) in (Godot.Collections.Dictionary<char, int>)dictionary["KeyboardStatus"])
        {
            keyboardStatus[letter] = (KeyboardLetterStatus)status;
        }

        var gameboardStatus = new List<(int row, int col, string letter, GameTileStatus status)>();
        foreach (var status in (Array<string>)dictionary["GameboardStatus"])
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

public class GameOverState : MultiplayerGameManagerState
{
    private MultiplayerGameManagerStateMachine _stateMachine;
    private MultiplayerGameManager _gameManager;
    private bool _didWin; 
    public GameOverState(MultiplayerGameManagerStateMachine stateMachine, MultiplayerGameManager multiplayerGameManager, bool didWin)
    {
        _stateMachine = stateMachine;
        _gameManager = multiplayerGameManager;
        _gameManager.SetConnectionListener(this);
        _didWin = didWin;
    }

    public override void Exit()
    {
    }

    public override void Enter()
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

    public override void Reconnected()
    {
        throw new NotImplementedException();
    }

    public override void ReconnectFailed()
    {
        throw new NotImplementedException();
    }

    public override void Disconnected()
    {
        throw new NotImplementedException();
    }

    public override void ProcessLocalUpdate(UIEvent @event)
    {
        throw new NotImplementedException();
    }
}


public abstract class MultiplayerGameManagerState:IP2PConnectionListener
{
    public abstract void Exit();
    public abstract void Enter();
    public virtual void Connected()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement Connected");
    }

    public virtual void UnableToConnect()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement UnableToConnect");
    }

    public abstract void Reconnected();

    public abstract void ReconnectFailed();

    public abstract void Disconnected();

    public virtual void Receive(Dictionary message)
    {
        throw new NotImplementedException($"{GetType().Name} does not implement Receive");
    }

    public abstract void ProcessLocalUpdate(UIEvent @event);
}
