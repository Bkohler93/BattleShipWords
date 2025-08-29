using System;
using System.Net.WebSockets;
using BattleshipWithWords.Services.ConnectionManager.Server;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;

public abstract class InternetMatchmakingState:IServerConnectionListener
{
   public abstract void Enter();
   public abstract void Exit();
   public abstract void OnCancelButtonPressed();

   public virtual void OnPlayButtonPressed()
   {
      throw new Exception($"[InternetMatchmakingState] OnPlayButtonPressed not implemented for implementation of {GetType().Name}");
   }

   public abstract void Connecting();
   public abstract void Connected();
   public abstract void UnableToConnect();
   public abstract void Disconnected();
   public abstract void Reconnecting();
   public abstract void Reconnected();
   public abstract void Receive(IServerReceivable message);
   public abstract void Disconnecting();
   public abstract void HttpResponse(string response);
}