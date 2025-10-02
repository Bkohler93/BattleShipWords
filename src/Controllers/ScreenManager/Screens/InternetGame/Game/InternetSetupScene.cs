using System;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager.Screens.InternetGame.Game;

public class InternetSetupScene : IScene 
{
    private InternetGameScreen _internetGameScreen;
    private PackedScene _packedScene;
    private InternetSetup _internetSetupNode;
    public const string ResourcePath = ResourcePaths.InternetSetupNodePath;
    private bool _isWaiting; 
    
    private Action _bothSetupsCompletedHandler;

    public InternetSetupScene(InternetGameScreen internetGameScreen) 
    {
        _internetGameScreen = internetGameScreen;
    }

    public override Control Initialize()
    {
        if (_packedScene == null)
        {
            EnsureSceneLoaded(ResourcePath); 
            _packedScene = LoadScene(ResourcePath);            
        }
        
        var internetSetup = _packedScene.Instantiate() as InternetSetup;
        internetSetup!.OnSetupComplete = () =>
        {
           _internetGameScreen.StartInternetGame(); 
        };
        internetSetup.OnLocalSetupComplete = () =>
        {
            _internetGameScreen.WaitForOpponentSetup();
        };

        _internetSetupNode = internetSetup;
        return _internetSetupNode;
    }

    public override Control GetNode()
    {
        return _internetSetupNode;
    }
}