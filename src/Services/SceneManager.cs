using System.Runtime.InteropServices;
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

    public void TransitionTo(IScene newScene)
    {
        _currentScene?.Exit();
        if (_currentNode != null)
        {
            _rootNode.RemoveChild(_currentNode);
            _currentNode.QueueFree();
        }
        _currentScene = newScene;
        _currentNode = _currentScene.Enter();
        _rootNode.AddChild(_currentNode);
    }

    public void QuitGame()
    {
        GD.Print("Goodbye =)");
        _rootNode.GetTree().Quit(); 
    }
}

public interface IScene
{
    public void Exit();
    public Node Enter();
}

public class TutorialScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public TutorialScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager; 
        _uiManager = uiManager;
    }
    
    public void Exit()
    {
        GD.Print("exiting tutorial state");
    }

    public Node Enter()
    {
        var tutorial = ResourceLoader.Load<PackedScene>("res://scenes/games/tutorial.tscn").Instantiate() as Tutorial;
        tutorial!.OnFinish = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager));
        };
        return tutorial;
    }
}

public class MainMenuScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public MainMenuScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit()
    {
        GD.Print("Exiting main menu");
    }

    public Node Enter()
    {
        var mainMenu = ResourceLoader.Load<PackedScene>("res://scenes/menus/main_menu.tscn").Instantiate() as MainMenu;
        mainMenu!.OnSinglePlayerButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new SinglePlayerGameScene(_sceneManager, _uiManager));
        };
        mainMenu.OnMultiplayerButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _uiManager));
        };
        mainMenu.OnSettingsButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new SettingsScene(_sceneManager, _uiManager));
        };
        mainMenu.OnQuitButtonPressed = () =>
        {
            _sceneManager.QuitGame();
        };
        return mainMenu;
    }
}

public class SettingsScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public SettingsScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit()
    {
        GD.Print("Exiting settings");
    }

    public Node Enter()
    {
        var settings = ResourceLoader.Load<PackedScene>("res://scenes/menus/settings.tscn").Instantiate() as Settings;
        settings!.OnBackButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager));
        };
        return settings;
    }
}

public class SinglePlayerGameScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;
    public SinglePlayerGameScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit()
    {
        GD.Print("Exiting single player scene");
    }

    public Node Enter()
    {
        var singlePlayerGame = ResourceLoader.Load<PackedScene>("res://scenes/games/single_player_game.tscn").Instantiate() as SinglePlayerGame;
        singlePlayerGame!.OnPause = () =>
        {
            GD.Print("show pause overlay"); 
        };
        singlePlayerGame.OnFinish = () =>
        {
            GD.Print("show finish overlay");
        };
        return singlePlayerGame;
    }
}

public class MultiplayerGameScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public MultiplayerGameScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit()
    {
        GD.Print("Exiting multiplayer scene");
    }

    public Node Enter()
    {
        var multiplayerGame = ResourceLoader.Load<PackedScene>("res://scenes/games/single_player_game.tscn").Instantiate() as SinglePlayerGame;
        multiplayerGame!.OnPause = () =>
        {
        };
        multiplayerGame.OnFinish = () =>
        {
            GD.Print("show finish overlay");
        };
        return multiplayerGame;
    }
}

public class InternetMatchmakingScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public InternetMatchmakingScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit()
    {
        // throw new System.NotImplementedException();
    }

    public Node Enter()
    {
        GD.Print("Entered internet matchmaking state");
        var matchmaking = ResourceLoader.Load<PackedScene>("res://scenes/menus/internet_matchmaking.tscn").Instantiate() as InternetMatchmaking;
        matchmaking!.OnCancelButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager));
        };
            
        return matchmaking;
    }
}

public class MultiplayerMenuScene: IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public MultiplayerMenuScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit()
    {
        GD.Print("Exiting multiplayer menu");
    }

    public Node Enter()
    {
        var multiplayerMenu = ResourceLoader.Load<PackedScene>("res://scenes/menus/multiplayer.tscn").Instantiate() as MultiplayerMenu;
        multiplayerMenu!.OnBackButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MainMenuScene(_sceneManager, _uiManager));
        };
        multiplayerMenu.OnLocalButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new LocalMatchmakingScene(_sceneManager, _uiManager));
        };
        return multiplayerMenu;
    }
}

public class LocalMatchmakingScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public LocalMatchmakingScene(SceneManager sceneManager, UIManager uiManager)
    {
        _sceneManager = sceneManager;
        _uiManager = uiManager;
    }

    public void Exit()
    {
        GD.Print("Exiting local matchmaking scene");
    }

    public Node Enter()
    {
        var localMatchmaking = ResourceLoader.Load<PackedScene>("res://scenes/menus/local_matchmaking.tscn").Instantiate() as LocalMatchmaking;
        localMatchmaking.OnBackButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MultiplayerMenuScene(_sceneManager, _uiManager));
        };
        localMatchmaking.OnPlayButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MultiplayerGameScene(_sceneManager, _uiManager));
        };
        return localMatchmaking;
    }
}