using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.SceneManager;

public class InternetTurnDeciderScene : IScene
{
    private SceneManager _sceneManager;
    private OverlayManager _overlayManager;
    private InternetTurnDecider _node;

    public InternetTurnDeciderScene(SceneManager sceneManager, OverlayManager overlayManager)
    {
        _sceneManager = sceneManager;
        _overlayManager = overlayManager;
    }

    public void Teardown()
    {
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuExit(_node, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_node, tween, direction);
        var pauseOverlay = new PauseOverlay();
        pauseOverlay.ExitButtonPressed += () =>
        {
            _overlayManager.RemoveAll();
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager),
                TransitionDirection.Backward);
        };
        pauseOverlay.ContinueButtonPressed += () => _overlayManager.ShowAllBut("pause");
        pauseOverlay.PauseButtonPressed += () => _overlayManager.HideAllBut("pause");
        _overlayManager.AddAfterTransition("pause", pauseOverlay, 10);
    }

    public Node Create()
    {
        var internetTurnDecider = ResourceLoader.Load<PackedScene>(ResourcePaths.InternetTurnDeciderNodePath).Instantiate() as InternetTurnDecider;
        _node = internetTurnDecider;
        _node.Init(_overlayManager);
        _node.OnExitGame += () =>
        {
            //TODO: exit game back to multiplayer menu
        };
        // _gameManager.GameLost += ()=>
        // {
        //     var loseOverlay = new LoseOverlay();
        //     loseOverlay.QuitButtonPressed += () =>
        //     {
        //         _gameManager.DisconnectAndFree();
        //         _overlayManager.RemoveAll();
        //         _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager),
        //             TransitionDirection.Backward);
        //     };
        //     _overlayManager.Add("lose", loseOverlay, 5);
        // };
        // _gameManager.GameWon += ()=>
        // {
        //     var winOverlay = new WinOverlay();
        //     winOverlay.QuitButtonPressed += () =>
        //     {
        //         _gameManager.DisconnectAndFree();
        //         _overlayManager.RemoveAll();
        //         _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager),
        //             TransitionDirection.Backward);
        //     };
        //     _overlayManager.Add("win", winOverlay, 5);
        // };
        return _node;
    }

    public Node GetNode()
    {
        return _node;
    }
}