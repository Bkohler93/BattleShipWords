using System;
using Godot;

namespace BattleshipWithWords.Controllers;

public class LoseOverlay : IOverlay
{
   public Action QuitButtonPressed;
   private Lose _loseNode;
   
   public LoseOverlay()
   {
      _loseNode =
         ResourceLoader.Load<PackedScene>("res://scenes/overlays/lose_overlay.tscn").Instantiate() as Lose;
      _loseNode.OnQuitPressed += () => QuitButtonPressed?.Invoke();
   }

   public CanvasLayer GetNode()
   {
      return _loseNode;
   }
}