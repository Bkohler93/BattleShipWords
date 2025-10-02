using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.Menu.UI;

public class SettingsScene : IScene
{
    private MenuScreen _menuScreen;
    private Settings _settings;
    private PackedScene _packedScene;
    public const string ResourcePath = ResourcePaths.SettingsMenuNodePath;

    public SettingsScene(MenuScreen menuScreen)
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
        var settings = _packedScene.Instantiate() as Settings;
        settings!.OnBackButtonPressed = () =>
        {
            _menuScreen.ChangeMenu(MenuNodeType.Settings, MenuNodeType.MainMenu, SlideTransitionDirection.Backward);
            // _screenManager.TransitionTo(new MainMenuScene(_screenManager, _overlayManager), TransitionDirection.Backward);
        };
        _settings = settings;
        return _settings;
    }

    public override Control GetNode()
    {
        return _settings;
    }
}