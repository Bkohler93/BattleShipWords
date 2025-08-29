using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Controllers.Multiplayer.Internet;
using BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;
using BattleshipWithWords.Nodes.Globals;
using BattleshipWithWords.Services.ConnectionManager;
using BattleshipWithWords.Services.ConnectionManager.Server;

public partial class InternetMatchmaking : Control
{
    [Export]
    private Button _cancelButton;

    [Export] private Button _playButton;
    
    [Export]
    private Label _statusLabel;
    
    [Export]
    private Label _infoLabel;
    public Auth Auth => GetNode<Auth>("/root/Auth");
    
    public Action OnCancelButtonPressed;
    public Action<StartSetup> OnStartSetup;
    private InternetMatchmakingController _controller;
    private ServerConnectionManager _connectionManager;

    public override void _Ready()
    {
        _connectionManager = new ServerConnectionManager();
        AddChild(_connectionManager);
        _controller = new InternetMatchmakingController(this, _connectionManager);
        
        _playButton.Hide();
        _cancelButton.Pressed += () =>
        {
            _controller.OnCancelButtonPressed();
            OnCancelButtonPressed?.Invoke();
        };
        _playButton.Pressed += () =>
        {
            _controller.OnPlayButtonPressed();
        };
        _controller.Ready();
    }

    public void SetInfo(string text)
    {
        _infoLabel.Text = text;
    }

    public void ShowPlayButton()
    {
        _playButton.Show();
    }

    public void HidePlayButton()
    {
        _playButton.Hide();
    }
}
