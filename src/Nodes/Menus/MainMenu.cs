using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Services;
using BattleshipWithWords.Utilities;

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
        Logger.Print("main menu ready");
        _singlePlayerButton.Pressed += OnSinglePlayerButtonPressed;
        _multiplayerButton.Pressed += OnMultiplayerButtonPressed;
        _settingsButton.Pressed += OnSettingsButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;
    }
    
    
    public override void _ExitTree()
    {
    }
}
