using System;
using BattleshipWithWords.Services.ConnectionManager;
using BattleshipWithWords.Services.ConnectionManager.Server;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;

public class MatchmakingState : InternetMatchmakingState, IServerConnectionListener
{
    private InternetMatchmakingController _controller;

    public MatchmakingState(InternetMatchmakingController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.SetConnectionListener(this);
        _controller.Node.SetInfo("Connected to matchmaking server.");
        var num = new Random().Next(0, 100);
        var req = MatchmakingRequest.New($"DooDooFucker{num}");
        
        var res = _controller.Send(req);
        if (res.Success)
            _controller.Node.SetInfo("Waiting to find a match");
        else
        {
            GD.Print("Failed to send matchmaking request to matchmaking server");
            _controller.Node.SetInfo("There was an error communicating with the server.");
        }
    }

    public override void Exit()
    {
    }

    public override void OnCancelButtonPressed()
    {
        _controller.CloseConnection();
    }

    public override void OnPlayButtonPressed()
    {
        throw new System.NotImplementedException();
    }

    public void Connecting()
    {
        GD.Print("connecting to matchmaking server");
    }

    public void Connected()
    {
        GD.Print("connected while in MatchmakingState");
    }

    public void UnableToConnect()
    {
        GD.Print("Unable to send matchmaking request to matchmaking server");
    }

    public void Disconnected()
    {
        GD.Print("disconnected while in MatchmakingState");
    }

    public void Reconnecting()
    {
        GD.Print("reconnecting to matchmaking server");
    }

    public void Reconnected()
    {
        GD.Print("reconnected while in MatchmakingState");
    }

    public void Receive(BaseServerReceiveMessage message)
    {
        if (message.Payload is MatchmakingResponse res)
        {
            _controller.TransitionTo(new MatchmadeState(_controller, res));
        }
    }

    public void Disconnecting()
    {
        GD.Print("disconnecting while in MatchmakingState");
    }
}

