using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

public class IdleState : ITileState
{
    private TileController _controller;

    public IdleState(TileController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.DisplayStyleBox("idle");
    }

    public override void Exit()
    {
    }

    // public override void Select(bool hasConflict)
    // {
    //     _controller.TransitionTo(new SelectedState(_controller, hasConflict));
    // }

    public override void Predict(bool hasConflict, bool isValid, string letter)
    {
        _controller.TransitionTo(new PendingState(_controller, hasConflict, isValid, letter, this));
    }
}