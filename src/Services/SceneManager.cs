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
        tutorial!.TutorialDone = () =>
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
            _sceneManager.TransitionTo(new SinglePlayerScene(_sceneManager, _uiManager));
        };
        mainMenu.OnMultiplayerButtonPressed = () =>
        {
            _sceneManager.TransitionTo(new MultiplayerScene(_sceneManager, _uiManager));
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

public class SinglePlayerScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;
    public SinglePlayerScene(SceneManager sceneManager, UIManager uiManager)
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

public class MultiplayerScene : IScene
{
    private SceneManager _sceneManager;
    private UIManager _uiManager;

    public MultiplayerScene(SceneManager sceneManager, UIManager uiManager)
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
            GD.Print("show pause overlay");
        };
        multiplayerGame.OnFinish = () =>
        {
            GD.Print("show finish overlay");
        };
        return multiplayerGame;
    }
}