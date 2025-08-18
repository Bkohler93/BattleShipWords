
using BattleshipWithWords.Services.ConnectionManager.Server;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;


public class InternetMatchmakingController : ControllerWithServerConnection
{
    public InternetMatchmaking Node;
    private InternetMatchmakingState _currentState;

    public InternetMatchmakingController(InternetMatchmaking node, ServerConnectionManager connectionManager)
    {
        Node = node; 
        ConnectionManager = connectionManager;
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
        _currentState.Enter();
    }

    public void OnCancelButtonPressed()
    {
        _currentState.OnCancelButtonPressed();
    }
}