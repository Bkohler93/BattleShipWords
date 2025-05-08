using System;
using Godot;

namespace BattleshipWithWords.Controllers;

public class PauseOverlay : IOverlay
{
   private PauseMenu _pauseMenu;
   public Action ExitButtonPressed;
   public Action ContinueButtonPressed;
   public Action PauseButtonPressed;
   
   public PauseOverlay()
   {
      _pauseMenu =
         ResourceLoader.Load<PackedScene>("res://scenes/overlays/pause_overlay.tscn").Instantiate() as PauseMenu;
      _pauseMenu.OnQuitButtonPressedEventHandler += () => ExitButtonPressed?.Invoke();
      _pauseMenu.OnContinueButtonPressedEventHandler += () => ContinueButtonPressed?.Invoke();
      _pauseMenu.OnPauseButtonPressedEventHandler += () => PauseButtonPressed?.Invoke();
   }

   public CanvasLayer GetNode()
   {
      return _pauseMenu;
   }
}