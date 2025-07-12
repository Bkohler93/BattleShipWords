using System;
using System.Text.Json;
using androidplugintest.ConnectionManager;
using androidplugintest.PeerFinderPlugin;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

public class DiscoveringState : LocalMatchmakingState
{
    private LocalMatchmakingController _controller;
    private LocalMatchmakingState _nextState;
    // private ENetMultiplayerPeer _hostPeer;
    private int _hostPort;
    // private ENetMultiplayerPeer _clientPeer;
    
    public DiscoveringState(LocalMatchmakingController localMatchmakingController)
    {
        _controller = localMatchmakingController;
        // _peerFinder.ConnectSignals(this);
    }

    public override void Enter()
    {
        _controller.PeerFinder.SetListener(this);
        _controller.ConnectionManager.SetListener(this);
    }


    public override void Exit()
    {
    }

    public override void OnBackPressed()
    {
        _controller.DisplayStatus("Disconnecting from search");
        _controller.PeerFinder.Stop();
    }

    public override void OnActionPressed()
    {
        throw new Exception("OnActionPressed while Discovering.. this button should be hidden during this state");
    }

    public override void Receive(Dictionary message)
    {
        var msg = LocalMatchmakingMessage.FromDictionary(message);
        if (msg.Type == LocalMatchmakingMessageType.Request)
        {
            _controller.TransitionTo(new ConfirmingState(_controller, msg.FromName));
        }
    }

    public override void OnStopped()
    {
        _controller.BackToMenu();
    }

    public override void OnStartErrorOccurred(string error)
    {
        _controller.DisplayStatus("Failed to start searching for peers");
    }

    public override void OnStopErrorOccured(string error)
    {
        throw new Exception($"Failed to stop searching for peers - {error}");
    }

    public  override void OnFailedToResolveService(string error)
    {
        GD.PrintErr($"Peer finder failed to resolve service - {error}");
    }

    public override void OnFoundService(ServiceInfo serviceInfo)
    {
        _controller.AddDiscoveredPeer(serviceInfo);
    }

    public override void Connected()
    {
        GD.Print("Peer connected to you");
    }
}