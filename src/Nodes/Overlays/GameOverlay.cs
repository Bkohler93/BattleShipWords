using Godot;
using System;
using BattleshipWithWords.Utilities;

public partial class GameOverlay : Control 
{
    [Export] private NodePath _playerAvatarNodePath;
    [Export] private NodePath _opponentAvatarNodePath;
    [Export] private Button _pauseButton;
    
    private Avatar _playerAvatar;
    private Avatar _opponentAvatar;
    private bool _isPlayerTurn;
    
    public Action OnPauseButtonPressed;

    public override void _Ready()
    {
        _pauseButton.Text = "\uf04c";
        _pauseButton.Pressed += OnPauseButtonPressed;
        _playerAvatar = GetNode<Avatar>(_playerAvatarNodePath);
        _opponentAvatar = GetNode<Avatar>(_opponentAvatarNodePath);
        Logger.Print($"node ready with nodes {_playerAvatar} {_opponentAvatar}");
    }

    public void SetPlayerTurn()
    {
        Logger.Print("setting player turn");
        _isPlayerTurn = true;
        _opponentAvatar.EndTurn();
        _playerAvatar.StartTurn();
    }

    public void SetOpponentTurn()
    {
        _isPlayerTurn = false;
        _playerAvatar.EndTurn();
        _opponentAvatar.StartTurn();
    }

    public void OnServerTick(uint timeLeft)
    {
        if (_isPlayerTurn)
            _playerAvatar.OnServerTick();
        else
            _opponentAvatar.OnServerTick();
    }
}
