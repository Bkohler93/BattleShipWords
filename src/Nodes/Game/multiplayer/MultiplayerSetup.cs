using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.Multiplayer.Setup;
using BattleshipWithWords.Networkutils;
using Godot;

public partial class MultiplayerSetup : MarginContainer
{
    private float _separationGap = 40f/6f; 
    
    [Export] private GridContainer _gridContainer;

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
    
    public Action<List<string>,List<List<(int row, int col)>>> SetupCompleteCallback;
    
    public override void _Ready()
    {
        _initializeBoard();
        _initializeUI();

        ConfirmButton.Pressed += _controller.HandleConfirmButton;
        NextWordsButton.Pressed += _controller.HandleNextWordsButton;
        PreviousWordsButton.Pressed += _controller.HandlePreviousWordsButton;
        ThreeLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Three);
        FourLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Four);
        FiveLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Five);
        _controller.TransitionTo(new InitialState(_controller));
        // _controller.AutoSetup();
        if (PlatformUtils.GetPlatform() == Platform.Other)
            _controller.AutoSetup();
    }


    public void Init(MultiplayerGameManager gameManager)
    {
        _controller = new SetupController(this, gameManager);
    } 

    private void _initializeUI()
    {
        ConfirmButton.Text = "\uf058";
        NextWordsButton.Text = "\uf105";
        PreviousWordsButton.Text = "\uf104";
        PreviousWordsButton.Disabled = true;
        ConfirmButton.SetDisabled(true);
        ThreeLetterWordButton.Text = _controller.WordOptions[0][_controller.WordsSelectionIndex];
        FourLetterWordButton.Text = _controller.WordOptions[1][_controller.WordsSelectionIndex];
        FiveLetterWordButton.Text = _controller.WordOptions[2][_controller.WordsSelectionIndex];
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
        var contentScreenHeight = GetViewportRect().Size.Y - (48 + 34);
        var boardSize = Mathf.Min(contentScreenHeight * 0.40f, 600); // board is reserved 40% of screen height 
        var tileSize = boardSize / 6f - _separationGap; 
        var styleBoxDict = new Godot.Collections.Dictionary<string, StyleBox>();
        
        for (var i = 0; i < 6; i++)
        {
            _controller.Board.Add([]);
            for (var j = 0; j < 6; j++)
            {
                if (i == 0 && j == 0)
                {
                    styleBoxDict.Add("idle", _idleStyleBox);
                    styleBoxDict.Add("pending", _pendingStyleBox);
                    styleBoxDict.Add("placeable", _placeableStyleBox);
                    styleBoxDict.Add("placed", _placedStyleBox);
                    styleBoxDict.Add("pendingFailed", _pendingFailedStyleBox);
                }
                var tile = tilePackedScene.Instantiate() as SetupTile;
                tile.CustomMinimumSize = new Vector2(tileSize, tileSize);
                tile.Init(_controller, i, j, tileSize, styleBoxDict);
                _gridContainer.AddChild(tile);
                
                _controller.Board[i].Add(tile);
            }
        }
    }

}
