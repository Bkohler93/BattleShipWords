using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.ScreenManager;

public record LayerNode(ScreenLayer Layer, Control Node);

public class ScreenManager
{
    private IScreen _currentScreen;

    private Node _backgroundRoot;
    private AppRoot _appRoot;
    private Node _gameRoot;
    private Node _uiRoot;
    private Node _popupRoot;
    
    private SlideTransitionDirection _exitSceneSlideTransitionDirection;
    private Loading _loading;
    
    private readonly Dictionary<ScreenLayer, List<Node>> _activeNodes = new()
    {
        { ScreenLayer.Background, new List<Node>() },
        { ScreenLayer.Game, new List<Node>() },
        { ScreenLayer.UI, new List<Node>() },
        { ScreenLayer.Popup, new List<Node>() }
    };
 
    // private Node _currentNode;
    // private OverlayManager _overlayManager;

    public ScreenManager(AppRoot appRoot, Node backgroundRoot, Node gameRoot, Node uiRoot, Node popupRoot)
    {
        _appRoot = appRoot;
        _backgroundRoot = backgroundRoot;
        _gameRoot = gameRoot;
        _uiRoot = uiRoot;
        _popupRoot = popupRoot;

        _loading = ResourceLoader.Load<PackedScene>(ResourcePaths.LoadingNodePath).Instantiate() as Loading;
    }
    
    public event Action TransitionOverEventHandler;

    private void AddToLayer(ScreenLayer layer, Node node)
    {
        switch (layer)
        {
            case ScreenLayer.Background:
                _backgroundRoot.AddChild(node);
                break;
            case ScreenLayer.Game:
                _gameRoot.AddChild(node);
                break;
            case ScreenLayer.UI:
                _uiRoot.AddChild(node);
                break;
            case ScreenLayer.Popup:
                _popupRoot.AddChild(node);
                break;
        }
        _activeNodes[layer].Add(node);
    }

    private void RemoveFromLayer(ScreenLayer layer, Node node)
    {
        // switch (layer)
        // {
        //     case ScreenLayer.Background:
        //         _backgroundRoot.CallDeferred("queue_free", node);
        //         break;
        //     case ScreenLayer.Game:
        //         _gameRoot.CallDeferred("queue_free", node);
        //         break;
        //     case ScreenLayer.UI:
        //         _uiRoot.CallDeferred("queue_free", node);
        //         break;
        //     case ScreenLayer.Popup:
        //         _popupRoot.CallDeferred("queue_free", node);
        //         break;
        // }
        node.CallDeferred("queue_free");
        _activeNodes[layer].Remove(node);
    }
    

    public List<ScreenNode> TransitionActiveNodesOut(Tween tween, SlideTransitionDirection direction)
    {
        List<ScreenNode> activeNodes = [];
        
        //TODO: handle background differently. May want just a static background the entire lifetime of the app
        // var backgroundNodes = _backgroundRoot.GetChildren();
        // foreach (var node in backgroundNodes)
        // {
        //     if (node is Control controlNode)
        //     {
        //         SceneTransitions.MenuExit(controlNode, tween, direction);
        //         activeNodes.Add(new ScreenNode(controlNode, ScreenLayer.Background));
        //     }
        //     else
        //         Logger.Print("background node is not a control node");
        // }
        
        var uiNodes = _uiRoot.GetChildren();
        foreach (var node in uiNodes)
        {
            if (node is Control controlNode)
            {
                SceneTransitions.SlideExit(controlNode, tween, direction);
                activeNodes.Add(new ScreenNode(controlNode, ScreenLayer.UI));
            } else 
                Logger.Print("ui node is not a control node");
        }
        
        var gameNodes = _gameRoot.GetChildren();
        foreach (var node in gameNodes)
        {
            if (node is Control controlNode)
            {
                SceneTransitions.SlideExit(controlNode, tween, direction);
                activeNodes.Add(new ScreenNode(controlNode, ScreenLayer.Game));
            }
            else
            {
                Logger.Print("game node is not a control node");
            }
        }
        
        var popupNodes = _popupRoot.GetChildren();
        foreach (var node in popupNodes)
        {
            if (node is Control controlNode)
            {
                SceneTransitions.SlideExit(controlNode, tween, direction);
                activeNodes.Add(new ScreenNode(controlNode, ScreenLayer.Popup));
            }
            else
            {
                Logger.Print("popup node is not a control node");
            }
        }
        return activeNodes;
    }


    public void SlideNodesTransition(List<LayerNode> fromNodes, List<LayerNode> toNodes, SlideTransitionDirection direction)
    {
        var tween = _appRoot.GetTree().CreateTween().SetParallel();
        
        foreach (var ln in fromNodes)
        {
            SceneTransitions.SlideExit(ln.Node, tween, direction);
        }

        foreach (var ln in toNodes)
        {
            AddToLayer(ln.Layer, ln.Node);
            SceneTransitions.SlideEnter(ln.Node, tween, direction);
        }
        
        tween.Play();
        tween.Finished += () =>
        {
            foreach (var ln in fromNodes)
            {
                RemoveFromLayer(ln.Layer, ln.Node); 
            }
            TransitionOverEventHandler?.Invoke();
        };
    }

    public void FadeNodesIn(List<LayerNode> toNodes)
    {
        var tween = _appRoot.GetTree().CreateTween().SetParallel();
        foreach (var node in toNodes)
        {
            AddToLayer(node.Layer, node.Node);
            SceneTransitions.FadeIn(node, tween);
        }

        tween.Finished += () =>
        {
            TransitionOverEventHandler?.Invoke();
        };
    }
    
    public void StartTransitionToScreen(IScreen newScreen, SlideTransitionDirection direction) 
    {
        var tween = _appRoot.GetTree().CreateTween().SetParallel();
        var previousActiveNodes = TransitionActiveNodesOut(tween, direction);

        // var previousNodes = _currentScreen?.GetNodes();
        var previousScene = _currentScreen;
        _currentScreen = newScreen;
        _currentScreen.Load();
        
        _loading.RequestReady();
        _uiRoot.AddChild(_loading);
        SceneTransitions.SlideEnter(_loading, tween, direction);
        _exitSceneSlideTransitionDirection = direction;
        
        tween.Play();
        tween.Finished += () =>
        {
            if (previousActiveNodes == null)
            {
                return;
            }
            previousActiveNodes.ForEach(previousNode =>
            {
                RemoveFromLayer(previousNode.Layer, previousNode.Node);
            });
            _appRoot.IsLoading = true;
        };
    }

    public void FinishTransitionToScreen()
    {
        var tween = _appRoot.GetTree().CreateTween().SetParallel();
        var newNodes = _currentScreen.Initialize();
        var direction = _exitSceneSlideTransitionDirection;

        newNodes.ForEach(newNode =>
        {
            AddToLayer(newNode.Layer, newNode.Node);
            SceneTransitions.SlideEnter(newNode.Node, tween, direction);
        });

        tween.Play();
        tween.Finished += () =>
        {
            TransitionOverEventHandler?.Invoke();
        };
    }


    
    // public void TransitionToScreen(Screen newScreen, TransitionDirection direction)
    // {
    //     var tween = _appRoot.GetTree().CreateTween().SetParallel();
    //     _currentScreen?.Exit(tween, direction);
    //     
    //     var previousNodes = _currentScreen?.GetNodes();
    //     var previousScene = _currentScreen;
    //     
    //     _currentScreen = newScreen;
    //     var newNodes = _currentScreen.Load();
    //     // if (currentNode is ISharedNodeReceiver c)
    //     // {
    //     //     var result = c.ReceiveSharedNodes(previousNode);
    //     //     if (!result.Success) 
    //     //         throw new Exception($"{currentNode.GetType().Name} did not receive the nodes it depends on from {previousNode!.GetType().Name}");
    //     // }
    //     newNodes.ForEach(currentNode =>
    //     {
    //         switch (currentNode.Layer)
    //         {
    //             case ScreenLayer.Background:
    //                 _backgroundRoot.AddChild(currentNode.Node);
    //                 break;
    //             case ScreenLayer.Game:
    //                 _gameRoot.AddChild(currentNode.Node);
    //                 break;
    //             case ScreenLayer.UI:
    //                 _uiRoot.AddChild(currentNode.Node);
    //                 break;
    //             case ScreenLayer.Popup:
    //                 _popupRoot.AddChild(currentNode.Node);
    //                 break;
    //             default:
    //                 throw new ArgumentOutOfRangeException();
    //         } 
    //     }); 
    //     // _appRoot.AddChild(currentNode);
    //     
    //     // if (previousNode != null && sharedChildren?.Count > 0)
    //     // {
    //     //     sharedChildren.ForEach(n =>
    //     //     {
    //     //         previousNode.RemoveChild(n);
    //     //         currentNode.AddChild(n);
    //     //         GD.Print($"Adding node to persist across scenes: {n.GetType().Name}");
    //     //         _currentScene.AddSharedNode(n);
    //     //     }); 
    //     // }
    //     previousScene?.Teardown(); 
    //     _currentScreen.Enter(tween, direction);
    //     tween.Play();
    //     tween.Finished += () =>
    //     {
    //         if (previousNodes == null) return;
    //         // not using CallDeferred on Android devices results in touch inputs trying to propagate from 
    //         // already removed child. 
    //         // SEE: https://github.com/godotengine/godot/issues/48607
    //         previousNodes.ForEach(previousNode =>
    //         {
    //             switch (previousNode.Layer)
    //             {
    //                 case ScreenLayer.Background:
    //                     _backgroundRoot.CallDeferred("remove_child", previousNode.Node); 
    //                     break;
    //                 case ScreenLayer.Game:
    //                     _gameRoot.CallDeferred("remove_child", previousNode.Node); 
    //                     break;
    //                 case ScreenLayer.UI:
    //                     _uiRoot.CallDeferred("remove_child", previousNode.Node);
    //                     break;
    //                 case ScreenLayer.Popup:
    //                     _popupRoot.CallDeferred("remove_child", previousNode.Node);
    //                     break;
    //                 default:
    //                     throw new ArgumentOutOfRangeException();
    //             }
    //             previousNode.Node.CallDeferred("queue_free");
    //         });
    //         // _appRoot.CallDeferred("remove_child", previousNodes);
    //         // previousNodes.CallDeferred("queue_free");
    //         TransitionOverEventHandler?.Invoke();
    //     };
    // }
    
    public void QuitGame()
    {
        _appRoot.GetTree().Quit(); 
    }

    public Node GetRoot() => _appRoot;

    // public void SetOverlayManager(OverlayManager overlayManager){
    //     _overlayManager = overlayManager;
    // }

    public void UpdateLoadingProgress()
    {
        Logger.Print("updating loading progress");
        var progress = _currentScreen.GetLoadingProgress();
        if (progress == 100)
        {
            _appRoot.IsLoading = false;
            _loading.UpdateProgress("0%");
           _uiRoot.RemoveChild(_loading); 
           FinishTransitionToScreen();
        }
        else
            _loading.UpdateProgress($"{progress}%");
    }

    public void RemoveNodes(List<LayerNode> layerNodes)
    {
        foreach (var lN in layerNodes)
        {
            RemoveFromLayer(lN.Layer, lN.Node);
        }
    }
}