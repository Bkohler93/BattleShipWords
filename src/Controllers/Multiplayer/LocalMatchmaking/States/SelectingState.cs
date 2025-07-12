using System.Text.Json;
using androidplugintest.ConnectionManager;
using androidplugintest.PeerFinderPlugin;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

public class SelectingState : LocalMatchmakingState{
    private LocalMatchmakingController _controller;

    public SelectingState(LocalMatchmakingController controller)
    {
        _controller = controller;
    }


    public override void Enter()
    {
        _controller.ShowActionButton();
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
        var serviceInfo = _controller.GetSelectedPeer();
        var result = _controller.ConnectionManager.ConnectAsClient(serviceInfo.Ip, serviceInfo.Port);
        if (!result.Success)
        {
            GD.PrintErr(result.Error);
            _controller.DisplayStatus($"Failed to connect to peer...");
            //TODO something should happen here if failed to connect
        }
    }

    public override void Connected()
    {
        var msg = new LocalMatchmakingMessage
        {
            FromName = _controller.PeerFinder.DisplayName,
            Type = LocalMatchmakingMessageType.Request,
            Msg = ""
        };
        
        _controller.ConnectionManager.Send(msg);
        _controller.TransitionTo(new WaitingForPeerState(_controller));
    }

    public override void UnableToConnect()
    {
        _controller.DisplayStatus("Unable to connect to peer.");
    }

    public override void Reconnected()
    {
        _controller.DisplayStatus("Reconnected to peer.");
    }

    public override void ReconnectFailed()
    {
        _controller.DisplayStatus("Reconnect failed.");
    }

    public override void Disconnected()
    {
        _controller.DisplayStatus("Disconnected from peer.");
    }

    public override void Receive(Dictionary message)
    {
        //TODO could be a connection request. If connection request, transition to AcceptingPeerState
    }

    public override void OnFailedToResolveService(string error)
    {
        GD.PrintErr($"Failed to resolve service - {error}");
    }

    public override void OnFoundService(ServiceInfo serviceInfo)
    {
        _controller.AddDiscoveredPeer(serviceInfo);
    }
}