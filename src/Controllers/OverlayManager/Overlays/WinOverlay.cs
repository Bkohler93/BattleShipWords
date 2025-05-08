using System;
using Godot;

namespace BattleshipWithWords.Controllers;

public class WinOverlay : IOverlay
{
   public Action QuitButtonPressed;
   private Win _winNode;
   
   public WinOverlay()
   {
      _winNode =
         ResourceLoader.Load<PackedScene>("res://scenes/overlays/win_overlay.tscn").Instantiate() as Win;
      _winNode.OnQuitPressed += () => QuitButtonPressed?.Invoke();
   }

   public CanvasLayer GetNode()
   {
      return _winNode;
   }
}