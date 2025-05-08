using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers;

public class SinglePlayerGameScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private SinglePlayerGame _singlePlayerGame;
    public SinglePlayerGameScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
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
        var singlePlayerGame = ResourceLoader.Load<PackedScene>("res://scenes/games/single_player_game.tscn").Instantiate() as SinglePlayerGame;
        singlePlayerGame!.OnPause = () =>
        {
        };
        singlePlayerGame.OnFinish = () =>
        {
        };
        _singlePlayerGame = singlePlayerGame;
        return singlePlayerGame;
    }
}