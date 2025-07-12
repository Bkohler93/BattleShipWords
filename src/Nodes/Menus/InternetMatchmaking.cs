using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;

public partial class InternetMatchmaking : Control, ISceneNode
{
    private List<Node> _nodesToKeepAlive = [];
    [Export]
    private Button _cancelButton;
    
    [Export]
    private Label _statusLabel;
    
    [Export]
    private Label _infoLabel;
    
    public Action OnCancelButtonPressed;

    public override void _Ready()
    {
        _cancelButton.Pressed += OnCancelButtonPressed;
    }

    public List<Node> GetNodesToShare() => _nodesToKeepAlive;
    public void AddNodeToShare(Node node)
    {
        _nodesToKeepAlive.Add(node);
    }
}
