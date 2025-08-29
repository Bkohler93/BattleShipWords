using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using androidplugintest.ConnectionManager;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.Multiplayer.Internet.TurnDecider;
using BattleshipWithWords.Controllers.SceneManager;
using BattleshipWithWords.Nodes.Globals;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;

public partial class InternetTurnDecider : Control, ISharedNodeReceiver
{
    private Gameboard _playerOneGameboard;
    private Gameboard _playerTwoGameboard;
    private OverlayManager _overlayManager;
    
    public ServerConnectionManager ConnectionManager;
    private TurnDeciderController _controller;

    private float _playerTwoStartingRotation = (float)Math.PI;
    private float _playerOneStartingRotation;

    public Auth Auth => GetNode<Auth>("/root/Auth");
    
    public Action OnExitGame;

    public void Init(OverlayManager overlayManager)
    {
        _controller = new TurnDeciderController(this);
        _overlayManager = overlayManager;
        _playerTwoGameboard = ResourceLoader.Load<PackedScene>(ResourcePaths.GameboardNodePath).Instantiate() as Gameboard;
        _playerOneGameboard = ResourceLoader.Load<PackedScene>(ResourcePaths.GameboardNodePath).Instantiate() as Gameboard;
        
        _playerOneGameboard.IsControlledLocally = true;
        _playerTwoGameboard.IsControlledLocally = false;

        // _playerOneGameboard.LocalUpdateMade += _multiplayerGameManager.LocalUpdateHandler;
        _playerOneGameboard.LocalUpdateMade += _controller.HandleLocalUpdate;
       _controller.LocalUIUpdated  += _playerOneGameboard.UIUpdatedHandler;
       _controller.OpponentUIUpdated  += _playerTwoGameboard.UIUpdatedHandler;
        // _multiplayerGameManager.LocalUIUpdated += _playerOneGameboard.UIUpdatedHandler;
        // _multiplayerGameManager.OpponentUIUpdated += _playerTwoGameboard.UIUpdatedHandler;
        
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
        // _playerOneGameboard.LocalUpdateMade += _multiplayerGameManager.LocalUpdateHandler;
        // _multiplayerGameManager.LocalUIUpdated += _playerOneGameboard.UIUpdatedHandler;
        // _multiplayerGameManager.OpponentUIUpdated += _playerTwoGameboard.UIUpdatedHandler;
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
        ConnectionManager = GodotNodeTree.FindFirstNodeOfType<ServerConnectionManager>(node);
        var allReceived = false;
        
        if (ConnectionManager != null) // add more null checks if new nodes that should be received from another scene are added here
        {
            node.RemoveChild(ConnectionManager);
            AddChild(ConnectionManager);
            allReceived = true;
        }

        return allReceived ? Result.Ok() : Result.Fail("did not receive shared nodes");
    }
}
