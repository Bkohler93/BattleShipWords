using System;
using Godot;

namespace BattleshipWithWords.Services.Overlays;

public class PauseOverlay : IOverlay
{
   private PauseMenu _pauseMenu;
   private Action _onQuit;
   public PauseOverlay()
   {
      _pauseMenu =
         ResourceLoader.Load<PackedScene>("res://scenes/overlays/pause_overlay.tscn").Instantiate() as PauseMenu;
   }

   public void SetOnQuit(Action onQuit)
   {
      _onQuit = onQuit;
      _pauseMenu.OnQuitButtonPressed = _onQuit;
   }

   public CanvasLayer GetNode()
   {
      return _pauseMenu;
   }
}