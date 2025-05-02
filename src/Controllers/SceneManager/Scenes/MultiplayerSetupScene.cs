using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.GameManager;
using BattleshipWithWords.Services.Overlays;
using Godot;
using Godot.NativeInterop;

namespace BattleshipWithWords.Services.Scenes;

public class MultiplayerSetupScene : IScene
{
    private OverlayManager _overlayManager;
    private SceneManager _sceneManager;
    private MultiplayerGameManager _gameManager;
    private MultiplayerSetup _node;

    public MultiplayerSetupScene(MultiplayerGameManager gameManager, SceneManager sceneManager, OverlayManager overlayManager)
    {
        _overlayManager = overlayManager;
        _sceneManager = sceneManager;
        _gameManager = gameManager;
    }

    public void Exit(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_node, tween, direction);
    }

    public void Enter(Tween tween, TransitionDirection direction)
    {
        SceneTransitions.MenuEnter(_node, tween, direction);
        
        var pauseOverlay = new PauseOverlay();
        pauseOverlay.SetOnQuit(() =>
        {
            _overlayManager.Pop();
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _overlayManager), TransitionDirection.Backward);
        });
        _overlayManager.Push(pauseOverlay);
    }

    public Node Create()
    {
        _node = ResourceLoader.Load<PackedScene>("res://scenes/games/multiplayer/multiplayer_setup.tscn").Instantiate() as MultiplayerSetup;
        return _node;
    }
}