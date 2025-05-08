using System;
using System.Collections.Generic;
using BattleshipWithWords.Services.GameManager;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.Game;

public partial class MultiplayerGameManager : Node
{
    private long _peerId;
    private long _id;
    private bool _waitingOnOtherPlayer;

    public RemoteGameStateManager PlayerTwoGameStateManager;
    private Queue<MultiplayerMessage> _remoteMessageQueue = new();
    private MultiplayerGameManagerStateMachine _stateMachine;
    public event Action SetupComplete;
    public event Action<WordGuessedResponseData> GuessResultReceived;
    public event Action<UncoveredTileResponse> TileUncoverResultReceived;
    public event Action<WordGuessedResponseData> OpponentGuessed;
    public event Action<UncoveredTileResponse> OpponentUncoveredTile;
    public event Action OpponentBackspacePressed;
    public event Action<string> OpponentKeyPressed;
    public event Action GameWon;
    public event Action GameLost;
    public event MultiplayerApi.PeerDisconnectedEventHandler PeerDisconnected;

    public void Init()
    {
        _id = Multiplayer.MultiplayerPeer.GetUniqueId();
        _peerId = Multiplayer.GetPeers()[0];
        Multiplayer.PeerDisconnected += (disconnectedPeerId) => PeerDisconnected!(disconnectedPeerId);
        _stateMachine = new MultiplayerGameManagerStateMachine(new SettingUp(this), this);
    }

    public override void _ExitTree()
    {
        SetupComplete = null; 
        GuessResultReceived = null;
        TileUncoverResultReceived = null;
        OpponentGuessed = null;
        OpponentUncoveredTile = null;
        OpponentBackspacePressed = null;
        OpponentKeyPressed = null;
        GameWon = null;
        GameLost = null;
    }

    public bool  PlayerTwoReady { get; private set; }
    public long LocalId => _id; 

    private void _sendSetupReady()
    {
        var msg = new MultiplayerMessage(MultiplayerMessageType.Setup, null);
        RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Rpc_Send(Dictionary data)
    {
        var msg = MultiplayerMessage.From(data);
        _remoteMessageQueue.Enqueue(msg);
    }

    public void CompleteLocalSetup(List<string> words, List<List<(int row, int col)>> boardSelection)
    {
        PlayerTwoGameStateManager = RemoteGameStateManager.FromSetup(words, boardSelection);
        _stateMachine.TransitionTo(new SetupComplete(this, _stateMachine));
        _sendSetupReady();
    }

    public override void _Process(double delta)
    {
        while (_remoteMessageQueue.TryPeek(out var msg))
        {
            if (_stateMachine.TryProcessRemote(msg)) _remoteMessageQueue.Dequeue();
            else
                break;
        }
    }

    public void SetWaiting()
    {
        _waitingOnOtherPlayer = true;
    }

    public void StartGame()
    {
        SetupComplete?.Invoke();
    }

    public void DisconnectAndFree()
    {
        Multiplayer.MultiplayerPeer.Close();
        Multiplayer.MultiplayerPeer = null;
        QueueFree();
    }

    public void GuessMadeEventHandler(string guessedWord)
    {
        if (_stateMachine.CurrentState is not PlayingState) return;
        
        var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
        {
            Type = PlayingDataType.Event,
            Data = new PlayingEventData()
            {
                Type = EventType.GuessedWord,
                Data = new GuessedWordData()
                {
                    Word = guessedWord,
                },
            }
        });
        RpcId(_peerId, nameof(Rpc_Send),msg.Serialize());
    }

    public void TileUncoveredEventHandler((int row, int col) coord)
    {
        if (_stateMachine.CurrentState is not PlayingState) return;

        var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
        {
            Type = PlayingDataType.Event,
            Data = new PlayingEventData()
            {
                Type = EventType.UncoveredTile,
                Data = new UncoveredTileData()
                {
                    Row = coord.row,
                    Col = coord.col,
                }
            }
        });
        
        RpcId(_peerId, nameof(Rpc_Send),msg.Serialize());
    }

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
        RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
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
        RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
    }

    public void OpponentPressBackspace()
    {
        OpponentBackspacePressed?.Invoke();
    }

    public void OpponentPressKey(string key)
    {
        OpponentKeyPressed?.Invoke(key); 
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
        OpponentGuessed?.Invoke(data);
        RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
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
        GuessResultReceived?.Invoke(data);
    }

    public void ProcessOpponentUncoverTile(int coordsRow, int coordsCol)
    {
        var res = PlayerTwoGameStateManager.ProcessTileUncovered(coordsRow, coordsCol);
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

        var msg = new MultiplayerMessage(MultiplayerMessageType.Playing, new PlayingData()
        {
            Type = PlayingDataType.Response,
            Data = new PlayingResponseData()
            {
                Type = ResponseType.TileUncovered,
                Data = res,
            }
        });
        OpponentUncoveredTile?.Invoke(res); 
        RpcId(_peerId, nameof(Rpc_Send), msg.Serialize());
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
}

