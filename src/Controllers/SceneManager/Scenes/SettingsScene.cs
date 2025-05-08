using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers;

public class SettingsScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private Settings _settings;

    public SettingsScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        throw new System.NotImplementedException();
    }

    public Node Create()
    {
        var settings = ResourceLoader.Load<PackedScene>("res://scenes/menus/settings.tscn").Instantiate() as Settings;
        settings!.OnBackButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _overlayManager), TransitionDirection.Backward);
        };
        _settings = settings;
        return settings;
    }
}