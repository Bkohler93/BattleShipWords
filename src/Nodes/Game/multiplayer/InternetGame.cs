using Godot;
using System;
using System.Text.Json.Serialization;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay;
using BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay.States;
using BattleshipWithWords.Nodes.Globals;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Services.NodeGetter;
using BattleshipWithWords.Utilities;

namespace BattleshipWithWords.Nodes.Game;

public partial class InternetGame : Control 
{
    private Gameboard _playerOneGameboard;
    private Gameboard _playerTwoGameboard;
    // private OverlayManager _overlayManager;
    private GameOverlay _gameOverlay;

    public ServerConnectionManager ConnectionManager;
    private InternetGameplayController _controller;

    private float _playerTwoStartingRotation = (float)Math.PI;
    private float _playerOneStartingRotation;

    public Auth Auth => GetNode<Auth>("/root/Auth");

    public  Action GameLost;
    public  Action GameWon;
    

// public void Init(OverlayManager overlayManager)
//     {
//         _controller = new InternetGameplayController(this);
//         // _overlayManager = overlayManager;
//         _playerTwoGameboard = ResourceLoader.Load<PackedScene>(ResourcePaths.GameboardNodePath).Instantiate() as Gameboard;
//         _playerOneGameboard = ResourceLoader.Load<PackedScene>(ResourcePaths.GameboardNodePath).Instantiate() as Gameboard;
//         _gameOverlay = ResourceLoader.Load<PackedScene>(ResourcePaths.GameOverlayNodePath).Instantiate() as  GameOverlay;
//         
//         _playerOneGameboard.IsControlledLocally = true;
//         _playerTwoGameboard.IsControlledLocally = false;
//         _playerOneGameboard.Init(null);
//         _playerTwoGameboard.Init(null);
//
//         // _playerOneGameboard.LocalUpdateMade += _multiplayerGameManager.LocalUpdateHandler;
//         _playerOneGameboard.LocalUpdateMade = _controller.HandleLocalUpdate;
//        _controller.LocalUIUpdated  += _playerOneGameboard.UIUpdatedHandler;
//        _controller.OpponentUIUpdated  += _playerTwoGameboard.UIUpdatedHandler;
//         // _multiplayerGameManager.LocalUIUpdated += _playerOneGameboard.UIUpdatedHandler;
//         // _multiplayerGameManager.OpponentUIUpdated += _playerTwoGameboard.UIUpdatedHandler;
//         
//         _playerOneGameboard.OnRotate += () =>
//         {
//             var t = GetTree().CreateTween().SetParallel();
//             t.TweenProperty(_playerOneGameboard, "rotation", (float) Math.PI * -1, 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
//             
//             t.TweenProperty(_playerTwoGameboard, "rotation", 0, 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
//             t.Play();
//         };
//         _playerTwoGameboard.OnRotate += () =>
//         {
//             var t = GetTree().CreateTween().SetParallel();
//             t.TweenProperty(_playerTwoGameboard, "rotation", (float) Math.PI, 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
//             t.TweenProperty(_playerOneGameboard, "rotation", 0, 0.5f).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
//             t.Play();
//         };
//     }

    public override void _ExitTree()
    {
        // _playerOneGameboard.LocalUpdateMade += _multiplayerGameManager.LocalUpdateHandler;
        // _multiplayerGameManager.LocalUIUpdated += _playerOneGameboard.UIUpdatedHandler;
        // _multiplayerGameManager.OpponentUIUpdated += _playerTwoGameboard.UIUpdatedHandler;
    }

    public override void _Ready()
    {
        ConnectionManager = AppRoot.Services.RetrieveService<ServerConnectionManager>();
        _controller = new InternetGameplayController(this);
        _playerTwoGameboard = ResourceLoader.Load<PackedScene>(ResourcePaths.GameboardNodePath).Instantiate() as Gameboard;
         _playerOneGameboard = ResourceLoader.Load<PackedScene>(ResourcePaths.GameboardNodePath).Instantiate() as Gameboard;
         _gameOverlay = AppRoot.Services.RetrieveService<NodeGetter>().Get<GameOverlay>() as GameOverlay;
         
         _playerOneGameboard.IsControlledLocally = true;
         _playerTwoGameboard.IsControlledLocally = false;
         _playerOneGameboard.Init(null);
         _playerTwoGameboard.Init(null);

         // _playerOneGameboard.LocalUpdateMade += _multiplayerGameManager.LocalUpdateHandler;
         _playerOneGameboard.LocalUpdateMade = _controller.HandleLocalUpdate;
        _controller.LocalUIUpdated  += _playerOneGameboard.UIUpdatedHandler;
        _controller.OpponentUIUpdated  += _playerTwoGameboard.UIUpdatedHandler;
        _controller.ReceivedServerTick += _gameOverlay.OnServerTick;
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
        _playerTwoGameboard.GameboardTitle = "Opponent's Board";
        _playerOneGameboard.GameboardTitle = "Your Board";
        _playerTwoGameboard.InitialRotation = (float)Math.PI;
        
        AddChild(_playerTwoGameboard);
        AddChild(_playerOneGameboard);
        _controller.TransitionTo(new TurnDeciderGuessingState(_controller));
        // _overlayManager.Add("game", _gameOverlay, 5);
    }

    public void SetPlayerTurn(bool isPlayerTurn)
    {
        Logger.Print($"setting player turn {isPlayerTurn}");
        if (isPlayerTurn)
            _gameOverlay.SetPlayerTurn();
        else
            _gameOverlay.SetOpponentTurn();    
        _playerOneGameboard.SetGameBoardTurn(isPlayerTurn);
    }

    // public Result ReceiveSharedNodes(Node node)
    // {
    //     ConnectionManager = GodotNodeTree.FindFirstNodeOfType<ServerConnectionManager>(node);
    //     var allReceived = false;
    //     
    //     if (ConnectionManager != null) // add more null checks if new nodes that should be received from another scene are added here
    //     {
    //         node.RemoveChild(ConnectionManager);
    //         AddChild(ConnectionManager);
    //         allReceived = true;
    //     }
    //
    //     return allReceived ? Result.Ok() : Result.Fail("did not receive shared nodes");
    // }
}
// type TimeUpdateMessage struct {
// TypeDiscriminator string `json:"$type"`
// TimeLeft          uint   `json:"time_left"`
// }

public class TimeUpdate() : UIUpdate
{
    [JsonPropertyName("time_left")]
    public uint TimeLeft { get; set; }
}