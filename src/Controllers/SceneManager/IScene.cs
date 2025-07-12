using System.Collections.Generic;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers;

public interface IScene
{
    public void Exit(Tween tween, TransitionDirection direction);
    public void Enter(Tween tween, TransitionDirection direction);
    public Node Create();
    public List<Node> GetChildNodesToTransfer();
    public void AddSharedNode(Node node);
    public Node GetNode();
}

public interface ISceneNode
{
    public List<Node> GetNodesToShare();
    public void AddNodeToShare(Node node);
}