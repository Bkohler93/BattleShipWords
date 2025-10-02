using System.Threading;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;
using Godot.NativeInterop;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.Menu.UI;

public class MainMenuScene : IScene 
{
    private MenuScreen _menuScreen;
    private MainMenu _mainMenu;
    private PackedScene _packedScene;
    public const string ResourcePath = ResourcePaths.MainMenuNodePath;

    public MainMenuScene(MenuScreen menuScreen)
    {
        _menuScreen = menuScreen;
    }

    public override Control Initialize()
    {
        if (_packedScene == null)
        {
            EnsureSceneLoaded(ResourcePath); 
            _packedScene = LoadScene(ResourcePath);            
        }
        
        var mainMenu = _packedScene.Instantiate() as MainMenu; 
        
        // var mainMenu = _packedScene.Instantiate() as MainMenu;
        mainMenu!.OnSinglePlayerButtonPressed = () =>
        {
            // _screenManager.TransitionTo(new SinglePlayerGameScreen(_screenManager, _overlayManager), TransitionDirection.Forward);
        };
        mainMenu.OnMultiplayerButtonPressed = () =>
        {
            _menuScreen.ChangeMenu(MenuNodeType.MainMenu, MenuNodeType.MultiplayerMenu, SlideTransitionDirection.Forward);
            // _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager), TransitionDirection.Forward);
        };
        mainMenu.OnSettingsButtonPressed = () =>
        {
            _menuScreen.ChangeMenu(MenuNodeType.MainMenu, MenuNodeType.Settings, SlideTransitionDirection.Forward);
            // _screenManager.TransitionTo(new SettingsScreen(_screenManager, _overlayManager), TransitionDirection.Forward);
        };
        mainMenu.OnQuitButtonPressed = () =>
        {
            _menuScreen.QuitGame();
        };
        
        _mainMenu = mainMenu;
        return _mainMenu;
    }

    public override Control GetNode()
    {
        return _mainMenu;
    }
}