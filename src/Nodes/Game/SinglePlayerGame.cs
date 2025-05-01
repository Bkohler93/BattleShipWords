using Godot;
using System;
using BattleshipWithWords.Games;

public partial class SinglePlayerGame : Control, IGame
{
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

}
