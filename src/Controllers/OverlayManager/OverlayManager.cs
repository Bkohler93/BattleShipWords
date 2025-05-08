using System;
using System.Collections.Generic;
using Godot;

namespace BattleshipWithWords.Controllers;

public class OverlayManager
{
    private Node _root;
    // private readonly Stack<IOverlay> _overlays = new Stack<IOverlay>();
    private readonly Dictionary<string, IOverlay> _overlayDict = new Dictionary<string, IOverlay>();
    private SceneManager _sceneManager;

    public OverlayManager(Node root)
    {
        _root = root;
    }
    
    public bool HasOverlays => _overlayDict.Count > 0;

    public void Add(string name, IOverlay overlay, int layerIndex)
    {
        if (_overlayDict.ContainsKey(name))
            throw new Exception($"OverlayManager: tried to add already existing overlay - {name}");
        var node = overlay.GetNode();
        _root.AddChild(overlay.GetNode());
        _overlayDict[name] = overlay; 
       node.Layer = layerIndex;
    }
    
    public void AddAfterTransition(string name, IOverlay overlay, int layerIndex)
    {
        if (_overlayDict.ContainsKey(name))
            throw new Exception($"OverlayManager: tried to add already existing overlay - {name}");
        var node = overlay.GetNode();

        Action handler = null;
        handler =  () =>
        {
            _sceneManager.TransitionOverEventHandler -= handler;
            _root.AddChild(overlay.GetNode());
            _overlayDict[name] = overlay; 
            node.Layer = layerIndex;
        };
        _sceneManager.TransitionOverEventHandler += handler;
    }

    public void Remove(string name)
    {
        if (!_overlayDict.ContainsKey(name))
            throw new Exception("OverlayManager: overlay with that name does not exist");
        var overlay = _overlayDict[name];
        _overlayDict.Remove(name);
        overlay.GetNode().QueueFree();
    }

    public void SetSceneManager(SceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public void ShowAllBut(string overlayName)
    {
        if (!_overlayDict.ContainsKey(overlayName))
            throw new Exception("OverlayManager: overlay with that name does not exist");
        foreach (var (name, overlay) in _overlayDict)
        {
            if (name == overlayName) continue;
            overlay.GetNode().SetVisible(true);
        }
    }

    public void HideAllBut(string overlayName)
    {
        if (!_overlayDict.ContainsKey(overlayName))
            throw new Exception("OverlayManager: overlay with that name does not exist");
        foreach (var (name, overlay) in _overlayDict)
        {
            if (name == overlayName) continue;
            overlay.GetNode().SetVisible(false);
        }
    }

    public void RemoveAll()
    {
        foreach (var (name, overlay) in _overlayDict)
        {
            _overlayDict.Remove(name);
            overlay.GetNode().QueueFree();
        }
    }
}