using Godot;
using System;

public partial class MultiplayerMenu : Control
{
    [Export]
    private Button _backButton;
    
    [Export]
    private Button _localPlayButton;
    public Action OnBackButtonPressed;
    public Action OnLocalButtonPressed;
    
    public override void _Ready()
    {
        _backButton.Pressed += OnBackButtonPressed;
        _localPlayButton.Pressed += OnLocalButtonPressed;
    }
}
