using System.Text.Json;
using androidplugintest.ConnectionManager;
using androidplugintest.PeerFinderPlugin;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

public class ConfirmingState: LocalMatchmakingState
{
    private LocalMatchmakingController _controller;
    private string _peerName;

    public ConfirmingState(LocalMatchmakingController controller, string peerName)
    {
        _controller = controller;
        _peerName = peerName;
    }

    public override void Enter()
    {
        _controller.ConnectionManager.SetListener(this);
        _controller.PeerFinder.SetListener(this);
       _controller.DisplayStatus("Peer connected");
       _controller.ShowAndUpdatePeerLabel(_peerName);
       _controller.ShowAction("Accept");
    }

    public override void Exit()
    {
    }

    public override void OnBackPressed()
    {
        _controller.ConnectionManager.Disconnect();
        _controller.PeerFinder.Stop();
    }

    public override void OnActionPressed()
    {
        var res = new LocalMatchmakingMessage
        {
            FromName = _controller.PeerFinder.DisplayName,
            Type = LocalMatchmakingMessageType.Response,
            Msg = "yes"
        };
        _controller.ConnectionManager.Send(res);
        GD.Print("ConfirmingState::OnActionPressed sent yes");
        _controller.CompleteMatchmaking();
    }

    public override void OnStopped()
    {
        _controller.BackToMenu();
    }

    public override void OnFoundService(ServiceInfo serviceInfo)
    {
        _controller.AddDiscoveredPeer(serviceInfo);
    }
}