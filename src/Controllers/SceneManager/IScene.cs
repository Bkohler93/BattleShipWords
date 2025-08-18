using System.Collections.Generic;
using androidplugintest.ConnectionManager;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public interface IScene
{
    public void Teardown();
    public void Exit(Tween tween, TransitionDirection direction);
    public void Enter(Tween tween, TransitionDirection direction);
    public Node Create();
    public Node GetNode();
}

public interface ISharedNodeReceiver
{
    public Result ReceiveSharedNodes(Node node);
}


// public interface ISceneNode
// {
//     public List<Node> GetNodesToShare();
//     public void AddNodeToShare(Node node);
// }