using Godot;
using System;
using BattleshipWithWords.Services;


public partial class Settings : Control
{
    private Button _backButton;
    public Action OnBackButtonPressed;

    public override void _Ready()
    {
        _backButton = GetNode<Button>("MarginContainer/Button");
        _backButton.Pressed += OnBackButtonPressed;
    }
}
