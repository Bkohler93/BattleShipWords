using System;
using BattleshipWithWords.Nodes.Globals;
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
        _controller.Node.HidePlayButton();
        _controller.Node.SetInfo("Connected to matchmaking server.");
        var timeCreated = DateTimeOffset.Now.ToUnixTimeSeconds();
        var req = new RequestMatchmaking
        {
            UserId = _controller.Node.Auth.UserId,
            Name = _controller.Node.Auth.Name,
            TimeCreated = timeCreated,
            Skill = 100,
            Region = "na" 
        };
        
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

    public override void Connecting()
    {
        GD.Print("connecting to matchmaking server");
    }

    public override void Connected()
    {
        GD.Print("connected while in MatchmakingState");
    }

    public override void UnableToConnect()
    {
        GD.Print("Unable to send matchmaking request to matchmaking server");
    }

    public override void Disconnected()
    {
        GD.Print("disconnected while in MatchmakingState");
    }

    public override void Reconnecting()
    {
        GD.Print("reconnecting to matchmaking server");
    }

    public override void Reconnected()
    {
        GD.Print("reconnected while in MatchmakingState");
    }

    public override void Receive(IServerReceivable message)
    {
        switch (message)
        {
            case PlayerLeftRoom msg:
                GD.Print($"Player {msg.UserLeftId} left the room");
                _controller.Node.SetInfo("Player left the room. Continuing to search...");
                break;
            case PlayerJoinedRoom msg:
                GD.Print($"Player {msg.UserJoinedId} joined the room.");
                _controller.Node.SetInfo("Player joined the room.");
                break;
            case RoomChanged msg:
                GD.Print($"Changed room to {msg.NewRoomId} with {msg.PlayerCount} players.");
                _controller.Node.SetInfo($"Joined room with {msg.PlayerCount} players.");
                break;
            case RoomFull msg:
                GD.Print($"Full room[{msg.RoomId}] with {msg.PlayerCount} players.");
                _controller.TransitionTo(new MatchmadeState(_controller, msg));
                break;
        }
    }

    public override void Disconnecting()
    {
        GD.Print("disconnecting while in MatchmakingState");
    }

    public override void HttpResponse(string response)
    {
        throw new NotImplementedException();
    }
}

