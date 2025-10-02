using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.ScreenManager;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class MultiplayerSetupScreen : IScene 
{
    // private OverlayManager _overlayManager;
    private ScreenManager.ScreenManager _screenManager;
    private MultiplayerGameManager _gameManager;
    private MultiplayerSetup _multiplayerSetupNode;
    private bool _isWaiting; 
    
    private Action _bothSetupsCompletedHandler;

    public MultiplayerSetupScreen(MultiplayerGameManager gameManager, ScreenManager.ScreenManager screenManager)
    {
        // _overlayManager = overlayManager;
        _screenManager = screenManager;
        _gameManager = gameManager;
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, SlideTransitionDirection direction)
    {
        // _overlayManager.RemoveAll();
        SceneTransitions.SlideExit(_multiplayerSetupNode, tween, direction);
    }

    // public void Enter(Tween tween, SlideTransitionDirection direction)
    // {
    //     SceneTransitions.SlideEnter(_multiplayerSetupNode, tween, direction);
    //     var pauseOverlay = new PauseOverlay();
    //     pauseOverlay.ExitButtonPressed += () =>
    //     {
    //         _gameManager.DisconnectAndFree();
    //         _overlayManager.RemoveAll();
    //         // _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager),
    //         //     TransitionDirection.Backward);
    //     };
    //     pauseOverlay.ContinueButtonPressed += () => _overlayManager.ShowAllBut("pause");
    //     pauseOverlay.PauseButtonPressed += () => _overlayManager.HideAllBut("pause");
    //     _overlayManager.AddAfterTransition("pause", pauseOverlay, 10);
    // }

    public Node Create()
    {
        _multiplayerSetupNode = ResourceLoader.Load<PackedScene>(ResourcePaths.MultiplayerSetupNodePath).Instantiate() as MultiplayerSetup;
        _multiplayerSetupNode.Init(_gameManager);

        _gameManager.SetupUpdated = (update) =>
        {
            switch (update)
            {
                case SetupSceneUpdate.WaitingForOtherPlayer:
                    _isWaiting = true;
                    // _overlayManager.Add("waiting", new WaitingOverlay(), 2);
                    break;
                case SetupSceneUpdate.SetupComplete:
                {
                    // if (_isWaiting)
                    //     _overlayManager.Remove("waiting");
                    // _screenManager.TransitionTo(new MultiplayerGameScreen(_screenManager, _overlayManager, _gameManager),
                    //     TransitionDirection.Forward);
                    break;
                }
            }
        };
        
        // _bothSetupsCompletedHandler = () =>
        // {
        //     _gameManager.BothSetupsCompleted -= _bothSetupsCompletedHandler;
        //     _overlayManager.Remove("waiting");
        //     _sceneManager.TransitionTo(new MultiplayerGameScene(_sceneManager, _overlayManager, _gameManager),
        //         TransitionDirection.Forward);
        //     GD.Print($"MultiplayerSetupScene: transitioned to MultiplayerGameScene"); 
        // };
        // _gameManager.BothSetupsCompleted += _bothSetupsCompletedHandler;
        // _multiplayerSetupNode.LocalSetupComplete += () =>
        // {
        // };
        return _multiplayerSetupNode;
    }

    public override Control Initialize()
    {
        throw new NotImplementedException();
    }

    public override Control GetNode()
    {
        return _multiplayerSetupNode;
    }
}