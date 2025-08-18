using BattleshipWithWords.Services.ConnectionManager;
using BattleshipWithWords.Services.ConnectionManager.Server;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;

public class MatchmadeState: InternetMatchmakingState, IServerConnectionListener
{
    private InternetMatchmakingController _controller;
    private MatchmakingResponse _response;

    public MatchmadeState(InternetMatchmakingController controller, MatchmakingResponse response)
    {
        _controller = controller;
        _response = response;
    }

    public override void Enter()
    {
        _controller.SetConnectionListener(this);
        _controller.Node.SetInfo("Connected with opponent.");
        _controller.Node.ShowPlayButton();
    }

    public override void Exit()
    {
    }

    public override void OnPlayButtonPressed()
    {
        GD.Print("time to exit Matchmaking Scene and move into Setup Scene");
    }

    public override void OnCancelButtonPressed()
    {
        _controller.CloseConnection();
    }

    public void Connecting()
    {
        throw new System.NotImplementedException();
    }

    public void Connected()
    {
        GD.Print("connected while in MatchmadeState");
    }

    public void UnableToConnect()
    {
        throw new System.NotImplementedException();
    }

    public void Disconnected()
    {
        GD.Print("disconnected while in MatchmadeState");
    }

    public void Reconnecting()
    {
        GD.Print("reconnecting while in MatchmadeState");
    }

    public void Reconnected()
    {
        GD.Print("reconnected while in MatchmadeState");
    }

    public void Receive(BaseServerReceiveMessage message)
    {
        GD.Print($"received message while in MatchmadeState - {message}");
    }

    public void Disconnecting()
    {
        GD.Print("disconnecting while in MatchmadeState");
    }
}