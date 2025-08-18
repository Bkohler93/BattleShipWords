using System.Collections.Generic;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class MultiplayerMenuScene: IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private MultiplayerMenu _multiplayerMenu;

    public MultiplayerMenuScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_multiplayerMenu, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_multiplayerMenu, tween, direction);
    }

    public Node Create()
    {
        var multiplayerMenu = ResourceLoader.Load<PackedScene>(ResourcePaths.MultiplayerMenuNodePath).Instantiate() as MultiplayerMenu;
        multiplayerMenu!.OnBackButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _overlayManager), TransitionDirection.Backward);
        };
        multiplayerMenu.OnLocalButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new LocalMatchmakingScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        multiplayerMenu.OnOnlineButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new InternetMatchmakingScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        _multiplayerMenu = multiplayerMenu;
        return multiplayerMenu;
    }

    public Node GetNode()
    {
        return _multiplayerMenu;
    }
}