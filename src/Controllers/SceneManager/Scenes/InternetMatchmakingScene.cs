using System.Collections.Generic;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers;

public class InternetMatchmakingScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private InternetMatchmaking _node;

    public InternetMatchmakingScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        // throw new System.NotImplementedException();
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        throw new System.NotImplementedException();
    }

    public Node Create()
    {
        var matchmaking = ResourceLoader.Load<PackedScene>("res://scenes/menus/internet_matchmaking.tscn").Instantiate() as InternetMatchmaking;
        matchmaking!.OnCancelButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _overlayManager), TransitionDirection.Backward);
        };

        _node = matchmaking;
        return matchmaking;
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