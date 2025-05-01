using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class SinglePlayerGameScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;
    private SinglePlayerGame _singlePlayerGame;
    public SinglePlayerGameScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
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