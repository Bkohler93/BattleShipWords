using BattleshipWithWords.Controllers.SceneManager;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers;

public class AppManager
{
    private SceneManager.SceneManager _sceneManager;
    private OverlayManager _overlayManager;

    public AppManager(SceneManager.SceneManager sceneManager, OverlayManager overlayManager)
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
        _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _overlayManager), TransitionDirection.Forward);

        // var gameManager = new MultiplayerGameManager(1, 1, _sceneManager.GetRoot());
        // _sceneManager.TransitionTo(new MultiplayerSetupScene(gameManager, _sceneManager, _overlayManager), TransitionDirection.Forward);
    }
}