using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class InternetMatchmakingScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;

    public InternetMatchmakingScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
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
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _overlayManager), TransitionDirection.Backward);
        };
            
        return matchmaking;
    }
}