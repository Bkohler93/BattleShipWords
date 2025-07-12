using System;
using androidplugintest.ConnectionManager;
using androidplugintest.PeerFinderPlugin;
using BattleshipWithWords.Utilities;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

public abstract class LocalMatchmakingState: IP2PConnectionListener, IPeerFinderListener
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void OnBackPressed();
    public abstract void OnActionPressed();

    public virtual void OnStarted()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement OnStarted");
    }

    public virtual void OnStopped()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement OnStopped");
    }

    public virtual void OnStartErrorOccurred(string error)
    {
        throw new NotImplementedException($"{GetType().Name} does not implement OnStartErrorOccurred");
    }

    public virtual void OnStopErrorOccured(string error)
    {
        throw new NotImplementedException($"{GetType().Name} does not implement OnStopErrorOccured");
    }

    public virtual void OnFailedToResolveService(string error)
    {
        
        throw new NotImplementedException($"{GetType().Name} does not implement OnFailedToResolveService");
    }

    public virtual void OnFoundService(ServiceInfo serviceInfo)
    {
        throw new NotImplementedException($"{GetType().Name} does not implement OnFoundService");
    }

    public virtual void Connected()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement Connected");
    }

    public virtual void UnableToConnect()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement UnableToConnect");
    }

    public virtual void Reconnected()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement Reconnected");
    }

    public virtual void ReconnectFailed()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement ReconnectFailed");
    }

    public virtual void Disconnected()
    {
        throw new NotImplementedException($"{GetType().Name} does not implement Disconnected");
    }

    public virtual void Receive(Dictionary message)
    {
        throw new NotImplementedException($"{GetType().Name} does not implement Receive");
    }
}

public enum LocalMatchmakingMessageType
{
    Request,
    Response
}

public record LocalMatchmakingMessage : IGodotSerializable
{
    public required string FromName;
    public required LocalMatchmakingMessageType Type;
    public required string Msg;

    public Dictionary ToDictionary()
    {
        return new Dictionary
        {
            { "FromName", FromName },
            { "Type", (int)Type },
            { "Msg", Msg }
        };
    }

    public static LocalMatchmakingMessage FromDictionary(Dictionary d)
    {
        return new LocalMatchmakingMessage
        {
            FromName = d["FromName"].ToString(),
            Type = (LocalMatchmakingMessageType)(int)d["Type"],
            Msg = (string)d["Msg"]
        };
    }
}

