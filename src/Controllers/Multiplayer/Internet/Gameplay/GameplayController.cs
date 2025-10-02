using System;
using BattleshipWithWords.Nodes.Game;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay;

public class InternetGameplayController
{
    public InternetGame Node;
    public GameplayState CurrentState;

    public InternetGameplayController(InternetGame node)
    {
        Node = node;
    }

    public Action<UIUpdate> LocalUIUpdated { get; set; }
    public Action<UIUpdate> OpponentUIUpdated { get; set; }
    public Action<uint> ReceivedServerTick;

    public bool HandleLocalUpdate(IUIEvent @event)
    {
        return CurrentState.HandleLocalUpdate(@event);
    }

    public void TransitionTo(GameplayState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        Node.ConnectionManager.Listener = CurrentState;
        CurrentState.Enter();
    }

}

public abstract class GameplayState:IServerConnectionListener
{
    public abstract void Enter();
    public abstract void Exit();
    public virtual void HandleGuessTile(Coordinate tileCoordinate){}
    public virtual void HandleGuessWord(string word){}
    public abstract void Connecting();
    public abstract void Connected();
    public abstract void UnableToConnect();
    public abstract void Disconnected();
    public abstract void Reconnecting();
    public abstract void Reconnected();
    public abstract void Receive(IServerReceivable message);
    public abstract void Disconnecting();
    public abstract void HttpResponse(string response);
    public abstract bool HandleLocalUpdate(IUIEvent @event);
}