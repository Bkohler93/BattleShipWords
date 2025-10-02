using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.InternetGame;

public class InternetGameplayScene : IScene
{
    private InternetGameScreen _screen;
    private Nodes.Game.InternetGame _node;
    private PackedScene _packedScene;

    public InternetGameplayScene(InternetGameScreen screen)
    {
        _screen = screen;
    }

    public static string ResourcePath = ResourcePaths.InternetGameNodePath; 

    public void Teardown()
    {
    }

    public void Exit(Tween tween, SlideTransitionDirection direction)
    {
        SceneTransitions.SlideExit(_node, tween, direction);
    }

    // public void Enter(Tween tween, SlideTransitionDirection direction)
    // {
    //     SceneTransitions.SlideEnter(_node, tween, direction);
    //     var pauseOverlay = new PauseOverlay();
    //     pauseOverlay.ExitButtonPressed += () =>
    //     {
    //         _overlayManager.RemoveAll();
    //         // _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager),
    //         //     TransitionDirection.Backward);
    //     };
    //     pauseOverlay.ContinueButtonPressed += () => _overlayManager.ShowAllBut("pause");
    //     pauseOverlay.PauseButtonPressed += () => _overlayManager.HideAllBut("pause");
    //     _overlayManager.AddAfterTransition("pause", pauseOverlay, 10);
    // }

    public Node Create()
    {
        // var internetGame = ResourceLoader.Load<PackedScene>(ResourcePaths.InternetGameNodePath).Instantiate() as InternetGame;
        // _node = internetGame;
        // _node.Init(_overlayManager);
        // _node.GameLost += ()=>
        // {
        //     var loseOverlay = new LoseOverlay();
        //     loseOverlay.QuitButtonPressed += () =>
        //     {
        //         _node.ConnectionManager.Close();
        //         _overlayManager.RemoveAll();
        //         _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager),
        //             TransitionDirection.Backward);
        //     };
        //     _overlayManager.Add("lose", loseOverlay, 5);
        // };
        // _node.GameWon += ()=>
        // {
        //     var winOverlay = new WinOverlay();
        //     winOverlay.QuitButtonPressed += () =>
        //     {
        //         _node.ConnectionManager.Close();
        //         _overlayManager.RemoveAll();
        //         _screenManager.TransitionTo(new MultiplayerMenuScreen(_screenManager, _overlayManager),
        //             TransitionDirection.Backward);
        //     };
        //     _overlayManager.Add("win", winOverlay, 5);
        // };
        
        
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

    public override Control Initialize()
    {
        if (_packedScene == null)
        {
            EnsureSceneLoaded(ResourcePath); 
            _packedScene = LoadScene(ResourcePath);            
        }
        
        _node = _packedScene.Instantiate() as Nodes.Game.InternetGame;
        _node!.GameWon += () =>
        {
            _screen.ShowWinPopup();
        };
        _node.GameLost += () =>
        {
            _screen.ShowLossPopup();
        };
        
        return _node;
    }

    public override Control GetNode()
    {
        return _node;
    }
}