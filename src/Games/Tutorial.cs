using Godot;
using System;

public partial class Tutorial : Control
{
    private Label _label;
    public Action TutorialDone;

    public override void _Ready()
    {
        _label = GetNode<Label>("Label");
        var timer = new Timer();
        AddChild(timer);
        timer.Start(5);
        timer.Timeout += TutorialDone;
    }
}
