using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.TurnDecider.States;

public class GuessingState: TurnDeciderState
{
    private TurnDeciderController _controller;

    public GuessingState(TurnDeciderController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        throw new System.NotImplementedException();
    }

    public override void Exit()
    {
        throw new System.NotImplementedException();
    }

    public override void HandleGuessTile(Coordinate tileCoordinate)
    {
        throw new System.NotImplementedException();
    }

    public override void HandleGuessWord(string word)
    {
        throw new System.NotImplementedException();
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
            case (UIUpdateMessage msg):
                if (msg.UserId == _controller.Node.Auth.UserId)
                {
                    _controller.LocalUIUpdated(msg.UIUpdate);
                }
                else
                {
                    _controller.OpponentUIUpdated(msg.UIUpdate);
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

    public override void HandleLocalUpdate(UIEvent @event)
    {
        if (@event.Type == EventType.KeyPressed)
        {
            var data = (@event.Data as EventKeyPressedData);
            _controller.LocalUIUpdated(new UIUpdate
            {
                Type = UIUpdateType.KeyPressed,
                Data = new KeyData(data.Key), 
            });
        }
        _controller.Node.ConnectionManager.Send(new UIEventMessage
        {
            UserId = _controller.Node.Auth.UserId,
            UIEvent = @event,
        });
    }
}