using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.ConnectionManager;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class InternetMatchmakingScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private InternetMatchmaking _node;

    public InternetMatchmakingScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_node, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_node, tween, direction);
    }

    public Node Create()
    {
        var matchmaking = ResourceLoader.Load<PackedScene>(ResourcePaths.InternetMatchmakingMenuNodePath).Instantiate() as InternetMatchmaking;
        matchmaking!.OnCancelButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _overlayManager), TransitionDirection.Backward);
        };
        matchmaking!.OnStartSetup = (StartSetup msg) =>
        {
            _sceneManager.TransitionTo(new InternetSetupScene(msg, _sceneManager, _overlayManager), TransitionDirection.Forward);
        };

        _node = matchmaking;
        return matchmaking;
    }

    public Node GetNode()
    {
        return _node;
    }
}