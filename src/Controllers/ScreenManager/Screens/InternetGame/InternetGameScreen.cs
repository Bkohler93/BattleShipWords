using System.Collections.Generic;
using BattleshipWithWords.Controllers.ScreenManager.Screens.InternetGame.Game;
using BattleshipWithWords.Controllers.ScreenManager.Screens.Menu;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.InternetGame;

public enum InternetGameNodeType
{
    Setup,
    Gameplay,
    GameOverlay,
    Pause,
    WaitingOverlay,
    WinOverlay,
    LossOverlay
}

public class InternetGameScreen :Screen<InternetGameNodeType>
{
    private ScreenManager _screenManager;

    public InternetGameScreen(ScreenManager screenManager)
    {
        _screenManager = screenManager;
    }

    public override List<ScreenNode> Initialize()
    {
        List<ScreenNode> nodes = new List<ScreenNode>();
        foreach (var (menuType, scene) in _scenes)
        {
            switch (menuType)
            {
                case InternetGameNodeType.Setup:
                    nodes.Add(new ScreenNode(scene.Initialize(), ScreenLayer.Game));
                    break;
                case InternetGameNodeType.GameOverlay:
                    nodes.Add(new ScreenNode(scene.Initialize(), ScreenLayer.UI));
                    break;
            }
        }
        return nodes;
    }

    public override void Load()
    {
        var internetSetupScene = new InternetSetupScene(this);
        var internetGameplayScene = new InternetGameplayScene(this);
        var gameOverlayScene = new GameOverlayScene(this);
        var pauseMenuScene = new PausePopupScene(this);
        var waitingOverlayScene = new WaitingPopupScene(this);
        var winPopupScene = new WinPopupScene(this);
        var lossPopupScene = new LossPopupScene(this);
        
        _resourcePaths.Add(InternetSetupScene.ResourcePath);
        _resourcePaths.Add(InternetGameplayScene.ResourcePath);
        _resourcePaths.Add(GameOverlayScene.ResourcePath);
        _resourcePaths.Add(PausePopupScene.ResourcePath);
        _resourcePaths.Add(WaitingPopupScene.ResourcePath);
        _resourcePaths.Add(WinPopupScene.ResourcePath);
        _resourcePaths.Add(LossPopupScene.ResourcePath);
        
        _scenes.Add(InternetGameNodeType.Setup, internetSetupScene);
        _scenes.Add(InternetGameNodeType.Gameplay, internetGameplayScene);
        _scenes.Add(InternetGameNodeType.GameOverlay, gameOverlayScene);
        _scenes.Add(InternetGameNodeType.Pause, pauseMenuScene);
        _scenes.Add(InternetGameNodeType.WaitingOverlay, waitingOverlayScene);
        _scenes.Add(InternetGameNodeType.WinOverlay, winPopupScene);
        _scenes.Add(InternetGameNodeType.LossOverlay, lossPopupScene);
        
        foreach (var resourcePath in _resourcePaths)
        {
            var error = ResourceLoader.LoadThreadedRequest(resourcePath);
            if (error != Error.Ok)
            {
                Logger.Print($"error loading {resourcePath} - {error}");
            }
        }
    }

    public void QuitGame()
    {
        AppRoot.Services.StopService<ServerConnectionManager>();
        _screenManager.StartTransitionToScreen(new MenuScreen(_screenManager), SlideTransitionDirection.Backward);
    }

    public void StartInternetGame()
    {
        _screenManager.RemoveNodes([new LayerNode(ScreenLayer.Popup, _scenes[InternetGameNodeType.WaitingOverlay].GetNode())]);
        _screenManager.SlideNodesTransition(
            [new LayerNode(ScreenLayer.Game, _scenes[InternetGameNodeType.Setup].GetNode())],
            [new LayerNode(ScreenLayer.Game, _scenes[InternetGameNodeType.Gameplay].Initialize())],
            SlideTransitionDirection.Forward);
    }

    public void ShowPauseMenu()
    {
        
        _screenManager.FadeNodesIn([
            new LayerNode(ScreenLayer.Popup, _scenes[InternetGameNodeType.Pause].Initialize())
        ]);
    }

    public void HidePauseMenu()
    {
        _screenManager.RemoveNodes([new LayerNode(ScreenLayer.Popup, _scenes[InternetGameNodeType.Pause].GetNode())]);
    }

    public void WaitForOpponentSetup()
    {
        _screenManager.FadeNodesIn([
            new LayerNode(ScreenLayer.Popup, _scenes[InternetGameNodeType.WaitingOverlay].Initialize())
        ]);
    }

    public void ShowWinPopup()
    {
        _screenManager.FadeNodesIn([
            new LayerNode(ScreenLayer.Popup, _scenes[InternetGameNodeType.WinOverlay].Initialize())
        ]);
    }
    public void ShowLossPopup()
    {
        _screenManager.FadeNodesIn([
            new LayerNode(ScreenLayer.Popup, _scenes[InternetGameNodeType.LossOverlay].Initialize())
        ]);
    }
}