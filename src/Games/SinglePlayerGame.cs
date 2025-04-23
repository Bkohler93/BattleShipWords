using Godot;
using System;

public partial class SinglePlayerGame : Control
{
    private Button _pauseButton;
    public Action OnPause;
    public Action OnFinish;

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
