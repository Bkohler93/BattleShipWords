using androidplugintest.ConnectionManager;
using androidplugintest.PeerFinderPlugin;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

public class InitialState: LocalMatchmakingState
{
    private LocalMatchmakingController _controller;

    public InitialState(LocalMatchmakingController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.ConnectionManager.SetListener(this);
        _controller.PeerFinder.SetListener(this);
        _controller.HideActionButton();
        _controller.HidePeerLabel();
        
        var result = _controller.ConnectionManager.CreateServer();
        if (!result.Success)
        {
            GD.PrintErr($"Failed to create server - {result.Error}");
            _controller.TransitionTo(new ErrorState(_controller));
            return;
        }

        var port = result.Value;
        
        _controller.PeerFinder.Start("TitFucker", "word-battle", port);
    }

    public override void Exit()
    {
    }

    public override void OnBackPressed()
    {
        _controller.ConnectionManager.Disable();
        _controller.PeerFinder.Stop();
    }

    public override void OnActionPressed()
    {
        throw new ("InitialState: Action button should be shown");
    }

    public override void OnStarted()
    {
        _controller.TransitionTo(new DiscoveringState(_controller));
    }

    public override void OnFoundService(ServiceInfo serviceInfo)
    {
        _controller.AddDiscoveredPeer(serviceInfo);
    }
}