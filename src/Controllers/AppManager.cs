using BattleshipWithWords.Controllers.SceneManager;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using BattleshipWithWords.Controllers.SceneManager;
using Godot;

namespace BattleshipWithWords.Controllers;
//
// public class AppManager
// {
//     private ScreenManager.ScreenManager _screenManager;
//     // private OverlayManager _overlayManager;
//
//     public AppManager(ScreenManager.ScreenManager screenManager)
//     {
//         _screenManager = screenManager;
//         // _overlayManager = overlayManager;
//     }
//
//     public void PlayTutorial()
//     {
//         _screenManager.StartTransitionToScreen(new TutorialScreen(_screenManager), TransitionDirection.Forward);
//     }
//
//     public void Start()
//     {
//         Logger.Print("app starting"); 
//         // ServerConfig.Environment = "Production";
//         _screenManager.StartTransitionToScreen(new MainMenuScene(_screenManager), TransitionDirection.Forward);
//
//         // var gameManager = new MultiplayerGameManager(1, 1, _sceneManager.GetRoot());
//         // _sceneManager.TransitionTo(new MultiplayerSetupScene(gameManager, _sceneManager, _overlayManager), TransitionDirection.Forward);
//     }
// }