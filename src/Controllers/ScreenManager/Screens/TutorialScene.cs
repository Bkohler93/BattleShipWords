using System.Collections.Generic;
using BattleshipWithWords.Controllers.ScreenManager;
using BattleshipWithWords.Controllers.ScreenManager.Screens.Menu.UI;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class TutorialScreen : IScene 
{
    private ScreenManager.ScreenManager _screenManager;
    // private OverlayManager _overlayManager;
    private Tutorial _tutorial;

    public TutorialScreen(ScreenManager.ScreenManager screenManager)
    {
        _screenManager = screenManager; 
        // _overlayManager = overlayManager;
    }


    public Node Create()
    {
        var tutorial = ResourceLoader.Load<PackedScene>(ResourcePaths.TutorialNodePath).Instantiate() as Tutorial;
        tutorial!.OnFinish = () =>
        {
            // _screenManager.TransitionTo(new MainMenuScene(_screenManager, _overlayManager), TransitionDirection.Forward);
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

    public override Control Initialize()
    {
        throw new System.NotImplementedException();
    }

    public override Control GetNode()
    {
        return _tutorial;
    }
}