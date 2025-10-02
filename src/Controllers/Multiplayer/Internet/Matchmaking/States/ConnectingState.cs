using System;
using BattleshipWithWords.Nodes.Globals;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;

public class ConnectingState : InternetMatchmakingState, IServerConnectionListener
{
    private readonly InternetMatchmakingController _controller;
    private string _jwtString;

    public ConnectingState(InternetMatchmakingController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        var res = _controller.GetRequest(ServerConfig.GetServer("AuthServer")+"/guest");
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

    public override void Connecting()
    {
        _controller.Node.SetInfo("Connecting to the Internet matchmaking server");
    }

    public override void Connected()
    {
        GD.Print("connected");
        _controller.Send(new ConnectingMessage
        {
            JwtString = _jwtString 
        });
    }

    public override void UnableToConnect()
    {
        _controller.Node.SetInfo("Unable to connect to the Internet matchmaking server");
    }

    public override void Disconnected()
    {
        _controller.Node.SetInfo("Disconnected from the Internet matchmaking server");
    }

    public override void Reconnecting()
    {
        throw new NotImplementedException();
    }

    public override void Reconnected()
    {
        throw new NotImplementedException();
    }

    public override void Receive(IServerReceivable message)
    {
        if (message is AuthenticatedMessage msg)
        {
            _controller.Node.Auth.SetUser(msg.UserId, _jwtString); 
            _controller.TransitionTo(new MatchmakingState(_controller));
        }
        else
        {
            GD.Print("Received an unexepcted, non-Authenticated Message message");
        }
    }

    public override void Disconnecting()
    {
        throw new NotImplementedException();
    }

    public override void HttpResponse(string response)
    {
        _jwtString = response;
        Logger.Print($"got response {_jwtString}");
        Result res;
        res = _controller.Connect(ServerConfig.GetServer("WebsocketServer") +"/ws", ServerConfig.Environment == "Production" ? TlsOptions.Client() : TlsOptions.ClientUnsafe());
        if (res.Success) return;
        GD.Print("Failed to connect to the Internet matchmaking server");
        _controller.Node.SetInfo("Could not connect to server");
    }
}