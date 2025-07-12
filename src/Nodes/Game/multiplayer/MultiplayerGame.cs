using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Services.GameManager;

public partial class MultiplayerGame : Control, ISceneNode
{
    private List<Node> _nodesToKeepAlive = [];
    private Gameboard _playerOneGameboard;
    private Gameboard _playerTwoGameboard;
    private MultiplayerGameManager _multiplayerGameManager;

    private float _playerTwoStartingRotation = (float)Math.PI;
    private float _playerOneStartingRotation;

    public void Init(MultiplayerGameManager gameManager)
    {
        _multiplayerGameManager = gameManager;
        _playerTwoGameboard = ResourceLoader.Load<PackedScene>("res://scenes/games/multiplayer/gameboard.tscn").Instantiate() as Gameboard;
        _playerOneGameboard = ResourceLoader.Load<PackedScene>("res://scenes/games/multiplayer/gameboard.tscn").Instantiate() as Gameboard;
        _playerTwoGameboard.Init(_multiplayerGameManager);
        _playerOneGameboard.Init(_multiplayerGameManager);
        
        _playerOneGameboard.IsControlledLocally = true;
        _playerTwoGameboard.IsControlledLocally = false;

        _playerOneGameboard.LocalUpdateMade += _multiplayerGameManager.LocalUpdateHandler;
        // _playerOneGameboard.GuessMade += _multiplayerGameManager.GuessMadeEventHandler;
        // _playerOneGameboard.TileUncovered += _multiplayerGameManager.TileUncoveredEventHandler;
        // _playerOneGameboard.BackspacePressed += _multiplayerGameManager.LocalBackspacePressedEventHandler;
        // _playerOneGameboard.KeyPressed += _multiplayerGameManager.LocalKeyPressedEventHandler;
        
        _multiplayerGameManager.LocalUIUpdated += _playerOneGameboard.UIUpdatedHandler;
        // _multiplayerGameManager.GuessResultReceived += _playerOneGameboard.ProcessGuessResult;
        // _multiplayerGameManager.TileUncoverResultReceived += _playerOneGameboard.ProcessUncoverTile;

        _multiplayerGameManager.OpponentUIUpdated += _playerTwoGameboard.UIUpdatedHandler;
        
        // _multiplayerGameManager.OpponentUncoveredTile += _playerTwoGameboard.ProcessUncoverTile;
        
        _playerOneGameboard.OnRotate += () =>
        {
            var t = GetTree().CreateTween().SetParallel();
            t.TweenProperty(_playerOneGameboard, "rotation", (float) Math.PI * -1, 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
            t.TweenProperty(_playerTwoGameboard, "rotation", 0, 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
            t.Play();
        };
        _playerTwoGameboard.OnRotate += () =>
        {
            var t = GetTree().CreateTween().SetParallel();
            t.TweenProperty(_playerTwoGameboard, "rotation", (float) Math.PI, 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
            t.TweenProperty(_playerOneGameboard, "rotation", 0, 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
            t.Play();
        };
    }

    public override void _ExitTree()
    {
        _multiplayerGameManager.OpponentUIUpdated -= _playerTwoGameboard.UIUpdatedHandler;     
        
        // _playerOneGameboard.GuessMade -= _multiplayerGameManager.GuessMadeEventHandler;
        // _playerOneGameboard.TileUncovered -= _multiplayerGameManager.TileUncoveredEventHandler;
        // _multiplayerGameManager.GuessResultReceived -= _playerOneGameboard.ProcessGuessResult;
        // _multiplayerGameManager.TileUncoverResultReceived -= _playerOneGameboard.ProcessUncoverTile;
        // _multiplayerGameManager.OpponentGuessed -= _playerTwoGameboard.ProcessGuessResult;
        // _multiplayerGameManager.OpponentUncoveredTile -= _playerTwoGameboard.ProcessUncoverTile;
        // _multiplayerGameManager.OpponentBackspacePressed -= _playerTwoGameboard.BackspacePressedEventHandler;
    }

    public override void _Ready()
    {
        _playerTwoGameboard.GameboardTitle = "Opponent's Board";
        _playerOneGameboard.GameboardTitle = "Your Board";
        _playerTwoGameboard.InitialRotation = (float)Math.PI;
        AddChild(_playerTwoGameboard);
        AddChild(_playerOneGameboard);
    }

    public List<Node> GetNodesToShare()
    {
        return _nodesToKeepAlive;
    }

    public void AddNodeToShare(Node node)
    {
        _nodesToKeepAlive.Add(node);
    }
}
