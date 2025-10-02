using System;
using System.Collections.Generic;
using Godot;
using BattleshipWithWords;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Controllers.SceneManager;
using BattleshipWithWords.Controllers.ScreenManager;
using BattleshipWithWords.Controllers.ScreenManager.Screens.Menu;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Services.NodeGetter;
using BattleshipWithWords.Services.SharedData;
using BattleshipWithWords.Utilities;

public class ServiceContainer
{
    private readonly Dictionary<Type, Func<object>> _factories = new();
    private readonly Dictionary<Type, object> _instances = new();
    
    public void RegisterService<T>(Func<T> factory) where T : class 
    {
        _factories[typeof(T)] = factory;
    }
    public T InitializeService<T>() where T: class 
    {
        if (_instances.ContainsKey(typeof(T)))
            throw new Exception($"Service type {typeof(T)} already exists");
        var service = _factories[typeof(T)]() as T;
        _instances.Add(typeof(T), service);
        return service;
    }

    public T RetrieveService<T>() where T : class 
    {
        if (_instances.TryGetValue(typeof(T), out var service))
            return service as T;
        throw new Exception($"Service type {typeof(T)} does not exist");
    }

    public void StopService<T>()
    {
        if (_instances.TryGetValue(typeof(T), out var service))
        {
            _instances.Remove(typeof(T));
            if (service is Node nodeService) 
                nodeService.QueueFree();
            return;
        }
        throw new Exception($"Service type {typeof(T)} does not exist");
    }
}

public partial class AppRoot : Control
{
    public static ServiceContainer Services { get; } = new ServiceContainer();
    private string _configPath = "user://config.cfg";
    private ConfigFile _configFile;
    // private AppManager _appManager;
    [Export] private Control _backgroundRoot;
    [Export]  private Control _gameRoot;
    [Export] private Control _uiRoot;
    [Export] private Control _popupRoot;
    private ScreenManager _screenManager;

    public bool IsLoading { get; set; }

    public override void _Ready()
    {
        Logger.Print($"number of children on background root: {_backgroundRoot.GetChildCount()}");
        Services.RegisterService(() =>
        {
            var serverConnectionManager = new ServerConnectionManager();
            AddChild(serverConnectionManager);
            return serverConnectionManager;
        });
        Services.RegisterService(() =>
        {
            var sharedData = new SharedData();
            return sharedData;
        });
        Services.InitializeService<SharedData>();
        Services.RegisterService(() =>
        {
            var nodeGetter = new NodeGetter(_backgroundRoot, _gameRoot, _uiRoot, _popupRoot);
            return nodeGetter;
        });
        Services.InitializeService<NodeGetter>();
        Logger.Print("starting app");
        var config = GetNode("/root/Config") as Config;
        config.ParseConfig(OS.GetCmdlineArgs()); 
        
         _screenManager = new ScreenManager(this, _backgroundRoot, _gameRoot, _uiRoot, _popupRoot);
        // var overlayManager = new OverlayManager(_uiRoot);
        // sceneManager.SetOverlayManager(overlayManager);
        // overlayManager.SetSceneManager(sceneManager);
        // _appManager = new AppManager(sceneManager);
        
        if (ShouldShowTutorial())
            Logger.Print("show tutorial");
        // screenManager.TransitionTo(new TutorialScreen(screenManager), TransitionDirection.Forward); 
        else
        {
           Logger.Print("transitioning to Menu Screen"); 
            _screenManager.StartTransitionToScreen(new MenuScreen(_screenManager), SlideTransitionDirection.Forward);
        }
        // _appManager.Start();
    }

    public override void _Process(double delta)
    {
        if (IsLoading)
        {
            _screenManager.UpdateLoadingProgress();
        }
    }

    private bool ShouldShowTutorial()
    {
        _configFile = new ConfigFile();
        _configFile.Load(_configPath);

        bool isFirstLaunch = (bool)_configFile.GetValue("settings", "first_launch", true);
        if (isFirstLaunch)
        {
            _configFile.SetValue("settings", "first_launch", false);
        }

        _configFile.Save(_configPath);
        return isFirstLaunch;
    }
}
