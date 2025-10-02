using System.Collections.Generic;
using System.Runtime.InteropServices;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.ScreenManager;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.GameManager;
using BattleshipWithWords.Nodes.Menus;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.Menu.UI;

public class LocalMatchmakingScene : IScene
{
    private MenuScreen _menuScreen;
    private LocalMatchmaking _node;
    private PackedScene _packedScene;
    public const string ResourcePath = ResourcePaths.LocalMatchmakingMenuNodePath;

    public LocalMatchmakingScene(MenuScreen menuScreen)
    {
        _menuScreen = menuScreen;
    }

    public void Teardown()
    {
        _node.Shutdown();
    }

    public override Control Initialize()
    {
        if (_packedScene == null)
        {
            EnsureSceneLoaded(ResourcePath); 
            _packedScene = LoadScene(ResourcePath);            
        }
        var localMatchmaking = _packedScene.Instantiate() as LocalMatchmaking;
        localMatchmaking!.BackToMainMenu = () =>
        {
            _menuScreen.ChangeMenu(MenuNodeType.LocalMatchmaking, MenuNodeType.MultiplayerMenu, SlideTransitionDirection.Backward);
            // _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager), TransitionDirection.Backward);
        };
        localMatchmaking.StartGame = () =>
        {
            var gameManager = new MultiplayerGameManager(localMatchmaking.ConnectionManager);
            // _sceneManager.GetRoot().AddChild(gameManager);
            // gameManager.Init(); //TODO this logic was just moved into the constructor after removing Node dependency
            // _sceneManager.HookPeerDisconnected(gameManager);
            _menuScreen.StartLocalGame(gameManager);
            // _screenManager.TransitionTo(new MultiplayerSetupScreen(gameManager,_screenManager, _overlayManager), TransitionDirection.Forward);
        };
        _node = localMatchmaking;
        return _node;
    }

    public override Control GetNode()
    {
        return _node;
    }

}