using System;
using System.Reflection;
using BattleshipWithWords.Utilities;
using Godot;
using Colors = BattleshipWithWords.Utilities.Colors;

public partial class Tile : Button
{
    private int _row;
    private int _col;
    private float _tileSize;
    public event Action<int, int> ButtonPressed;

    public void Init(int row, int col, float tileSize)
    {
        _row = row;
        _col = col;
        _tileSize = tileSize;
    }

    public override void _Ready()
    {
        Pressed += () => ButtonPressed?.Invoke(_row, _col);
    }

    public void SetDisabledStyleBox(string name)
    {
        var styleBox = GetThemeStylebox(name);
        AddThemeStyleboxOverride("disabled", styleBox);
    }

    public void SetInPlay(string letter)
    {
        Text = letter;
        SetDisabled(true);
    }

    public void SetOutOfPlay(string letter)
    {
        Text = letter; 
        SetDisabled(true);
        var color =Colors.GetColor(ColorName.White);
        color.A = 0.4f;
        Modulate = color;
    }
}
