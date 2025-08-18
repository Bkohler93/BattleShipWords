using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class MultiplayerGameScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private MultiplayerGameManager _gameManager;
    private MultiplayerGame _multiplayerGame;

    public MultiplayerGameScene(SceneManager sceneManager, OverlayManager overlayManager, MultiplayerGameManager gameManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
        _gameManager = gameManager; 
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_multiplayerGame, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_multiplayerGame, tween, direction);
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
        var multiplayerGameNode = ResourceLoader.Load<PackedScene>(ResourcePaths.MultiplayerGameNodePath).Instantiate() as MultiplayerGame;
        _multiplayerGame = multiplayerGameNode;
        _multiplayerGame.Init(_gameManager);
        _gameManager.GameLost += ()=>
        {
            var loseOverlay = new LoseOverlay();
            loseOverlay.QuitButtonPressed += () =>
            {
                _gameManager.DisconnectAndFree();
                _overlayManager.RemoveAll();
                _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager),
                    TransitionDirection.Backward);
            };
            _overlayManager.Add("lose", loseOverlay, 5);
        };
        _gameManager.GameWon += ()=>
        {
            var winOverlay = new WinOverlay();
            winOverlay.QuitButtonPressed += () =>
            {
                _gameManager.DisconnectAndFree();
                _overlayManager.RemoveAll();
                _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager),
                    TransitionDirection.Backward);
            };
            _overlayManager.Add("win", winOverlay, 5);
        };
        return multiplayerGameNode;
    }

    public Node GetNode()
    {
        return _multiplayerGame;
    }
}