using BattleshipWithWords.Services.Multiplayer.Setup;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

public class PlacedState: ITileState
{
    private TileController _controller;

    public PlacedState(TileController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        //TODO display Placed stylebox
        _controller.DisplayStyleBox("placed");
    }

    public override void Exit()
    {
    }

    public override void SetPlaceable()
    {
        _controller.TransitionTo(new TilePlaceableState(_controller, this));
    }

    public override void Predict(bool hasConflict, bool isValid, string letter)
    {
        _controller.TransitionTo(new TilePendingState(_controller, hasConflict, isValid,letter, this));
    }
}