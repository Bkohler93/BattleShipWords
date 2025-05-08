using System;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers;

public class MultiplayerSetupScene : IScene
{
    private OverlayManager _overlayManager;
    private SceneManager _sceneManager;
    private MultiplayerGameManager _gameManager;
    private MultiplayerSetup _multiplayerSetupNode;
    private Action _setupCompleteEventHandler;

    public MultiplayerSetupScene(MultiplayerGameManager gameManager, SceneManager sceneManager, OverlayManager overlayManager)
    {
        _overlayManager = overlayManager;
        _sceneManager = sceneManager;
        _gameManager = gameManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        _overlayManager.RemoveAll();
        SceneTransitions.MenuExit(_multiplayerSetupNode, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_multiplayerSetupNode, tween, direction);
        var pauseOverlay = new PauseOverlay();
        pauseOverlay.ExitButtonPressed += () =>
        {
            _gameManager.DisconnectAndFree();
            _overlayManager.RemoveAll();
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager),
                TransitionDirection.Backward);
        };
        pauseOverlay.ContinueButtonPressed += () => _overlayManager.ShowAllBut("pause");
        pauseOverlay.PauseButtonPressed += () => _overlayManager.HideAllBut("pause");
        _overlayManager.AddAfterTransition("pause", pauseOverlay, 10);
    }

    public Node Create()
    {
        _multiplayerSetupNode = ResourceLoader.Load<PackedScene>("res://scenes/games/multiplayer/multiplayer_setup.tscn").Instantiate() as MultiplayerSetup;
        _multiplayerSetupNode.Init(_gameManager);
        _setupCompleteEventHandler = () =>
        {
            _gameManager.SetupComplete -= _setupCompleteEventHandler;
            _overlayManager.Remove("waiting");
            _sceneManager.TransitionTo(new MultiplayerGameScene(_sceneManager, _overlayManager, _gameManager),
                TransitionDirection.Forward);
            GD.Print($"MultiplayerSetupScene: transitioned to MultiplayerGameScene"); 
        };
        _gameManager.SetupComplete += _setupCompleteEventHandler;
        _multiplayerSetupNode.SetupCompleteCallback += (selectedWords, boardSelection) =>
        {
            _gameManager.CompleteLocalSetup(selectedWords, boardSelection);
            _overlayManager.Add("waiting", new WaitingOverlay(), 2);
        };
        return _multiplayerSetupNode;
    }
}