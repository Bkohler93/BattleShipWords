using Godot;
using System;

public partial class WinPopup : Control 
{
    [Export] private Button _quitButton;
    
    public event Action OnQuitPressed;

    public override void _Ready()
    {
        _quitButton.Pressed += () => OnQuitPressed?.Invoke();
    }
}
