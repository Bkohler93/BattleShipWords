using System;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers;

public class SceneManager
{
    private IScene _currentScene;
    private Node _rootNode;
    // private Node _currentNode;
    private OverlayManager _overlayManager;

    public SceneManager(Node rootNode)
    {
        _rootNode = rootNode; 
    }
    
    public event Action TransitionOverEventHandler;
    
    public void TransitionTo(IScene newScene, TransitionDirection direction)
    {
        var tween = _rootNode.GetTree().CreateTween().SetParallel();
        _currentScene?.Exit(tween, direction);
        
        var sharedChildren = _currentScene?.GetChildNodesToTransfer();
        var previousNode = _currentScene?.GetNode();
        
        _currentScene = newScene;
        var currentNode = _currentScene.Create();
        _rootNode.AddChild(currentNode);
        
        if (previousNode != null && sharedChildren?.Count > 0)
        {
            sharedChildren.ForEach(n =>
            {
                previousNode.RemoveChild(n);
                currentNode.AddChild(n);
                GD.Print($"Adding node to persist across scenes: {n.GetType().Name}");
                _currentScene.AddSharedNode(n);
            }); 
        }
        
        _currentScene.Enter(tween, direction);
        tween.Play();
        tween.Finished += () =>
        {
            if (previousNode == null) return;
            // not using CallDeferred on Android devices results in touch inputs trying to propagate from 
            // already removed child. 
            // SEE: https://github.com/godotengine/godot/issues/48607
            _rootNode.CallDeferred("remove_child", previousNode);
            previousNode.CallDeferred("queue_free");
            TransitionOverEventHandler?.Invoke();
        };
    }
    
    public void QuitGame()
    {
        _rootNode.GetTree().Quit(); 
    }

    public Node GetRoot() => _rootNode;

    public void SetOverlayManager(OverlayManager overlayManager){
        _overlayManager = overlayManager;
    }

    // public void HookPeerDisconnected(MultiplayerGameManager gameManager)
    // {
    //     MultiplayerApi.PeerDisconnectedEventHandler onPeerDisconnected = null;
    //     onPeerDisconnected = (long id) =>
    //     {
    //         gameManager.PeerDisconnected -= onPeerDisconnected;
    //         var peerDisconnectedOverlay = new PeerDisconnectedOverlay();
    //         Action onExitedOverlay = null;
    //         onExitedOverlay = () =>
    //         {
    //             gameManager.DisconnectAndFree();
    //             peerDisconnectedOverlay.ExitedOverlay -= onExitedOverlay;
    //             _overlayManager.RemoveAll();
    //             TransitionTo(new MultiplayerMenuScene(this, _overlayManager), TransitionDirection.Backward);
    //         };
    //         peerDisconnectedOverlay.ExitedOverlay += onExitedOverlay;
    //         _overlayManager.Add("peer_disconnected", peerDisconnectedOverlay, 4);
    //         _overlayManager.HideAllBut("peer_disconnected");
    //     };
    //     gameManager.PeerDisconnected += onPeerDisconnected;
    // }
}