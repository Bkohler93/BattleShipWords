using Godot;
using System;
using BattleshipWithWords.Services;


public partial class Settings : Control
{
    [Export]
    private Button _backButton;
    public Action OnBackButtonPressed;

    public override void _Ready()
    {
        _backButton.Pressed += OnBackButtonPressed;
    }
}
