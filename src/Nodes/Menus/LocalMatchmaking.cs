using Godot;
using System;
using BattleshipWithWords.Controllers.Multiplayer;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services;
using Godot.Collections;

namespace BattleshipWithWords.Nodes.Menus;

public partial class LocalMatchmaking : Control
{
    [Export]
    public Button BackButton;
    
    [Export]
    public Button PlayButton;

    [Export]    
    public Label StatusLabel;

    [Export] 
    public Label UpdateLabel;

    public Action BackToMainMenu;
    public Action StartGame;
    private readonly LocalMatchmakingController _controller;

    public LocalMatchmaking()
    {
        _controller = new LocalMatchmakingController(this);
    }

    public override void _Ready()
    {
        BackButton.Pressed += _controller.OnBackPressed;
        PlayButton.Pressed += _onPlayButtonPressed;
        PlayButton.Hide();
        _controller.Init();
    }

    public override void _ExitTree()
    {
        _controller.DetachMultiplayerSignals();
    }
    
    private void _onPlayButtonPressed()
    {
        PlayButton.Hide();
        RpcId(_controller.MultiplayerPeerId, MethodName.Rpc_SendPlayReady);
        _controller.OnPlayPressed();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Rpc_SendPlayReady()
    {
        _controller.OnPeerPressedPlay();
    }
}
