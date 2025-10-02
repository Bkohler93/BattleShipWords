using Godot;
using System;
using BattleshipWithWords.Controllers.SceneManager;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;

public partial class TurnDeciderToGameplayOverlay : MarginContainer
{
    [Export] public Label Label { get; set; }
    private const int WaitTime = 3;

    public Action Timeout { get; set; }
    public void Init(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            Label.Text = "Your turn!";
        }
        else
        {
            Label.Text = "Your opponent's turn!";
        }
    }

    public override void _Ready()
    {
        var t = new Timer();
        t.WaitTime = WaitTime;
        t.Autostart = true;
        t.Timeout += Timeout;
        AddChild(t);
    }
}
