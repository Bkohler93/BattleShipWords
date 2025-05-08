using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

public class TileIdleState : ITileState
{
    private TileController _controller;

    public TileIdleState(TileController controller)
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
        _controller.TransitionTo(new TilePendingState(_controller, hasConflict, isValid, letter, this));
    }
}