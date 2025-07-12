using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Services;


public partial class Settings : Control, ISceneNode
{
    [Export]
    private Button _backButton;
    public Action OnBackButtonPressed;
    private List<Node> _nodesToKeepAlive = [];

    public override void _Ready()
    {
        _backButton.Pressed += OnBackButtonPressed;
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
