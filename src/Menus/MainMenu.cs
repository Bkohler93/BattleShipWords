using Godot;
using System;
using BattleshipWithWords.Services;

public partial class MainMenu : Control
{
    public Action OnSinglePlayerButtonPressed;
    public Action OnMultiplayerButtonPressed;
    public Action OnSettingsButtonPressed;
    public Action OnQuitButtonPressed;
    
    [Export]
    private Button _singlePlayerButton;
    
    [Export]
    private Button _multiplayerButton;
    
    [Export]
    private Button _settingsButton;
    
    [Export]
    private Button _quitButton;
    
    public override void _Ready()
    {
        _singlePlayerButton.Pressed += OnSinglePlayerButtonPressed;
        _multiplayerButton.Pressed += OnMultiplayerButtonPressed;
        _settingsButton.Pressed += OnSettingsButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;
    }
}
