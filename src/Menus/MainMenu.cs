using Godot;
using System;
using BattleshipWithWords.Services;

public partial class MainMenu : Control
{
    public Action OnSinglePlayerButtonPressed;
    public Action OnMultiplayerButtonPressed;
    public Action OnSettingsButtonPressed;
    public Action OnQuitButtonPressed;

    private Button _singlePlayerButton;
    private Button _multiplayerButton;
    private Button _settingsButton;
    private Button _quitButton;
    
    public override void _Ready()
    {
        _singlePlayerButton = GetNode<Button>("VBoxContainer3/VBoxContainer2/SinglePlayerButton");
        _multiplayerButton = GetNode<Button>("VBoxContainer3/VBoxContainer2/MultiplayerButton");
        _settingsButton = GetNode<Button>("VBoxContainer3/VBoxContainer2/SettingsButton");
        _quitButton = GetNode<Button>("VBoxContainer3/VBoxContainer2/QuitButton");
        
        _singlePlayerButton.Pressed += OnSinglePlayerButtonPressed;
        _multiplayerButton.Pressed += OnMultiplayerButtonPressed;
        _settingsButton.Pressed += OnSettingsButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;
    }
}
