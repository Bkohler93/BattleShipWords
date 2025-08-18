using System.Collections.Generic;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class SinglePlayerGameScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private SinglePlayerGame _singlePlayerGame;
    public SinglePlayerGameScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Teardown()
    {
        
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
    }

    public void Enter(Tween tween, TransitionDirection direction)
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

    public Node GetNode()
    {
        return _singlePlayerGame;
    }
}