using Godot;
using System;
using System.Collections.Generic;
using androidplugintest.ConnectionManager;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.SceneManager;
using BattleshipWithWords.Utilities;

public partial class MultiplayerGame : Control, ISharedNodeReceiver
{
    private List<Node> _nodesToKeepAlive = [];
    private Gameboard _playerOneGameboard;
    private Gameboard _playerTwoGameboard;
    private MultiplayerGameManager _multiplayerGameManager;
    
    private ENetP2PPeerService _peerService;

    private float _playerTwoStartingRotation = (float)Math.PI;
    private float _playerOneStartingRotation;

    public void Init(MultiplayerGameManager gameManager)
    {
        _multiplayerGameManager = gameManager;
        _playerTwoGameboard = ResourceLoader.Load<PackedScene>(ResourcePaths.GameboardNodePath).Instantiate() as Gameboard;
        _playerOneGameboard = ResourceLoader.Load<PackedScene>(ResourcePaths.GameboardNodePath).Instantiate() as Gameboard;
        _playerTwoGameboard.Init(_multiplayerGameManager);
        _playerOneGameboard.Init(_multiplayerGameManager);
        
        _playerOneGameboard.IsControlledLocally = true;
        _playerTwoGameboard.IsControlledLocally = false;

        _playerOneGameboard.LocalUpdateMade += _multiplayerGameManager.LocalUpdateHandler;
        _multiplayerGameManager.LocalUIUpdated += _playerOneGameboard.UIUpdatedHandler;
        _multiplayerGameManager.OpponentUIUpdated += _playerTwoGameboard.UIUpdatedHandler;
        
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
        _playerOneGameboard.LocalUpdateMade += _multiplayerGameManager.LocalUpdateHandler;
        _multiplayerGameManager.LocalUIUpdated += _playerOneGameboard.UIUpdatedHandler;
        _multiplayerGameManager.OpponentUIUpdated += _playerTwoGameboard.UIUpdatedHandler;
    }

    public override void _Ready()
    {
        _playerTwoGameboard.GameboardTitle = "Opponent's Board";
        _playerOneGameboard.GameboardTitle = "Your Board";
        _playerTwoGameboard.InitialRotation = (float)Math.PI;
        AddChild(_playerTwoGameboard);
        AddChild(_playerOneGameboard);
    }

    public Result ReceiveSharedNodes(Node node)
    {
        _peerService = GodotNodeTree.FindFirstNodeOfType<ENetP2PPeerService>(node);
        var allReceived = false;
        
        if (_peerService != null) // add more null checks if new nodes that should be received from another scene are added here
        {
            node.RemoveChild(_peerService);
            AddChild(_peerService);
            allReceived = true;
        }

        return allReceived ? Result.Ok() : Result.Fail("did not receive shared nodes");
    }
}
