using System;
using System.Collections.Generic;
using androidplugintest.ConnectionManager;
using BattleshipWithWords.Services.GameManager;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.Game;

public class MultiplayerGameManager
{
    public RemoteGameStateManager PlayerTwoGameStateManager;
    private P2PConnectionManager _connectionManager;
    // private Queue<MultiplayerMessage> _remoteMessageQueue = new();
    private MultiplayerGameManagerStateMachine _stateMachine;

    public void SetConnectionListener(IP2PConnectionListener listener)
    {
        _connectionManager.SetListener(listener);
    }

    public MultiplayerGameManager(P2PConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
        _stateMachine = new MultiplayerGameManagerStateMachine(this);
    }

    public Action<SetupSceneUpdate> SetupUpdated;
    public event Action<UIUpdate> OpponentUIUpdated;
    public event Action<UIUpdate> LocalUIUpdated;
    public event Action BothSetupsCompleted;
    public event Action<WordGuessedResponseData> GuessResultReceived;
    public event Action<UncoveredTileResponse> TileUncoverResultReceived;
    public event Action GameWon;
    public event Action GameLost;
    public event MultiplayerApi.PeerDisconnectedEventHandler PeerDisconnected;

    //TODO disconnect events somewhere....
    // public override void _ExitTree()
    // {
    //     SetupComplete = null; 
    //     GuessResultReceived = null;
    //     TileUncoverResultReceived = null;
    //     OpponentGuessed = null;
    //     OpponentUncoveredTile = null;
    //     OpponentBackspacePressed = null;
    //     OpponentKeyPressed = null;
    //     GameWon = null;
    //     GameLost = null;
    // }

    public bool  PlayerTwoReady { get; private set; }

    private void _sendSetupReady()
    {
        var msg = new MultiplayerMessage(MultiplayerMessageType.Setup, null);
        _connectionManager.Send(msg);
        // RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
    }
    
    // [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    // public void Rpc_Send(Dictionary data)
    // {
    //     var msg = MultiplayerMessage.From(data);
    //     _remoteMessageQueue.Enqueue(msg);
    // }

    //TODO process incoming messages
    // public override void _Process(double delta)
    // {
    //     while (_remoteMessageQueue.TryPeek(out var msg))
    //     {
    //         if (_stateMachine.TryProcessRemote(msg)) _remoteMessageQueue.Dequeue();
    //         else
    //             break;
    //     }
    // }

    public void StartGame()
    {
        BothSetupsCompleted?.Invoke();
    }

    public void DisconnectAndFree()
    {
        //TODO maybe change this
        _connectionManager.Disable();
        // Multiplayer.MultiplayerPeer.Close();
        // Multiplayer.MultiplayerPeer = null;
        // QueueFree();
    }

    // public void GuessMadeEventHandler(string guessedWord)
    // {
    //     if (_stateMachine.CurrentState is not PlayingState) return;
    //     
    //     var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
    //     {
    //         Type = PlayingDataType.Event,
    //         Data = new PlayingEventData()
    //         {
    //             Type = EventType.GuessedWord,
    //             Data = new GuessedWordData()
    //             {
    //                 Word = guessedWord,
    //             },
    //         }
    //     });
    //     _connectionManager.Send(msg);
    //     // RpcId(_peerId, nameof(Rpc_Send),msg.Serialize());
    // }

    // public void TileUncoveredEventHandler((int row, int col) coord)
    // {
    //     if (_stateMachine.CurrentState is not PlayingState) return;
    //
    //     var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
    //     {
    //         Type = PlayingDataType.Event,
    //         Data = new PlayingEventData()
    //         {
    //             Type = EventType.UncoveredTile,
    //             Data = new UncoveredTileData()
    //             {
    //                 Row = coord.row,
    //                 Col = coord.col,
    //             }
    //         }
    //     });
    //     _connectionManager.Send(msg); 
    //     // RpcId(_peerId, nameof(Rpc_Send),msg.Serialize());
    // }

    public void LocalBackspacePressedEventHandler()
    {
        var playingData = new PlayingData()
        {
            Type = PlayingDataType.Event,
            Data = new PlayingEventData()
            {
                Type = EventType.BackspacePressed
            }
        };
        var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, playingData);
        _connectionManager.Send(msg);
        // RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
    }

    public void LocalKeyPressedEventHandler(string key)
    {
        var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
        {
            Type = PlayingDataType.Event,
            Data = new PlayingEventData()
            {
                Type = EventType.KeyPressed,
                Data = new EventKeyPressedData()
                {
                  Key = key,  
                },
            },
        });
        _connectionManager.Send(msg);
        // RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
    }

    public void LocalUpdateHandler(UIEvent @event)
    {
        _stateMachine.ProcessLocalUpdate(@event);
    }

    public void UpdateLocalUI(UIUpdate uiEvent)
    {
        LocalUIUpdated?.Invoke(uiEvent);
    }
    
    public void UpdateOpponentUI(UIUpdate uiEvent)
    {
        OpponentUIUpdated?.Invoke(uiEvent);
    }

    public void ProcessOpponentWordGuess(string word)
    {
        var res = PlayerTwoGameStateManager.ProcessWordGuess(word);
        switch (res.Result)
        {
            case TurnResult.GoAgain:
                break;
            case TurnResult.Win:
                _stateMachine.TransitionTo(new GameOverState(_stateMachine, this, false));
                break;
            case TurnResult.TurnOver:
                _stateMachine.TransitionTo(new PlayingState(_stateMachine, this));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        // if (!res.DoesPlayerGoAgain)
        //     _stateMachine.TransitionTo(new PlayingState(_stateMachine, this));

        var data = new WordGuessedResponseData()
        {
            Result = res.Result,
            ResponseLetters = res.ResponseLetters,
            WordLetterStatus = res.WordLetterStatus,
        };
        
        var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
        {
            Type = PlayingDataType.Response,
            Data = new PlayingResponseData()
            {
                Type = ResponseType.WordGuessed,
                Data = data,
            }
        });
        
        OpponentUIUpdated?.Invoke(new UIUpdate
        {
            Type = UIUpdateType.GuessedWord,
            Data = data, 
        });
        // OpponentGuessed?.Invoke(data);
        _connectionManager.Send(msg);
        // RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
    }

    public void UpdateLocalGameFromWordGuess(WordGuessedResponseData data)
    {
        switch (data.Result)
        {
            case TurnResult.GoAgain:
                break;
            case TurnResult.Win:
                _stateMachine.TransitionTo(new GameOverState(_stateMachine, this, true));
                break;
            case TurnResult.TurnOver:
                _stateMachine.TransitionTo(new OpponentPlayingState(this, _stateMachine));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        // if (!data.DoesPlayerGoAgain) 
        //    _stateMachine.TransitionTo(new OpponentPlayingState(this, _stateMachine));
        ;
        UpdateLocalUI(new UIUpdate
        {
            Type = UIUpdateType.GuessedWord,
            Data =new WordGuessedResponseData() 
            {
                Result = TurnResult.GoAgain,
                ResponseLetters = null,
                WordLetterStatus = null
            }
        });
        GuessResultReceived?.Invoke(data);
    }

    public void ProcessOpponentUncoverTile(int coordsRow, int coordsCol)
    {
        
        // RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
    }

    public void UpdateLocalFromUncoverTile(UncoveredTileResponse uncoverTileData)
    {
        switch (uncoverTileData.Result)
        {
            case TurnResult.GoAgain:
                break;
            case TurnResult.Win:
                _stateMachine.TransitionTo(new GameOverState(_stateMachine, this, true));
                break;
            case TurnResult.TurnOver:
                _stateMachine.TransitionTo(new OpponentPlayingState(this, _stateMachine));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        TileUncoverResultReceived?.Invoke(uncoverTileData);
    }

    public void OnWin()
    {
        GameWon?.Invoke();
    }

    public void OnLose()
    {
        GameLost?.Invoke();
    }

    public bool IsHost()
    {
        return _connectionManager.IsHost();
    }


    public void SendPlayingResponse(ResponseType type, IResponseData data)
    {
        var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
        {
            Type = PlayingDataType.Response,
            Data = new PlayingResponseData()
            {
                Type = type,
                Data = data,
            }
        });
        _connectionManager.Send(msg);
    }

    public void SendSetupComplete()
    {
        var msg = new MultiplayerMessage(MultiplayerMessageType.Setup, null);
        _connectionManager.Send(msg);
    }

    public void SendPlayingEvent(EventType type, IEventData eventData)
    {
        var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
        {
            Type = PlayingDataType.Event,
            Data = new PlayingEventData()
            {
                Type = type,
                Data = eventData, 
            }
        });
        _connectionManager.Send(msg);
    }
}

public enum SetupSceneUpdate
{
    WaitingForOtherPlayer,
    SetupComplete
}

