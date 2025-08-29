using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class InternetSetupScene : IScene
{
    private OverlayManager _overlayManager;
    private SceneManager _sceneManager;

    private StartSetup _startSetupData;
    // private MultiplayerGameManager _gameManager;
    private InternetSetup _internetSetupNode;
    private bool _isWaiting; 
    
    private Action _bothSetupsCompletedHandler;

    public InternetSetupScene(StartSetup msg, SceneManager sceneManager, OverlayManager overlayManager)
    {
        _overlayManager = overlayManager;
        _sceneManager = sceneManager;
        _startSetupData = msg;
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        _overlayManager.RemoveAll();
        SceneTransitions.MenuExit(_internetSetupNode, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_internetSetupNode, tween, direction);
        var pauseOverlay = new PauseOverlay();
        pauseOverlay.ExitButtonPressed += () =>
        {
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
        _internetSetupNode = ResourceLoader.Load<PackedScene>(ResourcePaths.InternetSetupNodePath).Instantiate() as InternetSetup;
        _internetSetupNode.Init(_startSetupData, _overlayManager);
        // _multiplayerSetupNode.Init(_gameManager);

        _internetSetupNode.OnSetupComplete = () =>
        {
            Logger.Print("Internet Setup Complete. Transition into TurnDecider");
            _sceneManager.TransitionTo(new InternetTurnDeciderScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        // _gameManager.SetupUpdated = (update) =>
        // {
        //     switch (update)
        //     {
        //         case SetupSceneUpdate.WaitingForOtherPlayer:
        //             _isWaiting = true;
        //             _overlayManager.Add("waiting", new WaitingOverlay(), 2);
        //             break;
        //         case SetupSceneUpdate.SetupComplete:
        //         {
        //             if (_isWaiting)
        //                 _overlayManager.Remove("waiting");
        //             _sceneManager.TransitionTo(new MultiplayerGameScene(_sceneManager, _overlayManager, _gameManager),
        //                 TransitionDirection.Forward);
        //             break;
        //         }
        //     }
        // };
        
        return _internetSetupNode;
    }

    public Node GetNode()
    {
        return _internetSetupNode;
    }
}