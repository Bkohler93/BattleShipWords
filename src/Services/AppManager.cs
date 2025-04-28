using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.Scenes;

namespace BattleshipWithWords.Services;

public class AppManager
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public AppManager(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void PlayTutorial()
    {
        _sceneManager.TransitionTo(new TutorialScene(_sceneManager, _uiManager), TransitionDirection.Forward);
    }

    public void Start()
    {
        _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager), TransitionDirection.Forward);
    }
}