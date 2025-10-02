using System;
using System.Collections.Generic;
using androidplugintest.ConnectionManager;
using BattleshipWithWords.Controllers.ScreenManager.Screens.Menu;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;
using Array = Godot.Collections.Array;

namespace BattleshipWithWords.Controllers.ScreenManager;

public abstract class Screen<T>:IScreen where T : Enum
{
    protected Dictionary<T, IScene> _scenes = new();
    protected List<string> _resourcePaths = new();
    
    public int GetLoadingProgress()
    {
        Array progress = [];
        double totalProgress = 0;
        int numResources = 0;
        ResourceLoader.ThreadLoadStatus overallLoadStatus = ResourceLoader.ThreadLoadStatus.Loaded;

        foreach (string resourcePath in _resourcePaths)
        {
            CheckNodeLoading(resourcePath);
        }

        var overallLoadingProgress = (int)(totalProgress / numResources);
        Logger.Print($"overall loading progress with {overallLoadStatus}: {overallLoadingProgress}");
        
        return overallLoadingProgress;

        void CheckNodeLoading(string resourcePath)
        {
            var loadStatus = ResourceLoader.LoadThreadedGetStatus(resourcePath, progress);
            if (loadStatus != ResourceLoader.ThreadLoadStatus.Loaded)
            {
                overallLoadStatus = loadStatus;
                if (loadStatus != ResourceLoader.ThreadLoadStatus.InProgress)
                {
                    Logger.Print($"Invalid load status when trying to check load status for {resourcePath} - {loadStatus}");
                }
            }
            Logger.Print($"load status for {resourcePath}: {loadStatus}[{progress[0]}]");
            numResources++;
            totalProgress += (double)progress[0] * 100;
        }
    }

    /// <summary>
    /// Initialize instantiates a Screens nodes using _scenes, populating _initializedNodes after instantiating a scene,
    /// and returns a List of ScreenNodes for each Node that will be present after loading is complete.
    /// Must be called after Load. 
    /// </summary>
    /// <returns></returns>
    public abstract List<ScreenNode> Initialize();
    // public void Exit(Tween tween, TransitionDirection direction);
    // public void Enter(Tween tween, TransitionDirection direction);
   
    /// <summary>
    /// Load creates the Scenes for each resource a screen needs, stores the resource paths for each Node,
    /// and calls ResourceLoader.LoadThreadedRequest for each resource path to begin loading resources.
    /// </summary>
    public abstract void Load();
}

public interface IScreen
{
    public int GetLoadingProgress();
    /// <summary>
    /// Initialize instantiates a Screens nodes using _scenes, populating _initializedNodes after instantiating a scene,
    /// and returns a List of ScreenNodes for each Node that will be present after loading is complete.
    /// Must be called after Load. 
    /// </summary>
    /// <returns></returns>
    public List<ScreenNode> Initialize();
    // public void Exit(Tween tween, TransitionDirection direction);
    // public void Enter(Tween tween, TransitionDirection direction);
   
    /// <summary>
    /// Load creates the Scenes for each resource a screen needs, stores the resource paths for each Node,
    /// and calls ResourceLoader.LoadThreadedRequest for each resource path to begin loading resources.
    /// </summary>
    public void Load();
}

public abstract class IScene
{
    protected static void EnsureSceneLoaded(string resourcePath)
    {
        var loadStatus = ResourceLoader.LoadThreadedGetStatus(resourcePath);
        Logger.Print($"Ensuring resource at path {resourcePath} is loaded: status={loadStatus}");
        if (loadStatus != ResourceLoader.ThreadLoadStatus.Loaded)
        {
            throw new Exception($"resource at {resourcePath} is not loaded");
        }
    }

    protected static PackedScene LoadScene(string resourcePath)
    {
        var packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(resourcePath);
        if (packedScene == null)
            throw new Exception($"failed to load resource at {resourcePath}");
        return packedScene;
       // var node = ((PackedScene)ResourceLoader.LoadThreadedGet(resourcePath)).Instantiate() as T;
       // if (node == null)
       // {
       //     throw new Exception($"failed to load resource at {resourcePath}");
       // }
       //
       // return node;
    }
    
    /// <summary>
    /// Initialize starts by checking if the Scene is loaded into memory by calling ResourceLoader.LoadThreadedGetStatus.
    /// If the status is Loaded, then the resource is retrieved with ResourceLoader.LoadThreadedGet and then the Scene's
    /// Node is instantiated, signals hooked up, and the underlying Node stored within the Scene.
    /// </summary>
    public abstract Control Initialize();
    public abstract Control GetNode();
}

public enum ScreenLayer
{
    Background,
    Game,
    UI,
    Popup
}

public record ScreenNode(Control Node, ScreenLayer Layer);

public interface ISharedNodeReceiver
{
    public Result ReceiveSharedNodes(Node node);
}


// public interface ISceneNode
// {
//     public List<Node> GetNodesToShare();
//     public void AddNodeToShare(Node node);
// }