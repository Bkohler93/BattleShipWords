using Godot;
using System;
using BattleshipWithWords.Utilities;
using Colors = BattleshipWithWords.Utilities.Colors;

public partial class KeyboardKey :Button 
{
    [Export] private Label _letterLabel;
    [Export] private Label _statusLabel;
    private string _letter;
    public event Action<string> ButtonPressed; 

    public void Initialize(string letter)
    {
        _letter = letter;
    } 
    
    public override void _Ready()
    {
        _letterLabel.Text = _letter;
        Pressed += () => ButtonPressed?.Invoke(_letter);
    }

    public void SetInPlay()
    {
        _statusLabel.Text = "\uf1ce";
    }

    public void SetAllFound()
    {
        _statusLabel.Text = "\uf111";
    }

    public void SetOutOfPlay()
    {
        _statusLabel.Text = "";
        var sb = GetThemeStylebox("disabled");
        AddThemeStyleboxOverride("normal", sb);
    }
}
