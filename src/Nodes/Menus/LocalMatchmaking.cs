using Godot;
using System;
using System.Collections.Generic;
using androidplugintest.ConnectionManager;
using BattleshipWithWords.Controllers;
using BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

namespace BattleshipWithWords.Nodes.Menus;

public partial class LocalMatchmaking : Control, ISceneNode
{
    [Export]
    public Button BackButton;
    
    [Export]
    public Button ActionButton;

    [Export]    
    public Label StatusLabel;

    [Export]
    public ItemList DiscoveredPeers;

    [Export] public Label PeerLabel;

    public Action BackToMainMenu;
    public Action StartGame;
    private readonly LocalMatchmakingController _controller;

    public LocalMatchmaking()
    {
        _controller = new LocalMatchmakingController(this);
    }

    public List<Node> GetNodesToShare() => _nodesToKeepAlive;
    public void AddNodeToShare(Node node) => _nodesToKeepAlive.Add(node);

    public P2PConnectionManager ConnectionManager => _controller.ConnectionManager;
    private ENetP2PPeerService _peerService;
    private readonly List<Node> _nodesToKeepAlive = [];

    public override void _Ready()
    {
        BackButton.Pressed += _controller.OnBackPressed;
        DiscoveredPeers.ItemSelected += index =>
        {
            _controller.SelectItem(index);
        };
        ActionButton.Pressed += _controller.OnActionPressed;
        _peerService = new ENetP2PPeerService();
        AddChild(_peerService, forceReadableName:true);
        _controller.Init(_peerService);
    }

    public void Shutdown()
    {
        _controller.Shutdown();
    }

    public override void _ExitTree()
    {
    }

    public void PersistSharingNodes()
    {
        _nodesToKeepAlive.Add(_peerService);
    }
}
