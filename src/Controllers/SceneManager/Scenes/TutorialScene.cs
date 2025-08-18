using System.Collections.Generic;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class TutorialScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private Tutorial _tutorial;

    public TutorialScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager; 
        _overlayManager = overlayManager;
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
    }

    void IScene.Enter(Tween tween, TransitionDirection direction)
    {
        throw new System.NotImplementedException();
    }

    public Node Create()
    {
        var tutorial = ResourceLoader.Load<PackedScene>(ResourcePaths.TutorialNodePath).Instantiate() as Tutorial;
        tutorial!.OnFinish = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _overlayManager), TransitionDirection.Forward);
        };
        return tutorial;
    }

    public List<Node> GetChildNodesToTransfer()
    {
        return _tutorial.GetNodesToShare();
    }

    public void AddSharedNode(Node node)
    {
        _tutorial.AddNodeToShare(node);
    }

    public Node GetNode()
    {
        return _tutorial;
    }
}