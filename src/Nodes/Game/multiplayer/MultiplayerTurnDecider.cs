using System.Collections.Generic;
using BattleshipWithWords.Controllers;
using Godot;

public partial class MultiplayerTurnDecider : Control
{
    private List<Node> _nodesToKeepAlive = [];
    public List<Node> GetNodesToShare()
    {
        return _nodesToKeepAlive;
    }

    public void AddNodeToShare(Node node)
    {
        _nodesToKeepAlive.Add(node);
    }
}
