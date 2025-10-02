using System;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay.States;

public class TurnDeciderGuessingState: GameplayState
{
    private InternetGameplayController _controller;

    public TurnDeciderGuessingState(InternetGameplayController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        Logger.Print("Entered guessing state for TurnDecider");
    }

    public override void Exit()
    {
    }

    

    public override void Connecting()
    {
        throw new System.NotImplementedException();
    }

    public override void Connected()
    {
        throw new System.NotImplementedException();
    }

    public override void UnableToConnect()
    {
        throw new System.NotImplementedException();
    }

    public override void Disconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void Reconnecting()
    {
        throw new System.NotImplementedException();
    }

    public override void Reconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void Receive(IServerReceivable message)
    {
        switch (message)
        {
            case (TurnDeciderUIUpdateMessage msg):
                if (msg.UIUpdate != null)
                {
                    if (msg.TurnDeciderResults != null)
                    {
                        Logger.Print($"Received result {msg.TurnDeciderResults[_controller.Node.Auth.UserId]}");
                        switch (msg.TurnDeciderResults[_controller.Node.Auth.UserId])
                        {
                            case TurnDeciderResult.Won:
                                _controller.TransitionTo(new PlayersTurnState(_controller));
                                break;
                            case TurnDeciderResult.Lost:                            
                                _controller.TransitionTo(new OpponentsTurnState(_controller));
                                break;
                            case TurnDeciderResult.Tie:
                                //remain in guessing state.
                                break;
                            case TurnDeciderResult.WaitingForOtherPlayer:
                                _controller.TransitionTo(new TurnDeciderWaitingState(_controller));
                                break;
                            case TurnDeciderResult.OtherPlayerWent:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }   
                    }
                    
                    if (msg.UserId == _controller.Node.Auth.UserId)
                    {
                        _controller.LocalUIUpdated(msg.UIUpdate);
                    }
                    else
                    {
                        _controller.OpponentUIUpdated(msg.UIUpdate);
                    }    
                }

                break;
            default:
                throw new Exception($"Unknown message type - {message}");
        }
    }

    public override void Disconnecting()
    {
        throw new System.NotImplementedException();
    }

    public override void HttpResponse(string response)
    {
        throw new System.NotImplementedException();
    }

    public override bool HandleLocalUpdate(IUIEvent @event)
    {
        void SendEvent()
        {
            _controller.Node.ConnectionManager.Send(new TurnDeciderUIEventMessage { UserId = _controller.Node.Auth.UserId, IuiEvent = @event, });
        }

        switch (@event)
        {
            case KeyPressedEvent kp:
                _controller.LocalUIUpdated(new KeyPressedUpdate
                {
                    Key = kp.Key, 
                });
                SendEvent();
                return true;
                break;
            case GuessedWordEvent:
                SendEvent();
                return true;
                _controller.TransitionTo(new TurnDeciderWaitingState(_controller));
                break;
            case UncoveredTileEvent:
                return false;
        }

        return false;
    }
}