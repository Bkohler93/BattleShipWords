using System.Runtime.InteropServices;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class LocalMatchmakingScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;
    private LocalMatchmaking _node;

    public LocalMatchmakingScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
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
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _uiManager), TransitionDirection.Backward);
        };
        localMatchmaking.StartGame = () =>
        {
            _sceneManager.TransitionTo(new MultiplayerGameScene(_sceneManager, _uiManager), TransitionDirection.Forward);
        };
        _node = localMatchmaking;
        return localMatchmaking;
    }
}