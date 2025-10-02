using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.ScreenManager.Screens.InternetGame;
using BattleshipWithWords.Controllers.ScreenManager.Screens.Menu.UI;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.Menu;

public enum MenuNodeType
{
    MainMenu,
    MultiplayerMenu,
    Settings,
    LocalMatchmaking,
    InternetMatchmaking
}

public class MenuScreen :Screen<MenuNodeType>
{
    private ScreenManager _screenManager;
    // private IScene _currentMenuScene;

    public MenuScreen(ScreenManager screenManager)
    {
        _screenManager = screenManager;
    }

    public override List<ScreenNode> Initialize()
    {
        List<ScreenNode> nodes = new List<ScreenNode>();
        foreach (var (menuType, scene) in _scenes)
        {
            // _initializedNodes.Add(menuType, screenNode);
            if (menuType != MenuNodeType.MainMenu) continue;
            // var screenNode = new ScreenNode(scene.Initialize(), ScreenLayer.UI);
            nodes.Add(new ScreenNode(scene.Initialize(), ScreenLayer.UI));
        }

        return nodes;
    }


    // public void Enter(Tween tween, TransitionDirection direction)
    // {
    //     SceneTransitions.MenuEnter(_initializedNodes[MenuNodeType.MainMenu].Node, tween, direction);
    //     // _currentMenuScene.Enter(tween, direction);
    // }
    

    public override void Load()
    {
        var mainMenuScene = new MainMenuScene(this);
        var internetMatchmakingScene = new InternetMatchmakingScene(this);
        var localMatchmakingScene = new LocalMatchmakingScene(this);
        var multiplayerMenuScene = new MultiplayerMenuScene(this);
        var settingsMenuScene = new SettingsScene(this);
        
        _resourcePaths.Add(MainMenuScene.ResourcePath);
        _resourcePaths.Add(InternetMatchmakingScene.ResourcePath);
        _resourcePaths.Add(LocalMatchmakingScene.ResourcePath);
        _resourcePaths.Add(MultiplayerMenuScene.ResourcePath);
        _resourcePaths.Add(SettingsScene.ResourcePath);
        
        _scenes.Add(MenuNodeType.MainMenu, mainMenuScene);
        _scenes.Add(MenuNodeType.InternetMatchmaking, internetMatchmakingScene);
        _scenes.Add(MenuNodeType.LocalMatchmaking, localMatchmakingScene);
        _scenes.Add(MenuNodeType.MultiplayerMenu, multiplayerMenuScene);
        _scenes.Add(MenuNodeType.Settings, settingsMenuScene);

        foreach (var resourcePath in _resourcePaths)
        {
            Logger.Print($"starting to load {resourcePath}");
            var error = ResourceLoader.LoadThreadedRequest(resourcePath);
            if (error != Error.Ok)
            {
                Logger.Print($"error loading {resourcePath} - {error}");
            }
        }
    }

    public void ChangeMenu(MenuNodeType from, MenuNodeType to, SlideTransitionDirection direction)
    {
        _screenManager.SlideNodesTransition(
            [new LayerNode(ScreenLayer.UI, _scenes[from].GetNode())],
            [new LayerNode(ScreenLayer.UI,_scenes[to].Initialize())], 
            direction); 
    }

    public void QuitGame()
    {
        _screenManager.QuitGame();
    }

    public void StartInternetGame()
    {
        _screenManager.StartTransitionToScreen(new InternetGameScreen(_screenManager), SlideTransitionDirection.Forward);
    }

    public void StartLocalGame(MultiplayerGameManager gameManager)
    {
        //TODO: transition to LocalGameScreen
        // _screenmanager.StartTransitionToScreen(new LocalGameScreen(_screenManager_, TransitionDirection.Forward);
    }
}