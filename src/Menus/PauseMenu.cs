using Godot;
using System;

public partial class PauseMenu : Control
{
    private Button _continueButton;
    private Button _quitButton;

    public Action OnContinue;
    public Action OnQuit;

    public override void _Ready()
    {
        _continueButton = GetNode<Button>("VBoxContainer/ContinueButton");
        _quitButton = GetNode<Button>("VBoxContainer/QuitButton");
        
        _continueButton.Pressed += OnContinue;
        _quitButton.Pressed += OnQuit;
    }
}
