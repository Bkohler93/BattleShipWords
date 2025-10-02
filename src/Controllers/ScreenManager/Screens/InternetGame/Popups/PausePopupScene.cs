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

public class PausePopupScene : IScene 
{
    private InternetGameScreen _internetGameScreen;
    private PackedScene _packedScene;
    private PauseMenu _pausePopup;
    
    public const string ResourcePath = ResourcePaths.PauseOverlayNodePath;
    
    public PausePopupScene(InternetGameScreen internetGameScreen) 
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
        
        _pausePopup = _packedScene.Instantiate() as PauseMenu;
        _pausePopup!.OnContinueButtonPressedEventHandler += () => { _internetGameScreen.HidePauseMenu();};
        _pausePopup.OnQuitButtonPressedEventHandler += () => { _internetGameScreen.QuitGame(); };

        return _pausePopup;
    }

    public override Control GetNode()
    {
        return _pausePopup;
    }
}