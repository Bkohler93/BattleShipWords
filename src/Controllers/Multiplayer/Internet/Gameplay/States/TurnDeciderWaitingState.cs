using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay.States;

public class TurnDeciderWaitingState: GameplayState
{
    private InternetGameplayController _controller;

    public TurnDeciderWaitingState(InternetGameplayController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        Logger.Print("Entered waiting state for TurnDecider. Set ServerConnectionManager Listnener.");
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
                            case TurnDeciderResult.Lost:
                                _controller.TransitionTo(new OpponentsTurnState(_controller));
                                break;
                            case TurnDeciderResult.Won:
                                _controller.TransitionTo(new PlayersTurnState(_controller));
                                break;
                            case TurnDeciderResult.Tie:
                                _controller.TransitionTo(new TurnDeciderGuessingState(_controller));
                                break;
                            default:
                                Logger.Print("received unexpected result");
                                // remain in waiting state
                                break;
                        }    
                    }

                    if (msg.UIUpdate is TurnTakenUpdate turnTakenUpdate)
                    {
                        if (msg.UserId == _controller.Node.Auth.UserId)
                        {
                            _controller.LocalUIUpdated(msg.UIUpdate);
                        }
                        else
                        {
                            _controller.OpponentUIUpdated(msg.UIUpdate);
                        }
                    }
                }

                break;
            default:
                throw new System.Exception($"Unknown message type - {message}");
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
        switch (@event)
        {
            case GuessedWordEvent:
            case UncoveredTileEvent:
                return false;
            case KeyPressedEvent kp:
            {
                _controller.LocalUIUpdated(new KeyPressedUpdate
                {
                    Key = kp.Key, 
                });
                _controller.Node.ConnectionManager.Send(new TurnDeciderUIEventMessage
                {
                    UserId = _controller.Node.Auth.UserId,
                    IuiEvent = @event
                });
                return true;
                break;
            }
        }

        return false;
    }
}