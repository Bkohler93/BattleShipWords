using Godot;
using System;

public partial class Lose : CanvasLayer
{
    [Export] private Button _quitButton;

    public event Action OnQuitPressed;

    public override void _Ready()
    {
        _quitButton.Pressed += () => OnQuitPressed?.Invoke();
    }
}
