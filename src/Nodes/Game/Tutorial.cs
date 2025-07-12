using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Games;

public partial class Tutorial : Control, IGame, ISceneNode
{
    private List<Node> _nodesToKeepAlive = [];
    private Label _label;
    public Action OnPause { get; set; }
    public Action OnFinish { get; set; }
    public Action OnQuit { get; set; }

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");
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
