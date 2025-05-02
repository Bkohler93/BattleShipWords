using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.GameManager;
using Godot;

namespace BattleshipWithWords.Services.Scenes;

public class MultiplayerTurnDeciderScene : IScene
{
    private MultiplayerGameManager _gameManager;
    private OverlayManager _overlayManager;
    private SceneManager _sceneManager;
    private MultiplayerTurnDecider _node;

    public MultiplayerTurnDeciderScene(MultiplayerGameManager gameManager, SceneManager sceneManager, OverlayManager overlayManager)
    {
        _gameManager = gameManager;
        _overlayManager = overlayManager;
        _sceneManager = sceneManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_node, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_node, tween, direction);
        //TODO show Pause display
    }

    public Node Create()
    {
        throw new System.NotImplementedException();
    }
}