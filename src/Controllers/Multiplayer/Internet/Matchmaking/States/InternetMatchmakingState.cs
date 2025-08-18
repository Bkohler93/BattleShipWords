using System;
using System.Net.WebSockets;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;

public abstract class InternetMatchmakingState
{
   public abstract void Enter();
   public abstract void Exit();
   public abstract void OnCancelButtonPressed();

   public virtual void OnPlayButtonPressed()
   {
      throw new Exception($"[InternetMatchmakingState] OnPlayButtonPressed not implemented for implementation of {GetType().Name}");
   }
}