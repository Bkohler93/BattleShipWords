using System;
using Godot;

namespace BattleshipWithWords.Services.NodeGetter;

public class NodeGetter
{
    private Control _backgroundRoot;
    private Control _gameRoot;
    private Control _uiRoot;
    private Control _popupRoot;
    
    public NodeGetter(Control backgroundRoot, Control gameRoot, Control uiRoot, Control popupRoot)
    {
        _backgroundRoot = backgroundRoot; 
        _gameRoot = gameRoot;
        _uiRoot = uiRoot;
        _popupRoot = popupRoot;
    }
    
    public Control Get<T>() where T : Control
    {
        var backgroundNodes = _backgroundRoot.GetChildren();
        foreach (var backgroundNode in backgroundNodes)
        {
            if (backgroundNode is T returnNode)
                return returnNode;
        }
        
        var gameNodes = _gameRoot.GetChildren();
        foreach (var gameNode in gameNodes)
        {
            if (gameNode is T returnNode)
                return returnNode;
        }
        
        var uiNodes = _uiRoot.GetChildren();
        foreach (var uiNode in uiNodes)
        {
            if (uiNode is T returnNode)
                return returnNode;
        }
       
        var popupNodes = _popupRoot.GetChildren();
        foreach (var popupNode in popupNodes)
        {
            if (popupNode is T returnNode)
                return returnNode;
        }
        throw new Exception($"node of type {typeof(T).FullName} not found in AppRoot");
    }
}