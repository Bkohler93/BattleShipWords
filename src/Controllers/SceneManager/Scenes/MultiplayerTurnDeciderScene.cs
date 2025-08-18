using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

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

    public void Teardown()
    {
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

    public List<Node> GetChildNodesToTransfer()
    {
        return _node.GetNodesToShare();
    }

    public void AddSharedNode(Node node)
    {
        _node.AddNodeToShare(node);
    }

    public Node GetNode()
    {
        return _node;
    }
}