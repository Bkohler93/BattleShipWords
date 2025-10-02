using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay.States;

public class LoseState: GameplayState
{
    private InternetGameplayController _controller;

    public LoseState(InternetGameplayController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.Node.GameLost?.Invoke();
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
    }

    public override void Reconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void Receive(IServerReceivable message)
    {
        switch (message)
        {
            case (PlayerQuit): 
                Logger.Print("Other player quit");
                break;
            //TODO: case (EndGameUIUpdatemessage msg)
            case (GameplayUIUpdateMessage msg):
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
            }
        }

        return false;
    }
}