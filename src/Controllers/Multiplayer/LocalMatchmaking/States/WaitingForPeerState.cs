using System;
using System.Text.Json;
using androidplugintest.ConnectionManager;
using androidplugintest.PeerFinderPlugin;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

public class WaitingForPeerState : LocalMatchmakingState
{
    private LocalMatchmakingController _controller;
    private ServiceInfo _selectedPeerInfo;

    public WaitingForPeerState(LocalMatchmakingController controller)
    {
        _controller = controller;
        _selectedPeerInfo = _controller.GetSelectedPeer();
    }

    public override void Enter()
    {
        _controller.DisplayStatus($"Waiting for {_selectedPeerInfo.PeerName} to accept.");
        _controller.HideActionButton();
    }

    public override void Exit()
    {
    }

    public override void OnBackPressed()
    {
        _controller.PeerFinder.Stop();
        _controller.BackToMenu();
    }

    public override void OnActionPressed()
    {
    }

    public override void ReconnectFailed()
    {
        GD.Print("Reconnect failed during WaitingForPeerState");
        _controller.TransitionTo(new SelectingState(_controller));
    }

    public override void Disconnected()
    {
        GD.Print("Disconnected from peer during WaitingForPeerState");
        _controller.TransitionTo(new SelectingState(_controller));
    }

    public override void Receive(Dictionary message)
    {
        //TODO should receive message accepting or rejecting connection from peer
        var msg = LocalMatchmakingMessage.FromDictionary(message);
        if (msg.Type == LocalMatchmakingMessageType.Response)
        {
            if (msg.Msg == "yes")
            {
                GD.Print("moving to play game with peer");
                _controller.CompleteMatchmaking();
            }
        }
    }

   
    public override void OnFailedToResolveService(string error)
    {
        GD.PrintErr("Failed to resolve service during WaitingForPeerState");
    }

    public override void OnFoundService(ServiceInfo serviceInfo)
    {
        _controller.AddDiscoveredPeer(serviceInfo);
    }
}