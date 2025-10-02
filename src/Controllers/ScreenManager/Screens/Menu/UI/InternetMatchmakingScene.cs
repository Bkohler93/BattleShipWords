using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Services.SharedData;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.Menu.UI;

public class InternetMatchmakingScene : IScene
{
    private readonly MenuScreen _menuScreen;
    private InternetMatchmaking _node;
    private PackedScene _packedScene;
    public const string ResourcePath = ResourcePaths.InternetMatchmakingMenuNodePath;

    public InternetMatchmakingScene(MenuScreen menuScreen)
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
        
        var matchmaking = _packedScene.Instantiate() as InternetMatchmaking;
        matchmaking!.OnCancelButtonPressed = () =>
        {
            _menuScreen.ChangeMenu(MenuNodeType.InternetMatchmaking, MenuNodeType.MultiplayerMenu, SlideTransitionDirection.Backward);
        };
        matchmaking.OnStartSetup = (msg) =>
        {
            AppRoot.Services.RetrieveService<SharedData>().Set(msg);
            _menuScreen.StartInternetGame();
        };

        _node = matchmaking;
        return _node;
    }

    public override Control GetNode()
    {
        return _node;
    }
}