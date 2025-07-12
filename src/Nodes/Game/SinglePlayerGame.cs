using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Games;

public partial class SinglePlayerGame : Control, IGame, ISceneNode
{
    private List<Node> _nodesToKeepAlive = [];
    private Button _pauseButton;
    public Action OnPause { get; set; }
    public Action OnFinish { get; set; }
    public Action OnQuit { get; set; }

    public override void _Ready()
    {
        _pauseButton = GetNode<Button>("MarginContainer/PauseButton");
        _pauseButton.Pressed += OnPause;
        var timer = new Timer();
        AddChild(timer);
        timer.Start(5);
        timer.Timeout += OnFinish;
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
