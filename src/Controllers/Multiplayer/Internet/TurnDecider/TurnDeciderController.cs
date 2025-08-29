using System;
using BattleshipWithWords.Controllers.Multiplayer.Internet.TurnDecider.States;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.TurnDecider;

public class TurnDeciderController
{
    public InternetTurnDecider Node;
    public TurnDeciderState CurrentState;

    public TurnDeciderController(InternetTurnDecider node)
    {
        Node = node;
        CurrentState = new GuessingState(this);
    }

    public Action<UIUpdate> LocalUIUpdated { get; set; }
    public Action<UIUpdate> OpponentUIUpdated { get; set; }

    public void HandleLocalUpdate(UIEvent @event)
    {
        CurrentState.HandleLocalUpdate(@event);
    }
}

public abstract class TurnDeciderState:IServerConnectionListener
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
    public abstract void HandleLocalUpdate(UIEvent @event);
}