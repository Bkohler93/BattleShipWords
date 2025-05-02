using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class TutorialScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private Tutorial _tutorial;

    public TutorialScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager; 
        _overlayManager = overlayManager;
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
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        return tutorial;
    }
}