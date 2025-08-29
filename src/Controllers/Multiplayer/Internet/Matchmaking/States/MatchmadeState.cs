using BattleshipWithWords.Services.ConnectionManager;
using BattleshipWithWords.Services.ConnectionManager.Server;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;

public class MatchmadeState: InternetMatchmakingState, IServerConnectionListener
{
    private InternetMatchmakingController _controller;
    private RoomFull _response;

    public MatchmadeState(InternetMatchmakingController controller, RoomFull response)
    {
        _controller = controller;
        _response = response;
    }

    public override void Enter()
    {
        _controller.Node.SetInfo("Connected with opponent.");
        _controller.Node.ShowPlayButton();
    }

    public override void Exit()
    {
    }

    public override void OnPlayButtonPressed()
    {
        _controller.Send(new ConfirmMatch
        {
            UserId = _controller.Node.Auth.UserId,
            RoomId = _response.RoomId
        });
        _controller.Node.SetInfo("Waiting for opponent to confirm.");
    }

    public override void OnCancelButtonPressed()
    {
        //TODO this should not send an ExitMatchmaking but an ExitGame message
        _controller.Send(new ExitMatchmaking
        {
            UserId = _controller.Node.Auth.UserId,
            UserSkill = 100
        });
        _controller.CloseConnection();
    }

    public override void Connecting()
    {
        throw new System.NotImplementedException();
    }

    public override void Connected()
    {
        GD.Print("connected while in MatchmadeState");
    }

    public override void UnableToConnect()
    {
        throw new System.NotImplementedException();
    }

    public override void Disconnected()
    {
        GD.Print("disconnected while in MatchmadeState");
    }

    public override void Reconnecting()
    {
        GD.Print("reconnecting while in MatchmadeState");
    }

    public override void Reconnected()
    {
        GD.Print("reconnected while in MatchmadeState");
    }

    public override void Receive(IServerReceivable message)
    {
        switch (message)
        {
            case PlayerLeftRoom msg:
                GD.Print("received player left room");
                //TODO if opponent leaves return back to matchmaking.
                _controller.TransitionTo(new MatchmakingState(_controller));
                break;
            case StartSetup msg:
                _controller.Node.OnStartSetup.Invoke(msg);
                break;
        }
    }

    public override void Disconnecting()
    {
        GD.Print("disconnecting while in MatchmadeState");
    }

    public override void HttpResponse(string response)
    {
        throw new System.NotImplementedException();
    }
}