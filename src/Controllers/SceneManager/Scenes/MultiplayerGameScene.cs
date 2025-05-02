using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class MultiplayerGameScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private Control _multiplayerGame;//TODO CREATE MULTIPLAYER GAME AND SET IT TO THAT

    public MultiplayerGameScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_multiplayerGame, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_multiplayerGame, tween, direction);
    }

    public Node Create()
    {
        var multiplayerGame = ResourceLoader.Load<PackedScene>("res://scenes/games/single_player_game.tscn").Instantiate() as SinglePlayerGame;
        multiplayerGame!.OnPause = () =>
        {
        };
        multiplayerGame.OnFinish = () =>
        {
        };
        _multiplayerGame = multiplayerGame;
        return multiplayerGame;
    }
}