using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class InternetMatchmakingScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public InternetMatchmakingScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        // throw new System.NotImplementedException();
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        throw new System.NotImplementedException();
    }

    public Node Create()
    {
        var matchmaking = ResourceLoader.Load<PackedScene>("res://scenes/menus/internet_matchmaking.tscn").Instantiate() as InternetMatchmaking;
        matchmaking!.OnCancelButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager), TransitionDirection.Backward);
        };
            
        return matchmaking;
    }
}