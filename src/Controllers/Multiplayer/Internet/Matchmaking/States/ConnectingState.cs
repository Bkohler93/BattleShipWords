using System;
using BattleshipWithWords.Services.ConnectionManager;
using BattleshipWithWords.Services.ConnectionManager.Server;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;

public class ConnectingState : InternetMatchmakingState, IServerConnectionListener
{
    private InternetMatchmakingController _controller;

    public ConnectingState(InternetMatchmakingController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.SetConnectionListener(this);
        var res = _controller.Connect("ws://192.168.0.13:8083/ws", TlsOptions.ClientUnsafe());
        if (res.Success) return;
        GD.Print("Failed to connect to the Internet matchmaking server");
        _controller.Node.SetInfo("Could not connect to server");
    }

    public override void Exit()
    {
    }

    public override void OnCancelButtonPressed()
    {
        _controller.CloseConnection();
    }

    public void Connecting()
    {
        _controller.Node.SetInfo("Connecting to the Internet matchmaking server");
    }

    public void Connected()
    {
        _controller.TransitionTo(new MatchmakingState(_controller));
    }

    public void UnableToConnect()
    {
        _controller.Node.SetInfo("Unable to connect to the Internet matchmaking server");
    }

    public void Disconnected()
    {
        _controller.Node.SetInfo("Disconnected from the Internet matchmaking server");
    }

    public void Reconnecting()
    {
        throw new NotImplementedException();
    }

    public void Reconnected()
    {
        throw new NotImplementedException();
    }

    public void Receive(BaseServerReceiveMessage message)
    {
        throw new NotImplementedException();
    }

    public void Disconnecting()
    {
        throw new NotImplementedException();
    }
}