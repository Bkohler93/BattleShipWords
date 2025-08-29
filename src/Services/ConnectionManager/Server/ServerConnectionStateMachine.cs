using System;
using System.Net.WebSockets;
using BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;
using Godot;

namespace BattleshipWithWords.Services.ConnectionManager.Server;

public abstract class ServerConnectionState
{
    protected ServerConnectionStateMachine StateMachine;
    protected ServerConnectionManager Manager;

    protected ServerConnectionState(ServerConnectionStateMachine stateMachine, ServerConnectionManager manager)
    {
        StateMachine = stateMachine;
        Manager = manager;
    }

    public void ThrowStateException(WebSocketPeer.State state)
    {
        throw new Exception($"{GetType().Name} should not handle newState={state}");
    }
    public abstract void Enter();
    public abstract void Exit();
    public abstract void HandleNewWebsocketState(WebSocketPeer.State newState);
}

public class IdleState(ServerConnectionStateMachine stateMachine, ServerConnectionManager manager)
    : ServerConnectionState(stateMachine, manager)
{
    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void HandleNewWebsocketState(WebSocketPeer.State newState)
    {
        switch (newState)
        {
            case WebSocketPeer.State.Connecting:
                StateMachine.TransitionTo(new ConnectingState(StateMachine, Manager));
                break;
            case WebSocketPeer.State.Open:
                StateMachine.TransitionTo(new ConnectedState(StateMachine, Manager));
                break;
            case WebSocketPeer.State.Closing:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Closed:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        } 
    }
}

public class ConnectingState(ServerConnectionStateMachine stateMachine, ServerConnectionManager manager)
    : ServerConnectionState(stateMachine, manager)
{
    public override void Enter()
    {
        Manager.Listener.Connecting();
    }

    public override void Exit()
    {
    }

    public override void HandleNewWebsocketState(WebSocketPeer.State newState)
    {
        switch (newState)
        {
            case WebSocketPeer.State.Connecting:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Open:
                StateMachine.TransitionTo(new ConnectedState(StateMachine, Manager));
                break;
            case WebSocketPeer.State.Closing:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Closed:
                Manager.Listener.UnableToConnect(); 
                StateMachine.TransitionTo(new DisconnectedState(StateMachine, Manager));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}

public class ConnectedState(ServerConnectionStateMachine stateMachine, ServerConnectionManager manager)
    : ServerConnectionState(stateMachine, manager)
{
    public override void Enter()
    {
        Manager.Listener.Connected();
    }

    public override void Exit()
    {
    }

    public override void HandleNewWebsocketState(WebSocketPeer.State newState)
    {
        switch (newState)
        {
            case WebSocketPeer.State.Connecting:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Open:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Closing:
                StateMachine.TransitionTo(new DisconnectingState(StateMachine, manager));
                break;
            case WebSocketPeer.State.Closed:
                StateMachine.TransitionTo(new ReconnectingState(StateMachine, Manager));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}

public class ReconnectingState(ServerConnectionStateMachine stateMachine, ServerConnectionManager manager)
    : ServerConnectionState(stateMachine, manager)
{
    public override void Enter()
    {
        Manager.Listener.Reconnecting();
        Manager.StartReconnecting();
    }

    public override void Exit()
    {
    }

    public override void HandleNewWebsocketState(WebSocketPeer.State newState)
    {
        switch (newState)
        {
            case WebSocketPeer.State.Connecting:
                GD.Print($"ReconnectingState:{newState} -- reconnecting...");
                break;
            case WebSocketPeer.State.Open:
                GD.Print($"ReconnectingState:{newState} -- connected...");
                Manager.Listener.Reconnected();
                StateMachine.TransitionTo(new ConnectedState(StateMachine, manager));
                break;
            case WebSocketPeer.State.Closing:
                GD.Print($"ReconnectingState:{newState} -- closing the connection...");
                break;
            case WebSocketPeer.State.Closed:
                GD.Print($"ReconnectingState:{newState} -- closed the connection...");
                if (Manager.RetriedMaxTimes)
                {
                   GD.Print($"ServerConnectionStateMachine: Max retry attempts made with no reconnection"); 
                   Manager.Listener.UnableToConnect();
                   StateMachine.TransitionTo(new DisconnectedState(StateMachine, manager));
                } else
                    Manager.RetryReconnecting();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}

public class DisconnectingState(ServerConnectionStateMachine stateMachine, ServerConnectionManager manager)
    : ServerConnectionState(stateMachine, manager)
{
    public override void Enter()
    {
        Manager.Listener.Disconnecting();
    }

    public override void Exit()
    {
    }

    public override void HandleNewWebsocketState(WebSocketPeer.State newState)
    {
        switch (newState)
        {
            case WebSocketPeer.State.Connecting:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Open:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Closing:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Closed:
                StateMachine.TransitionTo(new DisconnectedState(StateMachine, manager));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}

public class DisconnectedState(ServerConnectionStateMachine stateMachine, ServerConnectionManager manager)
    : ServerConnectionState(stateMachine, manager)
{
    public override void Enter()
    {
        Manager.Listener.Disconnected();
    }

    public override void Exit()
    {
    }

    public override void HandleNewWebsocketState(WebSocketPeer.State newState)
    {
        switch (newState)
        {
            case WebSocketPeer.State.Connecting:
                StateMachine.TransitionTo(new ConnectingState(StateMachine, Manager));
                break;
            case WebSocketPeer.State.Open:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Closing:
                ThrowStateException(newState);
                break;
            case WebSocketPeer.State.Closed:
                ThrowStateException(newState);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}

public class ServerConnectionStateMachine
{
    private ServerConnectionState _currentState;

    public ServerConnectionStateMachine(ServerConnectionManager manager)
    {
        _currentState = new IdleState(this, manager);
    }

    public void TransitionTo(ServerConnectionState newState)
    {
        _currentState.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void HandleNewWebsocketState(WebSocketPeer.State newState)
    {
        _currentState.HandleNewWebsocketState(newState);
    }
}