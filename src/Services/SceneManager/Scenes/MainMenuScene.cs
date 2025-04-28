using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class MainMenuScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;
    private MainMenu _mainMenu;

    public MainMenuScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_mainMenu, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
       SceneTransitions.MenuEnter(_mainMenu, tween, direction); 
    }

    public Node Create()
    {
        var mainMenu = ResourceLoader.Load<PackedScene>("res://scenes/menus/main_menu.tscn").Instantiate() as MainMenu;
        mainMenu!.OnSinglePlayerButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new SinglePlayerGameScene(_sceneManager, _uiManager), TransitionDirection.Forward);
        };
        mainMenu.OnMultiplayerButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _uiManager), TransitionDirection.Forward);
        };
        mainMenu.OnSettingsButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new SettingsScene(_sceneManager, _uiManager), TransitionDirection.Forward);
        };
        mainMenu.OnQuitButtonPressed = () =>
        {
            _sceneManager.QuitGame();
        };
        _mainMenu = mainMenu;
        return mainMenu;
    }
}