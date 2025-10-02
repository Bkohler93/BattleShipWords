using System;
using System.Collections.Generic;
using androidplugintest.ConnectionManager;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Controllers.Multiplayer.Internet;
using BattleshipWithWords.Controllers.SceneManager;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Nodes.Globals;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Services.SharedData;
using BattleshipWithWords.Utilities;
using Godot;

public partial class InternetSetup : Control 
{
    private float _separationGap = 40f/6f; 
    
    [Export] private GridContainer _gridContainer;

    private SetupController _controller;
    // private OverlayManager _overlayManager;
    private ServerConnectionManager _serverConnectionManager;
    
    public Auth Auth => GetNode<Auth>("/root/Auth");
    public Action OnSetupComplete { get; set; }
    public Action OnLocalSetupComplete { get; set; }

    [Export] public Button ConfirmButton;

    [Export] private Button _nextThreeLetterWordButton;
    [Export] private Button _threeLetterWordButton; 
    [Export] private Button _prevThreeLetterWordButton;
    
    [Export] private Button _nextFourLetterWordButton;
    [Export] private Button _fourLetterWordButton;
    [Export] private Button _prevFourLetterWordButton;
    
    [Export] private Button _nextFiveLetterWordButton;
    [Export] private Button _fiveLetterWordButton;
    [Export] private Button _prevFiveLetterWordButton;
    
    [Export] private StyleBox _idleStyleBox;
    [Export] private StyleBox _pendingStyleBox;
    [Export] private StyleBox _placeableStyleBox;
    [Export] private StyleBox _placedStyleBox;
    [Export] private StyleBox _pendingFailedStyleBox;
    
    public readonly Dictionary<int, Button> WordButtons = new(); 
    public readonly Dictionary<int, Button> NextWordButtons = new();
    public readonly Dictionary<int, Button> PreviousWordButtons = new();

    // public void Init(OverlayManager overlayManager)
    // {
    //     _overlayManager = overlayManager;
    // }
    
    public override void _Ready()
    {
        _serverConnectionManager = AppRoot.Services.RetrieveService<ServerConnectionManager>();
        WordButtons[3] = _threeLetterWordButton;
        WordButtons[4] = _fourLetterWordButton;
        WordButtons[5] = _fiveLetterWordButton;
        
        NextWordButtons[3] = _nextThreeLetterWordButton;
        NextWordButtons[4] = _nextFourLetterWordButton;
        NextWordButtons[5] = _nextFiveLetterWordButton;
        
        PreviousWordButtons[3] = _prevThreeLetterWordButton;
        PreviousWordButtons[4] = _prevFourLetterWordButton;
        PreviousWordButtons[5] = _prevFiveLetterWordButton;
        
        var startSetupData = AppRoot.Services.RetrieveService<SharedData>().Consume<StartSetup>(); 
        
        _controller = new SetupController(this, _serverConnectionManager, startSetupData);
        _initializeBoard();
        _initializeUI();

        ConfirmButton.Pressed += _controller.HandleConfirmButton;
        
        _nextThreeLetterWordButton.Pressed += () =>
        {
            _controller.HandleNextWordsButton(3);
        };
        _nextFourLetterWordButton.Pressed += () =>
        {
            _controller.HandleNextWordsButton(4);
        };
        _nextFiveLetterWordButton.Pressed += () =>
        {
            _controller.HandleNextWordsButton(5);
        };
        
        _prevThreeLetterWordButton.Pressed += () =>
        {
            _controller.HandlePreviousWordsButton(3);
        };
        _prevFourLetterWordButton.Pressed += () =>
        {
            _controller.HandlePreviousWordsButton(4);   
        };
        _prevFiveLetterWordButton.Pressed += () =>
        {
            _controller.HandlePreviousWordsButton(5);
        };
        
        _threeLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Three);
        _fourLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Four);
        _fiveLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Five);
        _controller.TransitionTo(new InitialState(_controller));
        // _controller.AutoSetup();
        if (PlatformUtils.GetPlatform() == Platform.Other)
            _controller.AutoSetup();
    }

    private void _initializeUI()
    {
        ConfirmButton.Text = "\uf058";
        
        _nextThreeLetterWordButton.Text = "\uf105";
        _nextFourLetterWordButton.Text = "\uf105";
        _nextFiveLetterWordButton.Text = "\uf105";
        
        _prevThreeLetterWordButton.Text = "\uf104";
        _prevFourLetterWordButton.Text = "\uf104";
        _prevFiveLetterWordButton.Text = "\uf104";
        
        ConfirmButton.SetDisabled(true);
        _threeLetterWordButton.Text = _controller.WordOptions[3][_controller.WordSelectionIndices[3]];
        _fourLetterWordButton.Text = _controller.WordOptions[4][_controller.WordSelectionIndices[4]];
        _fiveLetterWordButton.Text = _controller.WordOptions[5][_controller.WordSelectionIndices[5]];
    }

    private void _onConfirmButtonPressed()
    {
        
    }

    private void _initializeBoard()
    {
        foreach (var name in Theme.GetStyleboxList(GetClass())) {
            GD.Print(name);
        }
        var tilePackedScene = ResourceLoader.Load<PackedScene>(ResourcePaths.SetupTileNodePath);
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

    // public Result ReceiveSharedNodes(Node node)
    // {
    //     GD.Print("MultiplayerSetup:ReceiveSharedNodes()---");
    //     _serverConnectionManager = GodotNodeTree.FindFirstNodeOfType<ServerConnectionManager>(node);
    //     var allReceived = false;
    //     
    //     if (_serverConnectionManager != null) // add more null checks if new nodes that should be received from another scene are added here
    //     {
    //         node.RemoveChild(_serverConnectionManager);
    //         AddChild(_serverConnectionManager);
    //         GD.Print($"MultiplayerSetup:ReceiveSharedNodes()--- added serverConnectionManager to {GetType().Name}");
    //         allReceived = true;
    //     }
    //
    //     return allReceived ? Result.Ok() : Result.Fail("did not receive shared nodes");
    // }
}
