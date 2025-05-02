using System;
using System.Reflection;
using BattleshipWithWords.Controllers.Multiplayer.Setup;
using BattleshipWithWords.Controllers.Multiplayer.Setup.States;
using Godot;
using Godot.Collections;

public partial class MultiplayerSetup : MarginContainer
{
    [Export]
    private GridContainer _gridContainer;

    private SetupController _controller;
    
    [Export] public Button ConfirmButton;
    [Export] public Button NextWordsButton;
    [Export] public Button PreviousWordsButton;
    [Export] public Button ThreeLetterWordButton;
    [Export] public Button FourLetterWordButton;
    [Export] public Button FiveLetterWordButton;
    [Export] private StyleBox _idleStyleBox;
    [Export] private StyleBox _pendingStyleBox;
    [Export] private StyleBox _placeableStyleBox;
    [Export] private StyleBox _placedStyleBox;
    [Export] private StyleBox _pendingFailedStyleBox;
    
    public override void _Ready()
    {
        _controller = new SetupController(this);
        _initializeBoard();
        _initializeUI();

        ConfirmButton.Pressed += _controller.HandleConfirmButton;
        NextWordsButton.Pressed += _controller.HandleNextWordsButton;
        PreviousWordsButton.Pressed += _controller.HandlePreviousWordsButton;
        ThreeLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Three);
        FourLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Four);
        FiveLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Five);
        _controller.TransitionTo(new InitialState(_controller));
    }

    private void _initializeUI()
    {
        ConfirmButton.Text = "\uf058";
        NextWordsButton.Text = "\uf105";
        PreviousWordsButton.Text = "\uf104";
        PreviousWordsButton.Disabled = true;
        ConfirmButton.SetDisabled(true);
        GD.Print("made it here"); 
        ThreeLetterWordButton.Text = _controller.Words[0][_controller.WordSelectionIndex];
        FourLetterWordButton.Text = _controller.Words[1][_controller.WordSelectionIndex];
        FiveLetterWordButton.Text = _controller.Words[2][_controller.WordSelectionIndex];
    }

    private void _onConfirmButtonPressed()
    {
        
    }

    private void _initializeBoard()
    {
        foreach (var name in Theme.GetStyleboxList(GetClass())) {
            GD.Print(name);
        }
    
        var tilePackedScene = ResourceLoader.Load<PackedScene>("res://scenes/games/multiplayer/setup_tile.tscn");
        var height = GetViewportRect().Size.Y;
        var boardSize = Mathf.Min(height * 0.40f, 600); // board is reserved 40% of screen height 
        var tileSize = boardSize / 7f; // 6 tiles but spaces between
        var styleBoxDict = new Dictionary<string, StyleBox>();
        
        for (var i = 0; i < 6; i++)
        {
            _controller.Board.Add([]);
            for (var j = 0; j < 6; j++)
            {
                var tile = tilePackedScene.Instantiate() as SetupTile;
                _gridContainer.AddChild(tile);
                tile.CustomMinimumSize = new Vector2(tileSize, tileSize);
                if (i == 0 && j == 0)
                {
                    styleBoxDict.Add("idle", _idleStyleBox);
                    styleBoxDict.Add("pending", _pendingStyleBox);
                    styleBoxDict.Add("placeable", _placeableStyleBox);
                    styleBoxDict.Add("placed", _placedStyleBox);
                    styleBoxDict.Add("pendingFailed", _pendingFailedStyleBox);
                }
                tile.Init(_controller, i, j, tileSize, styleBoxDict);
                _controller.Board[i].Add(tile);
            }
        }
    }
}
