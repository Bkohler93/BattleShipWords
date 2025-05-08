using System;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Controllers;

public class SceneManager
{
    private IScene _currentScene;
    private Node _rootNode;
    private Node _currentNode;
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
        _currentScene = newScene;
        var _previousNode = _currentNode;
        _currentNode = _currentScene.Create();
        _rootNode.AddChild(_currentNode);
        _currentScene.Enter(tween, direction);
        tween.Play();
        tween.Finished += () =>
        {
            if (_previousNode == null) return;
            // not using CallDeferred on Android devices results in touch inputs trying to propagate from 
            // already removed child. 
            // SEE: https://github.com/godotengine/godot/issues/48607
            _rootNode.CallDeferred("remove_child", _previousNode);
            _previousNode.Dispose();
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

    public void HookPeerDisconnected(MultiplayerGameManager gameManager)
    {
        MultiplayerApi.PeerDisconnectedEventHandler onPeerDisconnected = null;
        onPeerDisconnected = (long id) =>
        {
            gameManager.PeerDisconnected -= onPeerDisconnected;
            var peerDisconnectedOverlay = new PeerDisconnectedOverlay();
            Action onExitedOverlay = null;
            onExitedOverlay = () =>
            {
                gameManager.DisconnectAndFree();
                peerDisconnectedOverlay.ExitedOverlay -= onExitedOverlay;
                _overlayManager.RemoveAll();
                TransitionTo(new MultiplayerMenuScene(this, _overlayManager), TransitionDirection.Backward);
            };
            peerDisconnectedOverlay.ExitedOverlay += onExitedOverlay;
            _overlayManager.Add("peer_disconnected", peerDisconnectedOverlay, 4);
            _overlayManager.HideAllBut("peer_disconnected");
        };
        gameManager.PeerDisconnected += onPeerDisconnected;
    }
}