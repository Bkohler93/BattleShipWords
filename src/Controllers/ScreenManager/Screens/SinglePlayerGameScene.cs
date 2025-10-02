using System.Collections.Generic;
using BattleshipWithWords.Controllers.ScreenManager;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class SinglePlayerGameScreen : IScene 
{
    private ScreenManager.ScreenManager _screenManager;
    // private OverlayManager _overlayManager;
    private SinglePlayerGame _singlePlayerGame;
    public SinglePlayerGameScreen(ScreenManager.ScreenManager screenManager )
    {
        _screenManager = screenManager;
        // _overlayManager = overlayManager;
    }

    public void Teardown()
    {
        
    }

    public void Exit(Tween tween, SlideTransitionDirection direction)
    {
    }

    public void Enter(Tween tween, SlideTransitionDirection direction)
    {
        throw new System.NotImplementedException();
    }

    public Node Create()
    {
        var singlePlayerGame = ResourceLoader.Load<PackedScene>(ResourcePaths.SinglePlayerGameNodePath).Instantiate() as SinglePlayerGame;
        singlePlayerGame!.OnPause = () =>
        {
        };
        singlePlayerGame.OnFinish = () =>
        {
        };
        _singlePlayerGame = singlePlayerGame;
        return singlePlayerGame;
    }

    public override Control Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override Control GetNode()
    {
        return _singlePlayerGame;
    }
}