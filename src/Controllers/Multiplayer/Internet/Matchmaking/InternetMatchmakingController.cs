
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;


public class InternetMatchmakingController 
{
    public InternetMatchmaking Node;
    private InternetMatchmakingState _currentState;
    private ServerConnectionManager _connectionManager;

    public InternetMatchmakingController(InternetMatchmaking node, ServerConnectionManager connectionManager)
    {
        Node = node; 
        _connectionManager = connectionManager;
    }

    public void OnPlayButtonPressed()
    {
        _currentState.OnPlayButtonPressed();
    }

    public void Ready()
    {
        TransitionTo(new ConnectingState(this));
    }

    public void TransitionTo(InternetMatchmakingState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _connectionManager.Listener = _currentState;
        _currentState.Enter();
    }

    public void OnCancelButtonPressed()
    {
        _currentState.OnCancelButtonPressed();
    }

    public Result Send(IServerSendable msg)
    {
        var err =_connectionManager.Send(msg);
        return err != Error.Ok ? Result.Fail(err.ToString()) : Result.Ok();
    }

    public void CloseConnection()
    {
        _connectionManager.Close();
    }

    public Result GetRequest(string url)
    {
        var err = _connectionManager.HttpGet(url);
        return err != Error.Ok ? Result.Fail(err.ToString()) : Result.Ok();
    }

    public Result Connect(string url, TlsOptions options)
    {
        var err = _connectionManager.Connect(url, options);
        return err != Error.Ok ? Result.Fail(err.ToString()) : Result.Ok();
    }
}