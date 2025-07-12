using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;

public partial class MultiplayerMenu : Control, ISceneNode
{
    private List<Node> _nodesToKeepAlive = [];
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

    public List<Node> GetNodesToShare()
    {
        return _nodesToKeepAlive;
    }

    public void AddNodeToShare(Node node)
    {
        _nodesToKeepAlive.Add(node);
    }
}
