using System.Runtime.InteropServices;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services;

public class SceneManager
{
    private IScene _currentScene;
    private Node _rootNode;
    private Node _currentNode;

    public SceneManager(Node rootNode)
    {
        _rootNode = rootNode; 
    }
    
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
        };
    }

    public void QuitGame()
    {
        _rootNode.GetTree().Quit(); 
    }
}