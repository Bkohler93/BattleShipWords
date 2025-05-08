using Godot;
using System;

public partial class PeerDisconnected : CanvasLayer
{
    [Export] private Button _returnToMenuButton;
    public Action ReturnToMenuButtonPressed;

    public override void _Ready()
    {
        _returnToMenuButton.Pressed += ReturnToMenuButtonPressed;
    }
}
