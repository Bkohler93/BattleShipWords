using Godot;
using System;
using BattleshipWithWords;
using BattleshipWithWords.Services;

public partial class AppRoot : Control
{
    private string _configPath = "user://config.cfg";
    private ConfigFile _configFile;
    private AppManager _appManager;
    public override void _Ready()
    {
        var config = GetNode("/root/Config") as Config;
        config.ParseConfig(OS.GetCmdlineArgs()); 
        
        var sceneRoot = GetNode("SceneRoot");
        var uiRoot = GetNode("UIRoot");
        _appManager = new AppManager(new SceneManager(sceneRoot), new UIManager(uiRoot));
        
        if (ShouldShowTutorial())
            _appManager.PlayTutorial();
        else
            _appManager.Start();
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
