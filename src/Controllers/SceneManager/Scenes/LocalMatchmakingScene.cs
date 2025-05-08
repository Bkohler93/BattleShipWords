using System.Runtime.InteropServices;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.GameManager;
using BattleshipWithWords.Nodes.Menus;
using Godot;

namespace BattleshipWithWords.Controllers;

public class LocalMatchmakingScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private LocalMatchmaking _node;

    public LocalMatchmakingScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Exit(Tween tween,  TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_node, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_node, tween, direction);
    }

    public Node Create()
    {
        var localMatchmaking = ResourceLoader.Load<PackedScene>("res://scenes/menus/local_matchmaking.tscn").Instantiate() as LocalMatchmaking;
        localMatchmaking.BackToMainMenu = () =>
        {
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager), TransitionDirection.Backward);
        };
        localMatchmaking.StartGame = () =>
        {
            var gameManager = new MultiplayerGameManager();
            _sceneManager.GetRoot().AddChild(gameManager);
            gameManager.Init();
            _sceneManager.HookPeerDisconnected(gameManager);
            _sceneManager.TransitionTo(new MultiplayerSetupScene(gameManager,_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        _node = localMatchmaking;
        return localMatchmaking;
    }
}