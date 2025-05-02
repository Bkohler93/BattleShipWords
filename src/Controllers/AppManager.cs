using System.Runtime.InteropServices;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.GameManager;
using BattleshipWithWords.Services.Scenes;
using Godot;

namespace BattleshipWithWords.Services;

public class AppManager
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;

    public AppManager(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void PlayTutorial()
    {
        _sceneManager.TransitionTo(new TutorialScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
    }

    public void Start()
    {
        GD.Print("app starting"); 
        // _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager), TransitionDirection.Forward);

        var gameManager = new MultiplayerGameManager(1, 1);
        _sceneManager.TransitionTo(new MultiplayerSetupScene(gameManager, _sceneManager, _overlayManager), TransitionDirection.Forward);
    }
}