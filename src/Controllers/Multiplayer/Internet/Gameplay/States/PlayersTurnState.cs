using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Nodes.Game;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay.States;

public class PlayersTurnState: GameplayState
{
    private InternetGameplayController _controller;

    public PlayersTurnState(InternetGameplayController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.Node.SetPlayerTurn(true);
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
            case (GameplayUIUpdateMessage msg):
                if (msg.UIUpdate != null)
                {
                        switch (msg.TurnResult)
                        {
                            case TurnResult.Win:
                                _controller.TransitionTo(new WinState(_controller));
                                return;
                            case TurnResult.Loss:
                                _controller.TransitionTo(new LoseState(_controller));
                                return;
                            case TurnResult.EndTurn:
                                _controller.TransitionTo(new OpponentsTurnState(_controller));
                                break;
                            case TurnResult.GoAgain:
                                break;
                        }

                        if (msg.UIUpdate is TimeUpdate timeUpdateMsg)
                        {
                            _controller.ReceivedServerTick?.Invoke(timeUpdateMsg.TimeLeft); 
                        }
                        else
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
        void SendEvent()
        {
            _controller.Node.ConnectionManager.Send(new GameplayUIEventMessage
            {
                UserId = _controller.Node.Auth.UserId,
                IuiEvent = @event
            });
        }
        switch (@event)
        {
            case GuessedWordEvent:
            case UncoveredTileEvent:
                SendEvent();
                return true;
            case KeyPressedEvent kp:
            {
                _controller.LocalUIUpdated(new KeyPressedUpdate
                {
                    Key = kp.Key, 
                });
                SendEvent();
                return true;
            }
        }

        return false;
    }
}