using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.ScreenManager;
using BattleshipWithWords.Controllers.ScreenManager.Screens.Menu;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.InternetGame.Game;

public class WinPopupScene : IScene 
{
    private InternetGameScreen _internetGameScreen;
    private PackedScene _packedScene;
    private WinPopup _node;
    
    public const string ResourcePath = ResourcePaths.WinOverlayNodePath;
    
    public WinPopupScene(InternetGameScreen internetGameScreen) 
    {
        _internetGameScreen = internetGameScreen;
    }

    // public void Enter(Tween tween, TransitionDirection direction)
    // {
    //     SceneTransitions.MenuEnter(_gameOverlay, tween, direction);
    //     var pauseOverlay = new PauseOverlay();
    //     pauseOverlay.ExitButtonPressed += () =>
    //     {
    //         _overlayManager.RemoveAll();
    //         // _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager),
    //         //     TransitionDirection.Backward);
    //     };
    //     pauseOverlay.ContinueButtonPressed += () => _overlayManager.ShowAllBut("pause");
    //     pauseOverlay.PauseButtonPressed += () => _overlayManager.HideAllBut("pause");
    //     _overlayManager.AddAfterTransition("pause", pauseOverlay, 10);
    // }

    public override Control Initialize()
    {
        if (_packedScene == null)
        {
            EnsureSceneLoaded(ResourcePath); 
            _packedScene = LoadScene(ResourcePath);            
        }
        
        _node = _packedScene.Instantiate() as WinPopup;
        _node.OnQuitPressed += () =>
        {
            _internetGameScreen.QuitGame();
        };

        return _node;
    }

    public override Control GetNode()
    {
        return _node;
    }
}