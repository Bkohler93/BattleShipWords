using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.ScreenManager;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class MultiplayerGameScreen :IScene 
{
    private ScreenManager.ScreenManager _screenManager;
    // private OverlayManager _overlayManager;
    private MultiplayerGameManager _gameManager;
    private MultiplayerGame _multiplayerGame;

    public MultiplayerGameScreen(ScreenManager.ScreenManager screenManager, MultiplayerGameManager gameManager)
    {
        _screenManager = screenManager;
        // _overlayManager = overlayManager;
        _gameManager = gameManager; 
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, SlideTransitionDirection direction)
    {
        SceneTransitions.SlideExit(_multiplayerGame, tween, direction);
    }

    // public void Enter(Tween tween, SlideTransitionDirection direction)
    // {
    //     SceneTransitions.SlideEnter(_multiplayerGame, tween, direction);
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
        var multiplayerGameNode = ResourceLoader.Load<PackedScene>(ResourcePaths.MultiplayerGameNodePath).Instantiate() as MultiplayerGame;
        _multiplayerGame = multiplayerGameNode;
        _multiplayerGame.Init(_gameManager);
        _gameManager.GameLost += ()=>
        {
            // var loseOverlay = new LoseOverlay();
            // loseOverlay.QuitButtonPressed += () =>
            // {
            //     _gameManager.DisconnectAndFree();
            //     _overlayManager.RemoveAll();
            //     // _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager),
            //     //     TransitionDirection.Backward);
            // };
            // _overlayManager.Add("lose", loseOverlay, 5);
        };
        _gameManager.GameWon += ()=>
        {
            // var winOverlay = new WinOverlay();
            // winOverlay.QuitButtonPressed += () =>
            // {
            //     _gameManager.DisconnectAndFree();
            //     _overlayManager.RemoveAll();
            //     // _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager),
            //     //     TransitionDirection.Backward);
            // };
            // _overlayManager.Add("win", winOverlay, 5);
        };
        return multiplayerGameNode;
    }

    public override Control Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override Control GetNode()
    {
        return _multiplayerGame;
    }
}