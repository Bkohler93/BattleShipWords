using System.Collections.Generic;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;
using Godot.NativeInterop;

namespace BattleshipWithWords.Controllers.SceneManager;

public class MainMenuScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private MainMenu _mainMenu;

    public MainMenuScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_mainMenu, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
       SceneTransitions.MenuEnter(_mainMenu, tween, direction); 
    }

    public Node Create()
    {
        var mainMenu = ResourceLoader.Load<PackedScene>(ResourcePaths.MainMenuNodePath).Instantiate() as MainMenu;
        mainMenu!.OnSinglePlayerButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new SinglePlayerGameScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        mainMenu.OnMultiplayerButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        mainMenu.OnSettingsButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new SettingsScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        mainMenu.OnQuitButtonPressed = () =>
        {
            _sceneManager.QuitGame();
        };
        _mainMenu = mainMenu;
        return mainMenu;
    }

    public Node GetNode()
    {
        return _mainMenu;
    }
}