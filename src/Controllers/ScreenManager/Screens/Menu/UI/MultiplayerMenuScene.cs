using System;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.Menu.UI;

public class MultiplayerMenuScene: IScene 
{
    private MenuScreen _menuScreen;
    private MultiplayerMenu _multiplayerMenu;
    private PackedScene _packedScene;
    public const string ResourcePath = ResourcePaths.MultiplayerMenuNodePath;

    public MultiplayerMenuScene(MenuScreen menuScreen)
    {
        _menuScreen = menuScreen;
        // _screenManager = screenManager;
        // _overlayManager = overlayManager;
    }

    public override Control Initialize()
    {
        if (_packedScene == null)
        {
            EnsureSceneLoaded(ResourcePath); 
            _packedScene = LoadScene(ResourcePath);            
        }
        var multiplayerMenu = _packedScene.Instantiate() as MultiplayerMenu;
        multiplayerMenu!.OnBackButtonPressed = () =>
        {
            _menuScreen.ChangeMenu(MenuNodeType.MultiplayerMenu, MenuNodeType.MainMenu, SlideTransitionDirection.Backward);
            // _screenManager.TransitionTo(new MainMenuScene(_screenManager, _overlayManager), TransitionDirection.Backward);
        };
        multiplayerMenu.OnLocalButtonPressed = () =>
        {
            _menuScreen.ChangeMenu(MenuNodeType.MultiplayerMenu, MenuNodeType.LocalMatchmaking, SlideTransitionDirection.Forward);
            // _screenManager.TransitionTo(new LocalMatchmakingScreen(_screenManager, _overlayManager), TransitionDirection.Forward);
        };
        multiplayerMenu.OnOnlineButtonPressed = () =>
        {
            _menuScreen.ChangeMenu(MenuNodeType.MultiplayerMenu, MenuNodeType.InternetMatchmaking, SlideTransitionDirection.Forward);
            // _screenManager.TransitionTo(new InternetMatchmakingScreen(_screenManager, _overlayManager), TransitionDirection.Forward);
            
        };
        _multiplayerMenu = multiplayerMenu;
        return _multiplayerMenu;
    }

    public override Control GetNode()
    {
        return _multiplayerMenu;
    }

    // public Node Create()
    // {
    //     var multiplayerMenu = ResourceLoader.Load<PackedScene>(ResourcePaths.MultiplayerMenuNodePath).Instantiate() as MultiplayerMenu;
    //     multiplayerMenu!.OnBackButtonPressed = () =>
    //     {
    //         _screenManager.TransitionTo(new MainMenuScene(_screenManager, _overlayManager), TransitionDirection.Backward);
    //     };
    //     multiplayerMenu.OnLocalButtonPressed = () =>
    //     {
    //         _screenManager.TransitionTo(new LocalMatchmakingScreen(_screenManager, _overlayManager), TransitionDirection.Forward);
    //     };
    //     multiplayerMenu.OnOnlineButtonPressed = () =>
    //     {
    //         _screenManager.TransitionTo(new InternetMatchmakingScreen(_screenManager, _overlayManager), TransitionDirection.Forward);
    //     };
    //     _multiplayerMenu = multiplayerMenu;
    //     return multiplayerMenu;
    // }

    // public Node GetNode()
    // {
    //     return _multiplayerMenu;
    // }
}