using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class TutorialScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;
    private Tutorial _tutorial;

    public TutorialScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager; 
        _uiManager = uiManager;
    }
    
    public void Exit(Tween tween, TransitionDirection direction)
    {
    }

    void IScene.Enter(Tween tween, TransitionDirection direction)
    {
        throw new System.NotImplementedException();
    }

    public Node Create()
    {
        var tutorial = ResourceLoader.Load<PackedScene>("res://scenes/games/tutorial.tscn").Instantiate() as Tutorial;
        tutorial!.OnFinish = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager), TransitionDirection.Forward);
        };
        return tutorial;
    }
}