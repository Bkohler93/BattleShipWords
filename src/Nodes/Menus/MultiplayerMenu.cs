using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;

public partial class MultiplayerMenu : Control
{
    [Export]
    private Button _backButton;
    
    [Export]
    private Button _localPlayButton;
    
    [Export]
    private Button _onlinePlayButton;
    
    public Action OnBackButtonPressed;
    public Action OnLocalButtonPressed;
    public Action OnOnlineButtonPressed;
    
    public override void _Ready()
    {
        _backButton.Pressed += OnBackButtonPressed;
        _localPlayButton.Pressed += OnLocalButtonPressed;
        _onlinePlayButton.Pressed += OnOnlineButtonPressed;
    }
}
