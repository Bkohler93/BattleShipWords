using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class SettingsScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;
    private Settings _settings;

    public SettingsScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
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
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager), TransitionDirection.Backward);
        };
        _settings = settings;
        return settings;
    }
}