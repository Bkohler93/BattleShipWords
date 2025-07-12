using androidplugintest.ConnectionManager;
using androidplugintest.PeerFinderPlugin;

namespace BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

public class ErrorState : LocalMatchmakingState
{
    private LocalMatchmakingController _controller;

    public ErrorState(LocalMatchmakingController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.ConnectionManager.SetListener(this);
        _controller.PeerFinder.SetListener(this);
        _controller.HideActionButton();
        _controller.DisplayStatus("Unable to search for local peers."); 
    }

    public override void Exit()
    {
    }

    public override void OnBackPressed()
    {
        _controller.ConnectionManager.Disable();
        if (!_controller.PeerFinder.IsActive)
            _controller.BackToMenu();
        else
        {
            _controller.PeerFinder.Stop();
        }
    }

    public override void OnActionPressed()
    {
        throw new System.NotImplementedException();
    }

    public new void OnStopped()
    {
        _controller.BackToMenu();
    }
}