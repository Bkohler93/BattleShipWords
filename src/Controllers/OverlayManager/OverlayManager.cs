using System.Collections.Generic;
using Godot;

namespace BattleshipWithWords.Services;

public class OverlayManager
{
    private Node _root;
    private readonly Stack<IOverlay> _overlays = new Stack<IOverlay>();

    public OverlayManager(Node root)
    {
        _root = root;
    }
    
    public bool HasOverlays => _overlays.Count > 0;

    public void Push(IOverlay overlay)
    {
        var node = overlay.GetNode();
        _root.AddChild(overlay.GetNode());
        node.Layer = _overlays.Count;
       _overlays.Push(overlay); 
    }

    public void Pop()
    {
        var overlay = _overlays.Pop();
        overlay.GetNode().QueueFree();
    }
}