using System;
using Godot;

namespace BattleshipWithWords.Nodes.Game.multiplayer;

public partial class BackspaceButton: Button
{
   public BackspaceButton()
   {
      Text = "\uf55a";
      SetCustomMinimumSize(new Vector2(100, 60));
   }

   public event Action ButtonPressed;

   public override void _Ready()
   {
      Pressed += () => ButtonPressed?.Invoke();
   }
}