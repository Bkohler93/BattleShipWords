using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class MultiplayerMenuScene: IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;
    private MultiplayerMenu _multiplayerMenu;

    public MultiplayerMenuScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_multiplayerMenu, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_multiplayerMenu, tween, direction);
    }

    public Node Create()
    {
        var multiplayerMenu = ResourceLoader.Load<PackedScene>("res://scenes/menus/multiplayer.tscn").Instantiate() as MultiplayerMenu;
        multiplayerMenu!.OnBackButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager), TransitionDirection.Backward);
        };
        multiplayerMenu.OnLocalButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new LocalMatchmakingScene(_sceneManager, _uiManager), TransitionDirection.Forward);
        };
        _multiplayerMenu = multiplayerMenu;
        return multiplayerMenu;
    }
}